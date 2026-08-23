using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WerkPilot.Application.Auditing;
using WerkPilot.Application.Calculation;
using WerkPilot.Application.Billing;
using WerkPilot.Application.Crm;
using WerkPilot.Application.Customers;
using WerkPilot.Application.Dashboard;
using WerkPilot.Application.Documents;
using WerkPilot.Application.Identity;
using WerkPilot.Application.Inventory;
using WerkPilot.Application.Materials;
using WerkPilot.Application.Messaging;
using WerkPilot.Application.Notifications;
using WerkPilot.Application.Offers;
using WerkPilot.Application.Purchasing;
using WerkPilot.Application.Projects;
using WerkPilot.Application.ProjectCosts;
using WerkPilot.Application.Search;
using WerkPilot.Application.Settings;
using WerkPilot.Application.Workbench;
using WerkPilot.Application.TimeTracking;
using WerkPilot.Application.Work;
using WerkPilot.Application.Release;
using WerkPilot.Desktop.ViewModels;
using WerkPilot.Desktop.Services;
using WerkPilot.Desktop.Views;
using WerkPilot.Infrastructure.Documents;
using WerkPilot.Infrastructure.Materials;
using WerkPilot.Infrastructure.Messaging;
using WerkPilot.Infrastructure.Persistence;
using WerkPilot.Infrastructure.Inventory;
using WerkPilot.Infrastructure.Billing;
using WerkPilot.Infrastructure.Purchasing;
using WerkPilot.Infrastructure.ProjectCosts;
using WerkPilot.Infrastructure.Security;

namespace WerkPilot.Desktop;

