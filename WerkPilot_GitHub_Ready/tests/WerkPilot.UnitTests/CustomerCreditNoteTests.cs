using WerkPilot.Domain.Billing;

namespace WerkPilot.UnitTests;

public sealed class CustomerCreditNoteTests
{
    [Fact]
    public void IssuedCreditNote_CanBeApplied()
    {
        var note = CreateCreditNote();
        note.Issue();
        note.MarkApplied();

        Assert.Equal(CustomerCreditNoteStatus.Applied, note.Status);
        Assert.NotNull(note.AppliedAtUtc);
    }

    [Fact]
    public void AppliedCreditNote_CannotBeCancelled()
    {
        var note = CreateCreditNote();
        note.Issue();
        note.MarkApplied();

        Assert.Throws<InvalidOperationException>(() => note.Cancel());
    }

    [Fact]
    public void CreditReducesInvoiceOpenAmount()
    {
        var invoice = new CustomerInvoice(
            "RE-2026-0001",
            Guid.NewGuid(),
            "Muster GmbH",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 15),
            null,
            null,
            "Max");

        invoice.AddLine("Leistung", 1m, "Pauschal", 100m, 20m);
        invoice.Issue();
        invoice.ApplyCredit(30m);

        Assert.Equal(30m, invoice.CreditedAmount);
        Assert.Equal(90m, invoice.OpenAmount);
        Assert.Equal(CustomerInvoiceStatus.PartiallyPaid, invoice.Status);
    }

    private static CustomerCreditNote CreateCreditNote()
    {
        var note = new CustomerCreditNote(
            "GS-2026-0001",
            Guid.NewGuid(),
            "RE-2026-0001",
            Guid.NewGuid(),
            "Muster GmbH",
            new DateOnly(2026, 8, 6),
            "Preisnachlass",
            "Max");

        note.AddLine(Guid.NewGuid(), "Leistung", 1m, "Pauschal", 25m, 20m);
        return note;
    }
}
