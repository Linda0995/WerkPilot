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

public static class WerkPilotModelConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        var customer = modelBuilder.Entity<Customer>();
        customer.ToTable("customers");
        customer.HasKey(x => x.Id);

        customer.Property(x => x.CustomerNumber).HasMaxLength(30).IsRequired();
        customer.HasIndex(x => x.CustomerNumber).IsUnique();
        customer.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        customer.HasIndex(x => x.DisplayName);
        customer.Property(x => x.ContactPerson).HasMaxLength(150);
        customer.Property(x => x.Email).HasMaxLength(254);
        customer.Property(x => x.Phone).HasMaxLength(50);
        customer.Property(x => x.VatId).HasMaxLength(30);
        customer.HasIndex(x => x.VatId).HasFilter("\"VatId\" IS NOT NULL");
        customer.Property(x => x.Notes).HasMaxLength(4000);
        customer.Property(x => x.LastContactAtUtc);

        customer.OwnsOne(x => x.BillingAddress, address =>
        {
            address.Property(x => x.Street).HasColumnName("billing_street").HasMaxLength(200);
            address.Property(x => x.PostalCode).HasColumnName("billing_postal_code").HasMaxLength(20);
            address.Property(x => x.City).HasColumnName("billing_city").HasMaxLength(100);
            address.Property(x => x.CountryCode).HasColumnName("billing_country_code").HasMaxLength(2);
        });

        customer.OwnsOne(x => x.DeliveryAddress, address =>
        {
            address.Property(x => x.Street).HasColumnName("delivery_street").HasMaxLength(200);
            address.Property(x => x.PostalCode).HasColumnName("delivery_postal_code").HasMaxLength(20);
            address.Property(x => x.City).HasColumnName("delivery_city").HasMaxLength(100);
            address.Property(x => x.CountryCode).HasColumnName("delivery_country_code").HasMaxLength(2);
        });

        customer.OwnsMany(x => x.Contacts, contact =>
        {
            contact.ToTable("customer_contacts");
            contact.WithOwner().HasForeignKey("CustomerId");
            contact.HasKey(x => x.Id);
            contact.Property(x => x.Label).HasMaxLength(100).IsRequired();
            contact.Property(x => x.Email).HasMaxLength(254);
            contact.Property(x => x.Phone).HasMaxLength(50);
        });

        customer.HasQueryFilter(x => !x.IsDeleted);

        var audit = modelBuilder.Entity<AuditEntry>();
        audit.ToTable("audit_entries");
        audit.HasKey(x => x.Id);
        audit.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        audit.Property(x => x.Action).HasMaxLength(100).IsRequired();
        audit.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        audit.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAtUtc });

        var user = modelBuilder.Entity<AppUser>();
        user.ToTable("app_users");
        user.HasKey(x => x.Id);
        user.Property(x => x.UserName).HasMaxLength(100).IsRequired();
        user.HasIndex(x => x.UserName).IsUnique();
        user.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
        user.Property(x => x.PasswordHash).HasMaxLength(200);
        user.Property(x => x.PasswordSalt).HasMaxLength(100);
        user.HasQueryFilter(x => !x.IsDeleted);

        var offer = modelBuilder.Entity<Offer>();
        offer.ToTable("offers");
        offer.HasKey(x => x.Id);
        offer.Property(x => x.OfferNumber).HasMaxLength(30).IsRequired();
        offer.HasIndex(x => x.OfferNumber).IsUnique();
        offer.Property(x => x.Title).HasMaxLength(250).IsRequired();
        offer.Property(x => x.TaxRate).HasPrecision(5, 2);
        offer.Property(x => x.DiscountPercent).HasPrecision(5, 2);
        offer.HasIndex(x => x.CustomerId);
        offer.HasQueryFilter(x => !x.IsDeleted);

        offer.OwnsMany(x => x.Positions, position =>
        {
            position.ToTable("offer_positions");
            position.WithOwner().HasForeignKey("OfferId");
            position.HasKey(x => x.Id);
            position.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            position.Property(x => x.Quantity).HasPrecision(18, 3);
            position.Property(x => x.UnitPriceNet).HasPrecision(18, 2);
        });

        var company = modelBuilder.Entity<CompanyProfile>();
        company.ToTable("company_profiles");
        company.HasKey(x => x.Id);
        company.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
        company.Property(x => x.Street).HasMaxLength(200);
        company.Property(x => x.PostalCode).HasMaxLength(20);
        company.Property(x => x.City).HasMaxLength(100);
        company.Property(x => x.CountryCode).HasMaxLength(2).IsRequired();
        company.Property(x => x.Email).HasMaxLength(254);
        company.Property(x => x.Phone).HasMaxLength(50);
        company.Property(x => x.VatId).HasMaxLength(30);
        company.Property(x => x.Website).HasMaxLength(250);
        company.Property(x => x.OfferIntroText).HasMaxLength(2000).IsRequired();
        company.Property(x => x.OfferClosingText).HasMaxLength(2000).IsRequired();
        company.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        company.Property(x => x.OfferEmailSubjectTemplate).HasMaxLength(500).IsRequired();
        company.Property(x => x.OfferEmailBodyTemplate).HasMaxLength(5000).IsRequired();

        var calculation = modelBuilder.Entity<OfferCalculation>();
        calculation.ToTable("offer_calculations");
        calculation.HasKey(x => x.Id);
        calculation.HasIndex(x => x.OfferId).IsUnique();
        calculation.Property(x => x.ProfitTargetPercent).HasPrecision(7, 2);
        calculation.HasQueryFilter(x => !x.IsDeleted);

        calculation.OwnsMany(x => x.Items, item =>
        {
            item.ToTable("calculation_items");
            item.WithOwner().HasForeignKey("CalculationId");
            item.HasKey(x => x.Id);
            item.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            item.Property(x => x.Quantity).HasPrecision(18, 3);
            item.Property(x => x.UnitCost).HasPrecision(18, 2);
            item.Property(x => x.MaterialItemId);
        });

        var material = modelBuilder.Entity<MaterialItem>();
        material.ToTable("material_items");
        material.HasKey(x => x.Id);
        material.Property(x => x.ArticleNumber).HasMaxLength(100).IsRequired();
        material.HasIndex(x => x.ArticleNumber).IsUnique();
        material.Property(x => x.Description).HasMaxLength(500).IsRequired();
        material.Property(x => x.Unit).HasMaxLength(30).IsRequired();
        material.Property(x => x.PurchasePrice).HasPrecision(18, 4);
        material.Property(x => x.Supplier).HasMaxLength(200);
        material.Property(x => x.SupplierArticleNumber).HasMaxLength(100);
        material.HasQueryFilter(x => !x.IsDeleted && x.IsActive);

        var purchaseList = modelBuilder.Entity<PurchaseList>();
        purchaseList.ToTable("purchase_lists");
        purchaseList.HasKey(x => x.Id);
        purchaseList.Property(x => x.PurchaseListNumber).HasMaxLength(30).IsRequired();
        purchaseList.HasIndex(x => x.PurchaseListNumber).IsUnique();
        purchaseList.HasIndex(x => x.OfferId).IsUnique();
        purchaseList.Property(x => x.Title).HasMaxLength(300).IsRequired();
        purchaseList.HasQueryFilter(x => !x.IsDeleted);

        purchaseList.OwnsMany(x => x.Items, item =>
        {
            item.ToTable("purchase_list_items");
            item.WithOwner().HasForeignKey("PurchaseListId");
            item.HasKey(x => x.Id);
            item.Property(x => x.ArticleNumber).HasMaxLength(100).IsRequired();
            item.Property(x => x.Description).HasMaxLength(500).IsRequired();
            item.Property(x => x.Unit).HasMaxLength(30).IsRequired();
            item.Property(x => x.RequiredQuantity).HasPrecision(18, 3);
            item.Property(x => x.PurchasePrice).HasPrecision(18, 4);
            item.Property(x => x.Supplier).HasMaxLength(200);
            item.Property(x => x.OrderNote).HasMaxLength(1000);
        });

        var project = modelBuilder.Entity<Project>();
        project.ToTable("projects");
        project.HasKey(x => x.Id);
        project.Property(x => x.ProjectNumber).HasMaxLength(30).IsRequired();
        project.HasIndex(x => x.ProjectNumber).IsUnique();
        project.HasIndex(x => x.SourceOfferId).IsUnique();
        project.HasIndex(x => x.CustomerId);
        project.Property(x => x.Title).HasMaxLength(300).IsRequired();
        project.Property(x => x.Description).HasMaxLength(4000);
        project.Property(x => x.ProjectManager).HasMaxLength(150);
        project.HasQueryFilter(x => !x.IsDeleted);

        project.OwnsMany(x => x.Tasks, task =>
        {
            task.ToTable("project_tasks");
            task.WithOwner().HasForeignKey("ProjectId");
            task.HasKey(x => x.Id);
            task.Property(x => x.Title).HasMaxLength(500).IsRequired();
            task.Property(x => x.AssignedTo).HasMaxLength(150);
            task.HasIndex(x => x.AssignedUserId);
        });

        var folder = modelBuilder.Entity<DocumentFolder>();
        folder.ToTable("document_folders");
        folder.HasKey(x => x.Id);
        folder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        folder.HasIndex(x => new { x.OwnerType, x.OwnerId, x.ParentFolderId });
        folder.HasQueryFilter(x => !x.IsDeleted);

        var documentFile = modelBuilder.Entity<DocumentFile>();
        documentFile.ToTable("document_files");
        documentFile.HasKey(x => x.Id);
        documentFile.Property(x => x.DisplayName).HasMaxLength(500).IsRequired();
        documentFile.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
        documentFile.Property(x => x.RelativePath).HasMaxLength(1000).IsRequired();
        documentFile.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        documentFile.HasIndex(x => new { x.OwnerType, x.OwnerId, x.FolderId });
        documentFile.HasQueryFilter(x => !x.IsDeleted);

        var notificationRead = modelBuilder.Entity<NotificationReadState>();
        notificationRead.ToTable("notification_read_states");
        notificationRead.HasKey(x => x.Id);
        notificationRead.Property(x => x.NotificationKey).HasMaxLength(300).IsRequired();
        notificationRead.HasIndex(x => new { x.UserId, x.NotificationKey }).IsUnique();

        var workbench = modelBuilder.Entity<WorkbenchItem>();
        workbench.ToTable("workbench_items");
        workbench.HasKey(x => x.Id);
        workbench.Property(x => x.ItemType).HasMaxLength(50).IsRequired();
        workbench.Property(x => x.Number).HasMaxLength(100);
        workbench.Property(x => x.Title).HasMaxLength(500).IsRequired();
        workbench.Property(x => x.Subtitle).HasMaxLength(1000);
        workbench.HasIndex(x => new { x.UserId, x.ItemType, x.EntityId }).IsUnique();
        workbench.HasIndex(x => new { x.UserId, x.IsFavorite, x.LastOpenedAtUtc });

        var interaction = modelBuilder.Entity<CustomerInteraction>();
        interaction.ToTable("customer_interactions");
        interaction.HasKey(x => x.Id);
        interaction.HasIndex(x => new { x.CustomerId, x.OccurredAtUtc });
        interaction.HasIndex(x => new { x.FollowUpCompleted, x.FollowUpDate });
        interaction.Property(x => x.Subject).HasMaxLength(300).IsRequired();
        interaction.Property(x => x.Notes).HasMaxLength(8000).IsRequired();
        interaction.Property(x => x.ContactPerson).HasMaxLength(200);
        interaction.Property(x => x.CreatedBy).HasMaxLength(150);
        interaction.Property(x => x.FollowUpOwner).HasMaxLength(150);
        interaction.HasQueryFilter(x => !x.IsDeleted);

        var timeEntry = modelBuilder.Entity<TimeEntry>();
        timeEntry.ToTable("time_entries");
        timeEntry.HasKey(x => x.Id);
        timeEntry.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        timeEntry.HasIndex(x => new { x.ProjectId, x.StartedAtUtc });
        timeEntry.HasIndex(x => new { x.UserId, x.EndedAtUtc });
        timeEntry.HasQueryFilter(x => !x.IsDeleted);

        var actualCost = modelBuilder.Entity<ProjectActualCost>();
        actualCost.ToTable("project_actual_costs");
        actualCost.HasKey(x => x.Id);
        actualCost.Property(x => x.Description).HasMaxLength(500).IsRequired();
        actualCost.Property(x => x.AmountNet).HasPrecision(18, 2);
        actualCost.Property(x => x.Reference).HasMaxLength(200);
        actualCost.HasIndex(x => new { x.ProjectId, x.CostDate });
        actualCost.HasQueryFilter(x => !x.IsDeleted);

        var inventoryItem = modelBuilder.Entity<InventoryItem>();
        inventoryItem.ToTable("inventory_items");
        inventoryItem.HasKey(x => x.Id);
        inventoryItem.HasIndex(x => x.MaterialItemId).IsUnique();
        inventoryItem.Property(x => x.StorageLocation).HasMaxLength(150);
        inventoryItem.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
        inventoryItem.Property(x => x.ReservedQuantity).HasPrecision(18, 3);
        inventoryItem.Property(x => x.MinimumStock).HasPrecision(18, 3);
        inventoryItem.HasQueryFilter(x => !x.IsDeleted);

        var inventoryMovement = modelBuilder.Entity<InventoryMovement>();
        inventoryMovement.ToTable("inventory_movements");
        inventoryMovement.HasKey(x => x.Id);
        inventoryMovement.Property(x => x.Quantity).HasPrecision(18, 3);
        inventoryMovement.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        inventoryMovement.Property(x => x.Reference).HasMaxLength(200);
        inventoryMovement.Property(x => x.CreatedBy).HasMaxLength(150);
        inventoryMovement.HasIndex(x => new { x.InventoryItemId, x.OccurredAtUtc });
        inventoryMovement.HasQueryFilter(x => !x.IsDeleted);

        var inventoryCount = modelBuilder.Entity<InventoryCount>();
        inventoryCount.ToTable("inventory_counts");
        inventoryCount.HasKey(x => x.Id);
        inventoryCount.Property(x => x.CountNumber).HasMaxLength(30).IsRequired();
        inventoryCount.HasIndex(x => x.CountNumber).IsUnique();
        inventoryCount.Property(x => x.Title).HasMaxLength(300).IsRequired();
        inventoryCount.Property(x => x.StorageLocation).HasMaxLength(150);
        inventoryCount.Property(x => x.CreatedBy).HasMaxLength(150);
        inventoryCount.Property(x => x.PostedBy).HasMaxLength(150);
        inventoryCount.HasQueryFilter(x => !x.IsDeleted);

        inventoryCount.OwnsMany(x => x.Lines, line =>
        {
            line.ToTable("inventory_count_lines");
            line.WithOwner().HasForeignKey("InventoryCountId");
            line.HasKey(x => x.Id);
            line.HasIndex(x => new { x.InventoryItemId });
            line.Property(x => x.ExpectedQuantity).HasPrecision(18, 3);
            line.Property(x => x.CountedQuantity).HasPrecision(18, 3);
            line.Property(x => x.Note).HasMaxLength(1000);
            line.Property(x => x.CountedBy).HasMaxLength(150);
        });

        var supplierOrder = modelBuilder.Entity<SupplierOrder>();
        supplierOrder.ToTable("supplier_orders");
        supplierOrder.HasKey(x => x.Id);
        supplierOrder.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
        supplierOrder.HasIndex(x => x.OrderNumber).IsUnique();
        supplierOrder.Property(x => x.SupplierName).HasMaxLength(250).IsRequired();
        supplierOrder.Property(x => x.SupplierReference).HasMaxLength(200);
        supplierOrder.Property(x => x.CreatedBy).HasMaxLength(150);
        supplierOrder.HasQueryFilter(x => !x.IsDeleted);

        supplierOrder.OwnsMany(x => x.Lines, line =>
        {
            line.ToTable("supplier_order_lines");
            line.WithOwner().HasForeignKey("SupplierOrderId");
            line.HasKey(x => x.Id);
            line.HasIndex(x => new { x.MaterialItemId });
            line.Property(x => x.ArticleNumber).HasMaxLength(100).IsRequired();
            line.Property(x => x.Description).HasMaxLength(500).IsRequired();
            line.Property(x => x.Unit).HasMaxLength(30).IsRequired();
            line.Property(x => x.OrderedQuantity).HasPrecision(18, 3);
            line.Property(x => x.ReceivedQuantity).HasPrecision(18, 3);
            line.Property(x => x.UnitPriceNet).HasPrecision(18, 4);
        });

        var supplierInvoice = modelBuilder.Entity<SupplierInvoice>();
        supplierInvoice.ToTable("supplier_invoices");
        supplierInvoice.HasKey(x => x.Id);
        supplierInvoice.Property(x => x.InvoiceNumber).HasMaxLength(100).IsRequired();
        supplierInvoice.Property(x => x.SupplierName).HasMaxLength(250).IsRequired();
        supplierInvoice.Property(x => x.CreatedBy).HasMaxLength(150);
        supplierInvoice.Property(x => x.ReviewNote).HasMaxLength(2000);
        supplierInvoice.Property(x => x.ApprovedBy).HasMaxLength(150);
        supplierInvoice.HasIndex(x => new { x.SupplierName, x.InvoiceNumber }).IsUnique();
        supplierInvoice.HasQueryFilter(x => !x.IsDeleted);

        supplierInvoice.Property(x => x.CashDiscountPercent).HasPrecision(5, 2);

        supplierInvoice.OwnsMany(x => x.Lines, line =>
        {
            line.ToTable("supplier_invoice_lines");
            line.WithOwner().HasForeignKey("SupplierInvoiceId");
            line.HasKey(x => x.Id);
            line.HasIndex(x => new { x.SupplierOrderLineId });
            line.Property(x => x.ArticleNumber).HasMaxLength(100).IsRequired();
            line.Property(x => x.Description).HasMaxLength(500).IsRequired();
            line.Property(x => x.InvoicedQuantity).HasPrecision(18, 3);
            line.Property(x => x.UnitPriceNet).HasPrecision(18, 4);
        });

        supplierInvoice.OwnsMany(x => x.Payments, payment =>
        {
            payment.ToTable("supplier_invoice_payments");
            payment.WithOwner().HasForeignKey("SupplierInvoiceId");
            payment.HasKey(x => x.Id);
            payment.Property(x => x.Amount).HasPrecision(18, 2);
            payment.Property(x => x.Reference).HasMaxLength(200);
            payment.Property(x => x.CreatedBy).HasMaxLength(150);
            payment.HasIndex(x => new { x.PaymentDate });
        });

        var customerInvoice = modelBuilder.Entity<CustomerInvoice>();
        customerInvoice.ToTable("customer_invoices");
        customerInvoice.HasKey(x => x.Id);
        customerInvoice.Property(x => x.InvoiceNumber).HasMaxLength(30).IsRequired();
        customerInvoice.HasIndex(x => x.InvoiceNumber).IsUnique();
        customerInvoice.Property(x => x.CustomerName).HasMaxLength(300).IsRequired();
        customerInvoice.Property(x => x.CreatedBy).HasMaxLength(150);
        customerInvoice.Property(x => x.CreditedAmount).HasField("_creditedAmount").HasPrecision(18, 2);
        customerInvoice.HasQueryFilter(x => !x.IsDeleted);

        customerInvoice.OwnsMany(x => x.Lines, line =>
        {
            line.ToTable("customer_invoice_lines");
            line.WithOwner().HasForeignKey("CustomerInvoiceId");
            line.HasKey(x => x.Id);
            line.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            line.Property(x => x.Unit).HasMaxLength(30).IsRequired();
            line.Property(x => x.Quantity).HasPrecision(18, 3);
            line.Property(x => x.UnitPriceNet).HasPrecision(18, 4);
            line.Property(x => x.VatRatePercent).HasPrecision(5, 2);
        });

        customerInvoice.OwnsMany(x => x.Payments, payment =>
        {
            payment.ToTable("customer_invoice_payments");
            payment.WithOwner().HasForeignKey("CustomerInvoiceId");
            payment.HasKey(x => x.Id);
            payment.Property(x => x.Amount).HasPrecision(18, 2);
            payment.Property(x => x.Reference).HasMaxLength(200);
            payment.Property(x => x.CreatedBy).HasMaxLength(150);
            payment.HasIndex(x => new { x.PaymentDate });
        });

        var customerCreditNote = modelBuilder.Entity<CustomerCreditNote>();
        customerCreditNote.ToTable("customer_credit_notes");
        customerCreditNote.HasKey(x => x.Id);
        customerCreditNote.Property(x => x.CreditNoteNumber).HasMaxLength(30).IsRequired();
        customerCreditNote.HasIndex(x => x.CreditNoteNumber).IsUnique();
        customerCreditNote.Property(x => x.CustomerInvoiceNumber).HasMaxLength(30).IsRequired();
        customerCreditNote.Property(x => x.CustomerName).HasMaxLength(300).IsRequired();
        customerCreditNote.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
        customerCreditNote.Property(x => x.CreatedBy).HasMaxLength(150);
        customerCreditNote.HasIndex(x => x.CustomerInvoiceId);
        customerCreditNote.HasQueryFilter(x => !x.IsDeleted);

        customerCreditNote.OwnsMany(x => x.Lines, line =>
        {
            line.ToTable("customer_credit_note_lines");
            line.WithOwner().HasForeignKey("CustomerCreditNoteId");
            line.HasKey(x => x.Id);
            line.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            line.Property(x => x.Unit).HasMaxLength(30).IsRequired();
            line.Property(x => x.Quantity).HasPrecision(18, 3);
            line.Property(x => x.UnitPriceNet).HasPrecision(18, 4);
            line.Property(x => x.VatRatePercent).HasPrecision(5, 2);
        });

        var dunningNotice = modelBuilder.Entity<DunningNotice>();
        dunningNotice.ToTable("dunning_notices");
        dunningNotice.HasKey(x => x.Id);
        dunningNotice.Property(x => x.NoticeNumber).HasMaxLength(30).IsRequired();
        dunningNotice.HasIndex(x => x.NoticeNumber).IsUnique();
        dunningNotice.Property(x => x.CustomerInvoiceNumber).HasMaxLength(30).IsRequired();
        dunningNotice.Property(x => x.CustomerName).HasMaxLength(300).IsRequired();
        dunningNotice.Property(x => x.PrincipalAmount).HasPrecision(18, 2);
        dunningNotice.Property(x => x.FeeAmount).HasPrecision(18, 2);
        dunningNotice.Property(x => x.InterestAmount).HasPrecision(18, 2);
        dunningNotice.Property(x => x.AnnualInterestRatePercent).HasPrecision(7, 3);
        dunningNotice.Property(x => x.CreatedBy).HasMaxLength(150);
        dunningNotice.HasIndex(x => x.CustomerInvoiceId);
        dunningNotice.HasQueryFilter(x => !x.IsDeleted);

        var documentEmailDispatch = modelBuilder.Entity<DocumentEmailDispatch>();
        documentEmailDispatch.ToTable("document_email_dispatches");
        documentEmailDispatch.HasKey(x => x.Id);
        documentEmailDispatch.Property(x => x.DocumentNumber).HasMaxLength(50).IsRequired();
        documentEmailDispatch.Property(x => x.Recipient).HasMaxLength(320).IsRequired();
        documentEmailDispatch.Property(x => x.Subject).HasMaxLength(500).IsRequired();
        documentEmailDispatch.Property(x => x.Body).HasMaxLength(10000).IsRequired();
        documentEmailDispatch.Property(x => x.AttachmentFileName).HasMaxLength(300).IsRequired();
        documentEmailDispatch.Property(x => x.CreatedBy).HasMaxLength(150);
        documentEmailDispatch.Property(x => x.ErrorMessage).HasMaxLength(4000);
        documentEmailDispatch.HasIndex(x => new { x.DocumentType, x.DocumentId });
        documentEmailDispatch.HasIndex(x => x.CreatedAtUtc);
        documentEmailDispatch.HasIndex(x => x.NextRetryAtUtc);
        documentEmailDispatch.HasQueryFilter(x => !x.IsDeleted);

        var documentEmailTemplate = modelBuilder.Entity<DocumentEmailTemplate>();
        documentEmailTemplate.ToTable("document_email_templates");
        documentEmailTemplate.HasKey(x => x.Id);
        documentEmailTemplate.Property(x => x.Name).HasMaxLength(150).IsRequired();
        documentEmailTemplate.Property(x => x.SubjectTemplate).HasMaxLength(500).IsRequired();
        documentEmailTemplate.Property(x => x.BodyTemplate).HasMaxLength(10000).IsRequired();
        documentEmailTemplate.HasIndex(x => new { x.DocumentType, x.Name }).IsUnique();
        documentEmailTemplate.HasQueryFilter(x => !x.IsDeleted);


        var userAbsence = modelBuilder.Entity<UserAbsence>();
        userAbsence.ToTable("user_absences");
        userAbsence.HasKey(x => x.Id);
        userAbsence.Property(x => x.UserDisplayName).HasMaxLength(200).IsRequired();
        userAbsence.Property(x => x.SubstituteDisplayName).HasMaxLength(200);
        userAbsence.Property(x => x.Note).HasMaxLength(4000);
        userAbsence.Property(x => x.CreatedBy).HasMaxLength(150);
        userAbsence.HasIndex(x => new { x.UserId, x.StartDate, x.EndDate });
        userAbsence.HasIndex(x => x.SubstituteUserId);
        userAbsence.HasQueryFilter(x => !x.IsDeleted);

        var customerFollowUp = modelBuilder.Entity<CustomerFollowUp>();
        customerFollowUp.ToTable("customer_follow_ups");
        customerFollowUp.HasKey(x => x.Id);
        customerFollowUp.Property(x => x.CustomerNumber).HasMaxLength(50).IsRequired();
        customerFollowUp.Property(x => x.CustomerName).HasMaxLength(300).IsRequired();
        customerFollowUp.Property(x => x.Title).HasMaxLength(500).IsRequired();
        customerFollowUp.Property(x => x.Notes).HasMaxLength(4000);
        customerFollowUp.Property(x => x.AssignedTo).HasMaxLength(200);
        customerFollowUp.Property(x => x.CreatedBy).HasMaxLength(150);
        customerFollowUp.Property(x => x.CompletionNote).HasMaxLength(4000);
        customerFollowUp.HasIndex(x => x.CustomerId);
        customerFollowUp.HasIndex(x => new { x.Status, x.DueAtUtc });
        customerFollowUp.HasIndex(x => x.AssignedUserId);
        customerFollowUp.HasQueryFilter(x => !x.IsDeleted);
    }
}
