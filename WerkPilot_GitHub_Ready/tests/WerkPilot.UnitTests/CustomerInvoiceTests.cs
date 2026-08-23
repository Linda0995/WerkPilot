using WerkPilot.Domain.Billing;

namespace WerkPilot.UnitTests;

public sealed class CustomerInvoiceTests
{
    [Fact]
    public void PartialPayment_SetsPartiallyPaid()
    {
        var invoice = CreateIssuedInvoice();

        invoice.RegisterPayment(
            60m,
            new DateOnly(2026, 8, 5),
            "BANK-1",
            "Max");

        Assert.Equal(CustomerInvoiceStatus.PartiallyPaid, invoice.Status);
        Assert.Equal(60m, invoice.PaidAmount);
        Assert.Equal(60m, invoice.OpenAmount);
    }

    [Fact]
    public void FullPayment_SetsPaid()
    {
        var invoice = CreateIssuedInvoice();

        invoice.RegisterPayment(
            120m,
            new DateOnly(2026, 8, 5),
            "BANK-1",
            "Max");

        Assert.Equal(CustomerInvoiceStatus.Paid, invoice.Status);
        Assert.Equal(0m, invoice.OpenAmount);
    }

    [Fact]
    public void OverdueInvoice_AdvancesDunning()
    {
        var invoice = CreateIssuedInvoice();

        invoice.AdvanceDunning(new DateOnly(2026, 8, 20));

        Assert.Equal(DunningLevel.Reminder, invoice.DunningLevel);
        Assert.Equal(new DateOnly(2026, 8, 20), invoice.LastDunningDate);
    }

    private static CustomerInvoice CreateIssuedInvoice()
    {
        var invoice = new CustomerInvoice(
            "RE-2026-0001",
            Guid.NewGuid(),
            "Muster GmbH",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 10),
            null,
            null,
            "Max");

        invoice.AddLine(
            "Leistung",
            1m,
            "Pauschal",
            100m,
            20m);

        invoice.Issue();
        return invoice;
    }
}
