using Microsoft.EntityFrameworkCore;
using WerkPilot.Domain.Calculation;
using WerkPilot.Domain.Billing;
using WerkPilot.Domain.Documents;
using WerkPilot.Domain.Crm;
using WerkPilot.Domain.Customers;
using WerkPilot.Domain.Identity;
using WerkPilot.Domain.Inventory;
using WerkPilot.Domain.Materials;
using WerkPilot.Domain.Messaging;
using WerkPilot.Domain.Notifications;
using WerkPilot.Domain.Offers;
using WerkPilot.Domain.Purchasing;
using WerkPilot.Domain.Projects;
using WerkPilot.Domain.ProjectCosts;
using WerkPilot.Domain.Settings;
using WerkPilot.Domain.Workbench;
using WerkPilot.Domain.TimeTracking;

namespace WerkPilot.Infrastructure.Persistence;

public sealed class WerkPilotDbContext(DbContextOptions<WerkPilotDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerInteraction> CustomerInteractions => Set<CustomerInteraction>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<OfferCalculation> OfferCalculations => Set<OfferCalculation>();
    public DbSet<MaterialItem> MaterialItems => Set<MaterialItem>();
    public DbSet<PurchaseList> PurchaseLists => Set<PurchaseList>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<DocumentFolder> DocumentFolders => Set<DocumentFolder>();
    public DbSet<DocumentFile> DocumentFiles => Set<DocumentFile>();
    public DbSet<NotificationReadState> NotificationReadStates => Set<NotificationReadState>();
    public DbSet<WorkbenchItem> WorkbenchItems => Set<WorkbenchItem>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<ProjectActualCost> ProjectActualCosts => Set<ProjectActualCost>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<InventoryCount> InventoryCounts => Set<InventoryCount>();
    public DbSet<SupplierOrder> SupplierOrders => Set<SupplierOrder>();
    public DbSet<SupplierInvoice> SupplierInvoices => Set<SupplierInvoice>();
    public DbSet<CustomerInvoice> CustomerInvoices => Set<CustomerInvoice>();
    public DbSet<CustomerCreditNote> CustomerCreditNotes => Set<CustomerCreditNote>();
    public DbSet<DunningNotice> DunningNotices => Set<DunningNotice>();
    public DbSet<DocumentEmailDispatch> DocumentEmailDispatches => Set<DocumentEmailDispatch>();
    public DbSet<DocumentEmailTemplate> DocumentEmailTemplates => Set<DocumentEmailTemplate>();
    public DbSet<CustomerFollowUp> CustomerFollowUps => Set<CustomerFollowUp>();
    public DbSet<UserAbsence> UserAbsences => Set<UserAbsence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        WerkPilotModelConfiguration.Configure(modelBuilder);
}
