using WerkPilot.Domain.Purchasing;

namespace WerkPilot.UnitTests;

public sealed class SupplierInvoicePaymentTests
{
    [Fact]
    public void PartialPayment_ReducesOpenAmount()
    {
        var invoice = CreateApprovedInvoice();

        invoice.RegisterPayment(
            30m,
            new DateOnly(2026, 8, 5),
            "BANK-1",
            "Max");

        Assert.Equal(30m, invoice.PaidAmount);
        Assert.Equal(70m, invoice.OpenAmount);
        Assert.Equal(SupplierInvoiceStatus.Approved, invoice.Status);
    }

    [Fact]
    public void FullPayment_SetsPaidStatus()
    {
        var invoice = CreateApprovedInvoice();

        invoice.RegisterPayment(
            100m,
            new DateOnly(2026, 8, 5),
            "BANK-1",
            "Max");

        Assert.True(invoice.IsFullyPaid);
        Assert.Equal(SupplierInvoiceStatus.Paid, invoice.Status);
        Assert.NotNull(invoice.PaidAtUtc);
    }

    [Fact]
    public void PaymentAboveOpenAmount_Throws()
    {
        var invoice = CreateApprovedInvoice();

        Assert.Throws<InvalidOperationException>(() =>
            invoice.RegisterPayment(
                101m,
                new DateOnly(2026, 8, 5),
                null,
                "Max"));
    }

    private static SupplierInvoice CreateApprovedInvoice()
    {
        var invoice = new SupplierInvoice(
            "RE-100",
            Guid.NewGuid(),
            "Stahl GmbH",
            new DateOnly(2026, 8, 2),
            new DateOnly(2026, 8, 16),
            "Max",
            2m,
            new DateOnly(2026, 8, 10));

        invoice.AddLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "MAT-001",
            "Stahlblech",
            10m,
            10m);

        invoice.SubmitForReview();
        invoice.Approve("Max", null);
        return invoice;
    }
}
