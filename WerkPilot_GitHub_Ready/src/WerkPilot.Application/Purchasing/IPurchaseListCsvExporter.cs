namespace WerkPilot.Application.Purchasing;

public interface IPurchaseListCsvExporter
{
    string Export(PurchaseListDto purchaseList);
}
