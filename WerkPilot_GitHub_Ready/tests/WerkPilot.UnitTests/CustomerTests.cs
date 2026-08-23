using WerkPilot.Domain.Customers;
namespace WerkPilot.UnitTests;
public sealed class CustomerTests
{
 [Fact] public void Constructor_WithValidValues_CreatesCustomer(){var c=new Customer("K-2026-0001","Muster GmbH",CustomerType.Company);Assert.Equal("K-2026-0001",c.CustomerNumber);Assert.False(c.IsDeleted);}
 [Fact] public void Constructor_WithoutName_Throws()=>Assert.Throws<ArgumentException>(()=>new Customer("K-1"," ",CustomerType.Company));
 [Fact] public void MoveToTrash_AndRestore_ChangesState(){var c=new Customer("K-1","Test",CustomerType.Company);c.MoveToTrash();Assert.True(c.IsDeleted);c.Restore();Assert.False(c.IsDeleted);}
 [Fact] public void UpdateTax_NormalizesVatId(){var c=new Customer("K-1","Test",CustomerType.Company);c.UpdateTax(" atu12345678 ",TaxProfile.Domestic);Assert.Equal("ATU12345678",c.VatId);}
 [Fact] public void DeliveryAddress_DefaultsToBillingAddress(){var c=new Customer("K-1","Test",CustomerType.Company);var a=new Address("Straße 1","8010","Graz","AT");c.SetAddresses(a,null);Assert.Same(a,c.DeliveryAddress);}
 [Fact] public void AddPrimaryContact_DemotesPreviousPrimary(){var c=new Customer("K-1","Test",CustomerType.Company);c.AddContact("A","a@test.at",null,true);c.AddContact("B","b@test.at",null,true);Assert.Single(c.Contacts,x=>x.IsPrimary);Assert.Equal("B",c.Contacts.Single(x=>x.IsPrimary).Label);}
 [Fact] public void Favorite_CanBeChanged(){var c=new Customer("K-1","Test",CustomerType.Company);c.SetFavorite(true);Assert.True(c.IsFavorite);}

    [Fact]
    public void UpdateMasterData_ChangesNameAndType()
    {
        var customer = new Customer("K-1", "Alt", CustomerType.Company);
        customer.UpdateMasterData("Neu", CustomerType.Private);
        Assert.Equal("Neu", customer.DisplayName);
        Assert.Equal(CustomerType.Private, customer.Type);
    }

    [Fact]
    public void Restore_AfterTrash_MakesCustomerActiveAgain()
    {
        var customer = new Customer("K-1", "Test", CustomerType.Company);
        customer.MoveToTrash();
        customer.Restore();
        Assert.False(customer.IsDeleted);
    }

}
