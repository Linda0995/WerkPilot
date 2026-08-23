using WerkPilot.Application.Auditing;
using WerkPilot.Domain.Settings;

namespace WerkPilot.Application.Settings;

public sealed class CompanyProfileService(
    ICompanyProfileRepository repository,
    IAuditTrail auditTrail)
{
    public async Task<CompanyProfileDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var profile = await repository.GetAsync(cancellationToken);
        if (profile is null)
        {
            profile = new CompanyProfile("WerkPilot Musterbetrieb");
            await repository.AddAsync(profile, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
        }

        return Map(profile);
    }

    public async Task<CompanyProfileDto> UpdateAsync(
        UpdateCompanyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = await repository.GetAsync(cancellationToken)
            ?? new CompanyProfile(request.CompanyName);

        profile.UpdateCompany(
            request.CompanyName,
            request.Street,
            request.PostalCode,
            request.City,
            request.CountryCode,
            request.Email,
            request.Phone,
            request.VatId,
            request.Website);

        profile.UpdateOfferTexts(
            request.OfferIntroText,
            request.OfferClosingText,
            request.CurrencyCode);

        profile.UpdateOfferEmailTemplate(
            request.OfferEmailSubjectTemplate,
            request.OfferEmailBodyTemplate);

        if (profile.Id == Guid.Empty)
            await repository.AddAsync(profile, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            "CompanyProfile",
            profile.Id,
            "Updated",
            "Firmenstammdaten und Angebotsvorlage wurden aktualisiert.",
            cancellationToken);

        return Map(profile);
    }

    private static CompanyProfileDto Map(CompanyProfile x) => new(
        x.Id,
        x.CompanyName,
        x.Street,
        x.PostalCode,
        x.City,
        x.CountryCode,
        x.Email,
        x.Phone,
        x.VatId,
        x.Website,
        x.OfferIntroText,
        x.OfferClosingText,
        x.CurrencyCode,
        x.OfferEmailSubjectTemplate,
        x.OfferEmailBodyTemplate);
}
