using WerkPilot.Application.Customers;
using WerkPilot.Application.Documents;
using WerkPilot.Application.Materials;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Projects;
using WerkPilot.Domain.Documents;

namespace WerkPilot.Application.Search;

public sealed class GlobalSearchService(
    CustomerService customers,
    OfferService offers,
    ProjectService projects,
    MaterialService materials,
    DocumentService documents)
{
    public async Task<IReadOnlyList<GlobalSearchResult>> SearchAsync(
        string? searchText,
        int maximumResults = 40,
        CancellationToken cancellationToken = default)
    {
        var term = searchText?.Trim();
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return [];

        var normalized = term.ToUpperInvariant();
        var results = new List<GlobalSearchResult>();

        var customerItems = await customers.SearchAsync(term, includeDeleted: false, cancellationToken);
        results.AddRange(customerItems.Select(x => new GlobalSearchResult(
            SearchResultType.Customer,
            x.Id,
            x.DisplayName,
            Join(x.BillingPostalCode, x.BillingCity, x.Email),
            x.CustomerNumber,
            "Kunden",
            Score(normalized, x.CustomerNumber, x.DisplayName, x.Email, x.BillingCity))));

        var offerItems = await offers.GetAllAsync(cancellationToken);
        results.AddRange(offerItems
            .Where(x => Contains(normalized, x.OfferNumber, x.Title, x.Status.ToString()))
            .Select(x => new GlobalSearchResult(
                SearchResultType.Offer,
                x.Id,
                x.Title,
                $"{x.Status} · {x.GrossTotal:N2} €",
                x.OfferNumber,
                "Angebote",
                Score(normalized, x.OfferNumber, x.Title, x.Status.ToString()))));

        var projectItems = await projects.GetAllAsync(cancellationToken);
        results.AddRange(projectItems
            .Where(x => Contains(normalized, x.ProjectNumber, x.Title, x.ProjectManager, x.Status.ToString()))
            .Select(x => new GlobalSearchResult(
                SearchResultType.Project,
                x.Id,
                x.Title,
                $"{x.Status} · {x.ProgressPercent}% · {x.OpenTaskCount} offen",
                x.ProjectNumber,
                "Projekte",
                Score(normalized, x.ProjectNumber, x.Title, x.ProjectManager, x.Status.ToString()))));

        var materialItems = await materials.SearchAsync(term, includeInactive: true, cancellationToken);
        results.AddRange(materialItems.Select(x => new GlobalSearchResult(
            SearchResultType.Material,
            x.Id,
            x.Description,
            Join(x.Supplier, x.Unit, $"{x.PurchasePrice:N4} €"),
            x.ArticleNumber,
            "Materialstamm",
            Score(normalized, x.ArticleNumber, x.Description, x.Supplier, x.SupplierArticleNumber))));

        foreach (var project in projectItems)
        {
            var files = await documents.GetFilesAsync(
                DocumentOwnerType.Project,
                project.Id,
                includeDeleted: false,
                cancellationToken);

            results.AddRange(files
                .Where(x => Contains(normalized, x.DisplayName, x.ContentType, project.ProjectNumber, project.Title))
                .Select(x => new GlobalSearchResult(
                    SearchResultType.Document,
                    x.Id,
                    x.DisplayName,
                    $"{project.ProjectNumber} · {project.Title}",
                    project.ProjectNumber,
                    "Dokumente",
                    Score(normalized, x.DisplayName, x.ContentType, project.ProjectNumber, project.Title))));
        }

        return results
            .OrderByDescending(x => x.Relevance)
            .ThenBy(x => x.Type)
            .ThenBy(x => x.PrimaryText)
            .Take(Math.Clamp(maximumResults, 1, 200))
            .ToArray();
    }

    private static bool Contains(string term, params string?[] values) =>
        values.Any(value => value?.Contains(term, StringComparison.OrdinalIgnoreCase) == true);

    private static int Score(string term, params string?[] values)
    {
        var score = 0;
        foreach (var value in values.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var candidate = value!.ToUpperInvariant();
            if (candidate == term) score = Math.Max(score, 100);
            else if (candidate.StartsWith(term, StringComparison.Ordinal)) score = Math.Max(score, 80);
            else if (candidate.Contains(term, StringComparison.Ordinal)) score = Math.Max(score, 50);
        }
        return score;
    }

    private static string Join(params string?[] values) =>
        string.Join(" · ", values.Where(x => !string.IsNullOrWhiteSpace(x)));
}