public partial class App : Avalonia.Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                Log.Fatal(
                    exception,
                    "Unhandled application exception. IsTerminating={IsTerminating}",
                    args.IsTerminating);
            }
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Log.Error(
                args.Exception,
                "Unobserved task exception.");

            args.SetObserved();
        };

        QuestPdfLicenseConfigurator.Configure();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        // Create a plain visible Avalonia window first. It deliberately has
        // no application services or view model dependencies.
        var bootstrapWindow = new Avalonia.Controls.Window
        {
            Title = "WerkPilot",
            Width = 520,
            Height = 220,
            WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen,
            Content = new Avalonia.Controls.TextBlock
            {
                Text = "WerkPilot wird gestartet ...",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                FontSize = 22
            }
        };

        desktop.MainWindow = bootstrapWindow;
        bootstrapWindow.Show();

        base.OnFrameworkInitializationCompleted();

        _ = InitializeApplicationAsync(desktop, bootstrapWindow);
    }

    private async Task InitializeApplicationAsync(
        IClassicDesktopStyleApplicationLifetime desktop,
        Avalonia.Controls.Window bootstrapWindow)
    {
        try
        {
            _host = CreateHost();
            await _host.StartAsync();

            using var scope = _host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WerkPilotDbContext>();
            await DbInitializer.InitializeAsync(db);

            // Bootstrap succeeded: close placeholder and show login.
            bootstrapWindow.Close();
            ShowLogin(desktop);

            desktop.Exit += async (_, _) =>
            {
                if (_host is not null)
                    await _host.StopAsync();
            };
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "WerkPilot konnte nicht vollständig initialisiert werden.");

            bootstrapWindow.Title = "WerkPilot – Startfehler";
            bootstrapWindow.Content = new Avalonia.Controls.TextBlock
            {
                Text = UiErrorFormatter.Startup(exception, "Start"),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(24),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
        }
    }


    private void ShowLogin(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (_host is null)
            return;

        var loginViewModel = _host.Services.GetRequiredService<LoginViewModel>();
        var loginWindow = new LoginWindow { DataContext = loginViewModel };

        loginViewModel.LoginSucceeded += (_, result) =>
        {
            if (!result.UserId.HasValue)
                return;

            if (result.MustChangePassword)
            {
                var changeViewModel = new ChangePasswordViewModel(
                    _host.Services.GetRequiredService<AuthenticationService>(),
                    result.UserId.Value);

                var changeWindow = new ChangePasswordWindow
                {
                    DataContext = changeViewModel
                };

                changeViewModel.PasswordChanged += (_, _) =>
                {
                    changeWindow.Close();
                    ShowMainWindow(desktop);
                };

                desktop.MainWindow = changeWindow;
                changeWindow.Show();
                loginWindow.Close();
                return;
            }

            ShowMainWindow(desktop);
            loginWindow.Close();
        };

        desktop.MainWindow = loginWindow;
        loginWindow.Show();
    }

    private void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (_host is null)
            return;

        var viewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
        var mainWindow = new MainWindow
        {
            DataContext = viewModel,
            Title = $"WerkPilot 0.12.24 RC – {viewModel.SignedInUser}"
        };

        viewModel.LogoutRequested += (_, _) =>
        {
            mainWindow.Close();
            ShowLogin(desktop);
        };

        viewModel.OpenOffersRequested += (_, _) =>
        {
            var offersWindow = new OffersWindow
            {
                DataContext = _host.Services.GetRequiredService<OffersViewModel>()
            };
            offersWindow.Show(mainWindow);
        };

        viewModel.OpenCompanySettingsRequested += (_, _) =>
        {
            var settingsWindow = new CompanySettingsWindow
            {
                DataContext = _host.Services.GetRequiredService<CompanySettingsViewModel>()
            };
            settingsWindow.Show(mainWindow);
        };

        viewModel.OpenCalculationRequested += (_, _) =>
        {
            var calculationWindow = new CalculationWindow
            {
                DataContext = _host.Services.GetRequiredService<CalculationViewModel>()
            };
            calculationWindow.Show(mainWindow);
        };

        viewModel.OpenMaterialRequested += (_, _) =>
        {
            var materialWindow = new MaterialWindow
            {
                DataContext = _host.Services.GetRequiredService<MaterialViewModel>()
            };
            materialWindow.Show(mainWindow);
        };

        viewModel.OpenPurchaseListsRequested += (_, _) =>
        {
            var purchaseListsWindow = new PurchaseListsWindow
            {
                DataContext = _host.Services.GetRequiredService<PurchaseListsViewModel>()
            };
            purchaseListsWindow.Show(mainWindow);
        };

        viewModel.OpenProjectsRequested += (_, _) =>
        {
            var projectsWindow = new ProjectsWindow
            {
                DataContext = _host.Services.GetRequiredService<ProjectsViewModel>()
            };
            projectsWindow.Show(mainWindow);
        };

        viewModel.OpenDocumentsRequested += (_, _) =>
        {
            var documentsWindow = new DocumentsWindow
            {
                DataContext = _host.Services.GetRequiredService<DocumentsViewModel>()
            };
            documentsWindow.Show(mainWindow);
        };

        viewModel.OpenNotificationsRequested += (_, _) =>
        {
            var window = new NotificationsWindow
            {
                DataContext = _host.Services.GetRequiredService<NotificationsViewModel>()
            };
            window.Show(mainWindow);
        };



        viewModel.OpenGlobalSearchRequested += (_, _) =>
        {
            var searchViewModel = _host.Services.GetRequiredService<GlobalSearchViewModel>();
            var searchWindow = new GlobalSearchWindow { DataContext = searchViewModel };

            searchViewModel.ResultOpenRequested += async (_, result) =>
            {
                await _host.Services.GetRequiredService<WorkbenchService>().RecordOpenAsync(result);
                switch (result.Type)
                {
                    case SearchResultType.Customer:
                        var customer = viewModel.Customers.FirstOrDefault(x => x.Id == result.EntityId);
                        if (customer is not null)
                            viewModel.SelectedCustomer = customer;
                        searchWindow.Close();
                        break;
                    case SearchResultType.Offer:
                        viewModel.OpenOffersCommand.Execute(null);
                        break;
                    case SearchResultType.Project:
                        viewModel.OpenProjectsCommand.Execute(null);
                        break;
                    case SearchResultType.Material:
                        viewModel.OpenMaterialCommand.Execute(null);
                        break;
                    case SearchResultType.Document:
                        viewModel.OpenDocumentsCommand.Execute(null);
                        break;
                }
            };

            searchWindow.Show(mainWindow);
        };


        viewModel.OpenWorkbenchRequested += (_, _) =>
        {
            var workbenchViewModel = _host.Services.GetRequiredService<WorkbenchViewModel>();
            var window = new WorkbenchWindow { DataContext = workbenchViewModel };

            workbenchViewModel.ItemOpenRequested += (_, result) =>
            {
                switch (result.Type)
                {
                    case SearchResultType.Customer:
                        var customer = viewModel.Customers.FirstOrDefault(x => x.Id == result.EntityId);
                        if (customer is not null)
                            viewModel.SelectedCustomer = customer;
                        window.Close();
                        break;
                    case SearchResultType.Offer:
                        viewModel.OpenOffersCommand.Execute(null);
                        break;
                    case SearchResultType.Project:
                        viewModel.OpenProjectsCommand.Execute(null);
                        break;
                    case SearchResultType.Material:
                        viewModel.OpenMaterialCommand.Execute(null);
                        break;
                    case SearchResultType.Document:
                        viewModel.OpenDocumentsCommand.Execute(null);
                        break;
                }
            };

            window.Show(mainWindow);
        };

        viewModel.OpenCrmJournalRequested += (_, _) =>
        {
            var window = new CrmJournalWindow
            {
                DataContext = _host.Services.GetRequiredService<CrmJournalViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenCustomer360Requested += (_, _) =>
        {
            var window = new Customer360Window
            {
                DataContext = _host.Services.GetRequiredService<Customer360ViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenTimeTrackingRequested += (_, _) =>
        {
            var window = new TimeTrackingWindow
            {
                DataContext = _host.Services.GetRequiredService<TimeTrackingViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenProjectCostControllingRequested += (_, _) =>
        {
            var window = new ProjectCostControllingWindow
            {
                DataContext = _host.Services.GetRequiredService<ProjectCostControllingViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenInventoryRequested += (_, _) =>
        {
            var window = new InventoryWindow
            {
                DataContext = _host.Services.GetRequiredService<InventoryViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenInventoryCountRequested += (_, _) =>
        {
            var window = new InventoryCountWindow
            {
                DataContext = _host.Services.GetRequiredService<InventoryCountViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenSupplierOrdersRequested += (_, _) =>
        {
            var window = new SupplierOrderWindow
            {
                DataContext = _host.Services.GetRequiredService<SupplierOrderViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenSupplierInvoicesRequested += (_, _) =>
        {
            var window = new SupplierInvoiceWindow
            {
                DataContext = _host.Services.GetRequiredService<SupplierInvoiceViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenSupplierLiquidityRequested += (_, _) =>
        {
            var window = new SupplierLiquidityWindow
            {
                DataContext = _host.Services.GetRequiredService<SupplierLiquidityViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenCustomerInvoicesRequested += (_, _) =>
        {
            var window = new CustomerInvoiceWindow
            {
                DataContext = _host.Services.GetRequiredService<CustomerInvoiceViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenReceivablesRequested += (_, _) =>
        {
            var window = new ReceivablesWindow
            {
                DataContext = _host.Services.GetRequiredService<ReceivablesViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenCustomerCreditNotesRequested += (_, _) =>
        {
            var window = new CustomerCreditNoteWindow
            {
                DataContext = _host.Services.GetRequiredService<CustomerCreditNoteViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenDunningNoticesRequested += (_, _) =>
        {
            var window = new DunningNoticeWindow
            {
                DataContext = _host.Services.GetRequiredService<DunningNoticeViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenDocumentEmailRequested += (_, _) =>
        {
            var window = new DocumentEmailWindow
            {
                DataContext = _host.Services.GetRequiredService<DocumentEmailViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenCustomerCommunicationRequested += (_, _) =>
        {
            var window = new CustomerCommunicationWindow
            {
                DataContext = _host.Services.GetRequiredService<CustomerCommunicationViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenCustomerFollowUpsRequested += (_, _) =>
        {
            var window = new CustomerFollowUpWindow
            {
                DataContext = _host.Services.GetRequiredService<CustomerFollowUpViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenMyWorkRequested += (_, _) =>
        {
            var window = new MyWorkWindow
            {
                DataContext = _host.Services.GetRequiredService<MyWorkViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenTeamWorkRequested += (_, _) =>
        {
            var window = new TeamWorkWindow
            {
                DataContext = _host.Services.GetRequiredService<TeamWorkViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenWorkReassignmentRequested += (_, _) =>
        {
            var window = new WorkReassignmentWindow
            {
                DataContext = _host.Services.GetRequiredService<WorkReassignmentViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenUserAbsencesRequested += (_, _) =>
        {
            var window = new UserAbsenceWindow
            {
                DataContext = _host.Services.GetRequiredService<UserAbsenceViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenBasicWorkflowAuditRequested += (_, _) =>
        {
            var window = new BasicWorkflowAuditWindow
            {
                DataContext = _host.Services.GetRequiredService<BasicWorkflowAuditViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenReleaseDiagnosticsRequested += (_, _) =>
        {
            var window = new ReleaseDiagnosticsWindow
            {
                DataContext = _host.Services.GetRequiredService<ReleaseDiagnosticsViewModel>()
            };
            window.Show(mainWindow);
        };

        viewModel.OpenFirstRunReadinessRequested += (_, _) =>
        {
            var window = new FirstRunReadinessWindow
            {
                DataContext = _host.Services.GetRequiredService<FirstRunReadinessViewModel>()
            };
            window.Show(mainWindow);
        };

        desktop.MainWindow = mainWindow;
        mainWindow.Show();
    }

    private static IHost CreateHost()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/werkpilot-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        return Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                var connectionString =
                    context.Configuration.GetConnectionString("WerkPilot");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "Keine WerkPilot-Datenbankverbindung konfiguriert. "
                        + "Setze ConnectionStrings__WerkPilot oder verwende "
                        + "die Development-Konfiguration.");
                }

                services.AddDbContext<WerkPilotDbContext>(o => o.UseNpgsql(connectionString));

                services.AddDbContextFactory<WerkPilotDbContext>(
                    options => options.UseNpgsql(connectionString));
                services.AddScoped<ICustomerRepository, CustomerRepository>();
                services.AddScoped<IAuditTrail, EfAuditTrail>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddSingleton<SessionContext>();
                services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
                services.AddScoped<AuthenticationService>();
                services.AddScoped<AuthorizationService>();
                services.AddScoped<UserService>();
                services.AddScoped<IOfferRepository, OfferRepository>();
                services.AddScoped<OfferService>();
                services.AddScoped<ICalculationRepository, CalculationRepository>();
                services.AddScoped<IMaterialRepository, MaterialRepository>();
                services.AddScoped<IMaterialCsvSerializer, SemicolonMaterialCsvSerializer>();
                services.AddScoped<MaterialService>();
                services.AddScoped<WerkPilot.Application.Calculation.PurchaseListService>();
                services.AddScoped<IPurchaseListSource, CalculationPurchaseListSource>();
                services.AddScoped<IPurchaseListRepository, PurchaseListRepository>();
                services.AddScoped<IPurchaseListCsvExporter, SemicolonPurchaseListCsvExporter>();
                services.AddScoped<WerkPilot.Application.Purchasing.PurchaseListService>();
                services.AddTransient<PurchaseListsViewModel>();
                services.AddScoped<IProjectRepository, ProjectRepository>();
                services.AddScoped<ProjectService>();
                services.AddScoped<DashboardService>();
                services.AddScoped<INotificationReadRepository, NotificationReadRepository>();
                services.AddScoped<NotificationService>();
                services.AddTransient<NotificationsViewModel>();
                services.AddScoped<GlobalSearchService>();
                services.AddTransient<GlobalSearchViewModel>();
                services.AddScoped<IWorkbenchRepository, WorkbenchRepository>();
                services.AddScoped<WorkbenchService>();
                services.AddTransient<WorkbenchViewModel>();
                services.AddTransient<ProjectsViewModel>();
                services.AddScoped<IDocumentRepository, DocumentRepository>();
                services.AddSingleton<IFileStorage, LocalFileStorage>();
                services.AddScoped<DocumentService>();
                services.AddTransient<DocumentsViewModel>();
                services.AddTransient<MaterialViewModel>();
                services.AddScoped<CalculationService>();
                services.AddTransient<CalculationViewModel>();
                services.AddScoped<IOfferDocumentExporter, QuestPdfOfferDocumentExporter>();
                services.AddScoped<OfferDocumentService>();
                services.AddScoped<IEmailSender, SmtpEmailSender>();
                services.AddSingleton<ISmtpDiagnostics, SmtpDiagnostics>();
                services.AddScoped<OfferEmailService>();
                services.AddScoped<ICompanyProfileRepository, CompanyProfileRepository>();
                services.AddScoped<CompanyProfileService>();
                services.AddTransient<CompanySettingsViewModel>();
                services.AddTransient<OffersViewModel>();
                services.AddTransient<LoginViewModel>();
                services.AddScoped<CustomerService>();
                services.AddScoped<ICustomerInteractionRepository, CustomerInteractionRepository>();
                services.AddScoped<CustomerInteractionService>();
                services.AddTransient<CrmJournalViewModel>();
                services.AddScoped<Customer360Service>();
                services.AddTransient<Customer360ViewModel>();
                services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
                services.AddScoped<TimeTrackingService>();
                services.AddScoped<ProjectTimeControllingService>();
                services.AddTransient<TimeTrackingViewModel>();
                services.AddScoped<IProjectActualCostRepository, ProjectActualCostRepository>();
                services.AddScoped<ProjectActualCostService>();
                services.AddScoped<ProjectCostControllingService>();
                services.AddScoped<ProjectProfitabilityService>();
                services.AddSingleton<IProjectClosingReportExporter, ProjectClosingReportExporter>();
                services.AddScoped<ProjectClosingReportService>();
                services.AddTransient<ProjectCostControllingViewModel>();
                services.AddScoped<IInventoryRepository, InventoryRepository>();
                services.AddScoped<InventoryService>();
                services.AddSingleton<IReorderSuggestionCsvExporter, ReorderSuggestionCsvExporter>();
                services.AddScoped<ReorderSuggestionService>();
                services.AddSingleton<IInventoryValuationCsvExporter, InventoryValuationCsvExporter>();
                services.AddScoped<InventoryValuationService>();
                services.AddTransient<InventoryViewModel>();
                services.AddScoped<IInventoryCountRepository, InventoryCountRepository>();
                services.AddSingleton<IInventoryCountCsvExporter, InventoryCountCsvExporter>();
                services.AddScoped<InventoryCountService>();
                services.AddTransient<InventoryCountViewModel>();
                services.AddScoped<ISupplierOrderRepository, SupplierOrderRepository>();
                services.AddSingleton<ISupplierOrderCsvExporter, SupplierOrderCsvExporter>();
                services.AddScoped<SupplierOrderService>();
                services.AddTransient<SupplierOrderViewModel>();
                services.AddScoped<ISupplierInvoiceRepository, SupplierInvoiceRepository>();
                services.AddSingleton<ISupplierInvoiceCsvExporter, SupplierInvoiceCsvExporter>();
                services.AddScoped<SupplierInvoiceService>();
                services.AddTransient<SupplierInvoiceViewModel>();
                services.AddTransient<SupplierLiquidityViewModel>();
                services.AddScoped<ICustomerInvoiceRepository, CustomerInvoiceRepository>();
                services.AddSingleton<ICustomerInvoiceCsvExporter, CustomerInvoiceCsvExporter>();
                services.AddSingleton<ICustomerInvoicePdfExporter, CustomerInvoicePdfExporter>();
                services.AddSingleton<DocumentArchiveService>();
                services.AddScoped<CustomerInvoiceService>();
                services.AddTransient<CustomerInvoiceViewModel>();
                services.AddTransient<ReceivablesViewModel>();
                services.AddScoped<ICustomerCreditNoteRepository, CustomerCreditNoteRepository>();
                services.AddSingleton<ICustomerCreditNoteCsvExporter, CustomerCreditNoteCsvExporter>();
                services.AddSingleton<ICustomerCreditNotePdfExporter, CustomerCreditNotePdfExporter>();
                services.AddScoped<CustomerCreditNoteService>();
                services.AddTransient<CustomerCreditNoteViewModel>();
                services.AddScoped<IDunningNoticeRepository, DunningNoticeRepository>();
                services.AddSingleton<IDunningNoticePdfExporter, DunningNoticePdfExporter>();
                services.AddScoped<DunningNoticeService>();
                services.AddTransient<DunningNoticeViewModel>();
                services.AddScoped<IDocumentEmailDispatchRepository, DocumentEmailDispatchRepository>();
                services.AddScoped<IDocumentEmailTemplateRepository, DocumentEmailTemplateRepository>();
                services.AddScoped<DocumentEmailTemplateService>();
                services.AddScoped<DocumentEmailCatalogService>();
                services.AddScoped<DocumentEmailService>();
                services.AddTransient<DocumentEmailViewModel>();
                services.AddScoped<CustomerCommunicationService>();
                services.AddTransient<CustomerCommunicationViewModel>();
                services.AddScoped<ICustomerFollowUpRepository, CustomerFollowUpRepository>();
                services.AddScoped<CustomerFollowUpService>();
                services.AddTransient<CustomerFollowUpViewModel>();
                services.AddScoped<MyWorkService>();
                services.AddTransient<MyWorkViewModel>();
                services.AddScoped<TeamWorkService>();
                services.AddTransient<TeamWorkViewModel>();
                services.AddScoped<WorkReassignmentService>();
                services.AddTransient<WorkReassignmentViewModel>();
                services.AddScoped<IUserAbsenceRepository, UserAbsenceRepository>();
                services.AddScoped<UserAbsenceService>();
                services.AddTransient<UserAbsenceViewModel>();
                services.AddScoped<BasicWorkflowAuditService>();
                services.AddTransient<BasicWorkflowAuditViewModel>();
                services.AddSingleton<ReleaseDiagnosticsService>();
                services.AddTransient<ReleaseDiagnosticsViewModel>();
                services.AddSingleton<FirstRunReadinessService>();
                services.AddTransient<FirstRunReadinessViewModel>();
                services.AddHostedService<DocumentEmailOutboxHostedService>();
                services.AddTransient<MainWindowViewModel>();
            })
            .Build();
    }
}
