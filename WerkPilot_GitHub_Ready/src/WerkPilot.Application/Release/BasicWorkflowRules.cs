using WerkPilot.Domain.Billing;
using WerkPilot.Domain.Offers;
using WerkPilot.Domain.Projects;

namespace WerkPilot.Application.Release;

public static class BasicWorkflowRules
{
    public static string ResolveStage(
        OfferStatus offerStatus,
        bool hasCalculation,
        ProjectStatus? projectStatus,
        CustomerInvoiceStatus? invoiceStatus,
        decimal invoiceOpenAmount,
        bool hasDunning)
    {
        if (invoiceStatus == CustomerInvoiceStatus.Paid
            || (invoiceStatus is CustomerInvoiceStatus.Issued
                or CustomerInvoiceStatus.PartiallyPaid
                && invoiceOpenAmount <= 0m))
        {
            return "Abgeschlossen";
        }

        if (hasDunning)
            return "Mahnung";

        if (invoiceStatus == CustomerInvoiceStatus.PartiallyPaid)
            return "Teilzahlung";

        if (invoiceStatus == CustomerInvoiceStatus.Issued)
            return "Rechnung offen";

        if (invoiceStatus == CustomerInvoiceStatus.Draft)
            return "Rechnung Entwurf";

        if (projectStatus is ProjectStatus.Planned
            or ProjectStatus.Active
            or ProjectStatus.OnHold
            or ProjectStatus.Completed)
        {
            return "Projekt";
        }

        if (offerStatus == OfferStatus.Accepted)
            return hasCalculation ? "Angebot angenommen" : "Kalkulation fehlt";

        if (offerStatus == OfferStatus.Sent)
            return "Angebot versendet";

        if (hasCalculation)
            return "Kalkulation";

        return "Angebot";
    }

    public static int CompletionPercent(
        bool hasOffer,
        bool hasCalculation,
        bool accepted,
        bool hasProject,
        bool hasInvoice,
        bool issued,
        bool hasPayment)
    {
        var steps = new[]
        {
            hasOffer,
            hasCalculation,
            accepted,
            hasProject,
            hasInvoice,
            issued,
            hasPayment
        };

        return (int)Math.Round(
            steps.Count(x => x) * 100m / steps.Length,
            MidpointRounding.AwayFromZero);
    }
}
