using WerkPilot.Domain.Purchasing;

namespace WerkPilot.UnitTests;

public sealed class SupplierInvoiceTests
{
    [Fact]
    public void Invoice_CanBeApprovedAfterReview()
    {
        var invoice = new SupplierInvoice(
            "RE-100",
            Guid.NewGuid(),
            "Stahl GmbH",
            new DateOnly(2026, 8, 2),
            new DateOnly(2026, 8, 16),
            "Max");

        invoice.AddLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "MAT-001",
            "Stahlblech",
            10m,
            3m);

        invoice.SubmitForReview();
        invoice.Approve("Max", "Geprüft");

        Assert.Equal(SupplierInvoiceStatus.Approved, invoice.Status);
        Assert.Equal("Max", invoice.ApprovedBy);
        Assert.NotNull(invoice.ApprovedAtUtc);
    }

    [Fact]
    public void PaidInvoice_CannotBeCancelled()
    {
        var invoice = new SupplierInvoice(
            "RE-100",
            Guid.NewGuid(),
            "Stahl GmbH",
            new DateOnly(2026, 8, 2),
            new DateOnly(2026, 8, 16),
            "Max");

        invoice.AddLine(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "MAT-001",
            "Stahlblech",
            10m,
            3m);

        invoice.SubmitForReview();
        invoice.Approve("Max", null);
        invoice.MarkPaid();

        Assert.Throws<InvalidOperationException>(() => invoice.Cancel());
    }

    [Fact]
    public void DueDateBeforeInvoiceDate_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new SupplierInvoice(
                "RE-100",
                Guid.NewGuid(),
                "Stahl GmbH",
                new DateOnly(2026, 8, 10),
                new DateOnly(2026, 8, 2),
                "Max"));
    }
}
