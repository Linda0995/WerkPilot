using WerkPilot.Application.Auditing;
using WerkPilot.Domain.Customers;

namespace WerkPilot.Application.Customers;

public sealed class CustomerService(
    ICustomerRepository repository,
    IAuditTrail auditTrail)
{
    private const string EntityType = "Customer";

    public async Task<IReadOnlyList<CustomerDto>> SearchAsync(
        string? searchText,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default) =>
        (await repository.SearchAsync(searchText, includeDeleted, cancellationToken))
            .Select(Map)
            .ToArray();

    public Task<IReadOnlyList<AuditEvent>> GetHistoryAsync(
        Guid customerId,
        CancellationToken cancellationToken = default) =>
        auditTrail.GetForEntityAsync(EntityType, customerId, cancellationToken: cancellationToken);

    public async Task<CustomerDto> CreateAsync(
        string displayName,
        CustomerType type,
        CancellationToken cancellationToken = default)
    {
        var validation = CustomerValidator.ValidateNewCustomer(displayName);
        if (!validation.IsValid)
            throw new CustomerValidationException(validation);

        var duplicates = await repository.FindDuplicatesAsync(
            displayName, null, null, null, cancellationToken);

        if (duplicates.Count > 0)
            throw new CustomerDuplicateException(duplicates);

        var number = await repository.GetNextCustomerNumberAsync(DateTime.Today.Year, cancellationToken);
        var customer = new Customer(number, displayName, type);

        await repository.AddAsync(customer, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            EntityType, customer.Id, "Created",
            $"Kunde {customer.CustomerNumber} wurde angelegt.",
            cancellationToken);

        return Map(customer);
    }

    public async Task UpdateAsync(UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var validation = CustomerValidator.Validate(request);
        if (!validation.IsValid)
            throw new CustomerValidationException(validation);

        var duplicates = await repository.FindDuplicatesAsync(
            request.DisplayName,
            request.Email,
            request.VatId,
            request.Id,
            cancellationToken);

        var blockingDuplicates = duplicates
            .Where(x => x.Reason is "gleiche UID/ATU" or "gleiche E-Mail-Adresse")
            .ToArray();

        if (blockingDuplicates.Length > 0)
            throw new CustomerDuplicateException(blockingDuplicates);

        var customer = await GetRequiredAsync(request.Id, cancellationToken);
        customer.UpdateMasterData(request.DisplayName, request.Type);
        customer.UpdatePrimaryContact(request.ContactPerson, request.Email, request.Phone);

        var billing = new Address(
            request.BillingStreet,
            request.BillingPostalCode,
            request.BillingCity,
            request.BillingCountryCode);

        var delivery = request.DeliveryAddressEqualsBillingAddress
            ? null
            : new Address(
                request.DeliveryStreet,
                request.DeliveryPostalCode,
                request.DeliveryCity,
                request.DeliveryCountryCode);

        customer.SetAddresses(billing, delivery);
        customer.UpdateTax(request.VatId, request.TaxProfile);
        customer.UpdateNotes(request.Notes);

        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            EntityType, customer.Id, "Updated",
            "Kundenstammdaten wurden geändert.",
            cancellationToken);
    }

    public async Task AddContactAsync(
        AddCustomerContactRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = CustomerContactValidator.Validate(request);
        if (!validation.IsValid)
            throw new CustomerValidationException(validation);

        var customer = await GetRequiredAsync(request.CustomerId, cancellationToken);
        var contact = customer.AddContact(
            request.Label,
            request.Email,
            request.Phone,
            request.IsPrimary);

        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            EntityType, customer.Id, "ContactAdded",
            $"Ansprechpartner „{contact.Label}“ wurde hinzugefügt.",
            cancellationToken);
    }

    public async Task RemoveContactAsync(
        Guid customerId,
        Guid contactId,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetRequiredAsync(customerId, cancellationToken);
        var contact = customer.Contacts.Single(x => x.Id == contactId);
        customer.RemoveContact(contactId);

        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            EntityType, customer.Id, "ContactRemoved",
            $"Ansprechpartner „{contact.Label}“ wurde entfernt.",
            cancellationToken);
    }

    public async Task SetPrimaryContactAsync(
        Guid customerId,
        Guid contactId,
        CancellationToken cancellationToken = default)
    {
        var customer = await GetRequiredAsync(customerId, cancellationToken);
        var contact = customer.Contacts.Single(x => x.Id == contactId);
        customer.SetPrimaryContact(contactId);

        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            EntityType, customer.Id, "PrimaryContactChanged",
            $"„{contact.Label}“ wurde als Hauptansprechpartner festgelegt.",
            cancellationToken);
    }

    public async Task ToggleFavoriteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetRequiredAsync(id, cancellationToken);
        customer.SetFavorite(!customer.IsFavorite);

        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            EntityType, customer.Id, "FavoriteChanged",
            customer.IsFavorite ? "Kunde wurde als Favorit markiert." : "Favoritenmarkierung wurde entfernt.",
            cancellationToken);
    }

    public async Task MoveToTrashAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetRequiredAsync(id, cancellationToken);
        if (customer.IsDeleted)
            return;

        customer.MoveToTrash();
        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            EntityType, customer.Id, "MovedToTrash",
            "Kunde wurde in den Papierkorb verschoben.",
            cancellationToken);
    }

    public async Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetRequiredAsync(id, cancellationToken);
        if (!customer.IsDeleted)
            return;

        customer.Restore();
        await repository.SaveChangesAsync(cancellationToken);
        await auditTrail.WriteAsync(
            EntityType, customer.Id, "Restored",
            "Kunde wurde aus dem Papierkorb wiederhergestellt.",
            cancellationToken);
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default) =>
        new(
            await repository.CountAsync(cancellationToken),
            await repository.CountFavoritesAsync(cancellationToken),
            0,
            0);

    private async Task<Customer> GetRequiredAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new InvalidOperationException("Kunde wurde nicht gefunden.");

    private static CustomerDto Map(Customer c) => new(
        c.Id,
        c.CustomerNumber,
        c.DisplayName,
        c.Type,
        c.ContactPerson,
        c.BillingAddress?.Street,
        c.BillingAddress?.PostalCode,
        c.BillingAddress?.City,
        c.BillingAddress?.CountryCode ?? "AT",
        c.DeliveryAddress?.Street,
        c.DeliveryAddress?.PostalCode,
        c.DeliveryAddress?.City,
        c.DeliveryAddress?.CountryCode ?? c.BillingAddress?.CountryCode ?? "AT",
        c.Email,
        c.Phone,
        c.VatId,
        c.TaxProfile,
        c.Notes,
        c.IsFavorite,
        c.IsDeleted,
        c.LastContactAtUtc,
        c.Contacts
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.Label)
            .Select(x => new CustomerContactDto(x.Id, x.Label, x.Email, x.Phone, x.IsPrimary))
            .ToArray());
}
