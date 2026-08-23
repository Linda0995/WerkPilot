using WerkPilot.Domain.Materials;

namespace WerkPilot.UnitTests;

public sealed class MaterialItemTests
{
    [Fact]
    public void Constructor_CreatesActiveMaterial()
    {
        var item = new MaterialItem("MAT-001", "Stahlblech", "kg", 2.50m);

        Assert.True(item.IsActive);
        Assert.Equal("MAT-001", item.ArticleNumber);
        Assert.Equal(2.50m, item.PurchasePrice);
    }

    [Fact]
    public void Update_ChangesPriceAndSupplier()
    {
        var item = new MaterialItem("MAT-001", "Stahlblech", "kg", 2.50m);

        item.Update(
            "MAT-001",
            "Stahlblech S355",
            "kg",
            3.10m,
            "Muster Stahl",
            "S355-001");

        Assert.Equal(3.10m, item.PurchasePrice);
        Assert.Equal("Muster Stahl", item.Supplier);
        Assert.Equal("S355-001", item.SupplierArticleNumber);
    }

    [Fact]
    public void Deactivate_AndActivate_ChangeState()
    {
        var item = new MaterialItem("MAT-001", "Stahlblech", "kg", 2.50m);

        item.Deactivate();
        Assert.False(item.IsActive);

        item.Activate();
        Assert.True(item.IsActive);
    }

    [Fact]
    public void NegativePurchasePrice_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MaterialItem("MAT-001", "Stahlblech", "kg", -1m));
    }

    [Fact]
    public void IsPriceOutdated_WithFreshPrice_IsFalse()
    {
        var item = new MaterialItem("MAT-001", "Stahlblech", "kg", 2.50m);

        Assert.False(item.IsPriceOutdated(90));
    }

    [Fact]
    public void IsPriceOutdated_WithNegativeAge_Throws()
    {
        var item = new MaterialItem("MAT-001", "Stahlblech", "kg", 2.50m);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            item.IsPriceOutdated(-1));
    }
}
