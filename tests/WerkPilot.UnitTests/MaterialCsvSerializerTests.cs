using WerkPilot.Infrastructure.Materials;

namespace WerkPilot.UnitTests;

public sealed class MaterialCsvSerializerTests
{
    [Fact]
    public void Parse_ReadsSemicolonCsv()
    {
        var serializer = new SemicolonMaterialCsvSerializer();
        var rows = serializer.Parse(
            "Artikelnummer;Beschreibung;Einheit;Einkaufspreis;Lieferant;LieferantenArtikelnummer\n"
            + "MAT-001;Stahlblech;kg;2,50;Stahl GmbH;S-1\n");

        var row = Assert.Single(rows);
        Assert.Equal("MAT-001", row.ArticleNumber);
        Assert.Equal(2.50m, row.PurchasePrice);
        Assert.Equal("Stahl GmbH", row.Supplier);
    }

    [Fact]
    public void Parse_SupportsQuotedSemicolons()
    {
        var serializer = new SemicolonMaterialCsvSerializer();
        var rows = serializer.Parse(
            "Artikelnummer;Beschreibung;Einheit;Einkaufspreis\n"
            + "MAT-001;\"Blech; verzinkt\";kg;3,10\n");

        Assert.Equal("Blech; verzinkt", Assert.Single(rows).Description);
    }

    [Fact]
    public void Parse_InvalidPrice_Throws()
    {
        var serializer = new SemicolonMaterialCsvSerializer();

        Assert.Throws<FormatException>(() =>
            serializer.Parse(
                "Artikelnummer;Beschreibung;Einheit;Einkaufspreis\n"
                + "MAT-001;Stahlblech;kg;falsch\n"));
    }
}
