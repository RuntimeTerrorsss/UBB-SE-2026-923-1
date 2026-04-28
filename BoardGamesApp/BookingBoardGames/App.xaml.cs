using BookingBoardGames.Src.Mapper;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;
using BookingBoardgamesILoveBan;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using BookingBoardGames.Data;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BookingBoardGames
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? window;

        /// <summary>
        /// Gets the initialization of the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        ///

        // AppDbContext
        public static AppDbContext AppDbContext { get; private set; }

        // Repositories
        public static UserRepository UserRepository { get; private set; }

        public static InterfaceGamesRepository GameRepository { get; private set; }

        public static IRentalRepository RentalRepository { get; private set; }

        public static PaymentRepository PaymentRepository { get; private set; }

        public static IRepositoryPayment HistoryRepository { get; private set; }

        public static ConversationRepository ConversationRepository { get; private set; }


        // Services
        public static InterfaceGeographicalService? GlobalGeoService { get; private set; }

        public static IRentalService RentalService { get; private set; }

        public static ReceiptService ReceiptService { get; private set; }

        public static CardPaymentService CardPaymentService { get; private set; }

        public static MapService MapService { get; private set; }

        public static ServicePayment ServicePayment { get; private set; }

        public static CashPaymentService CashPaymentService { get; private set; }

        public static InterfaceBookingService BookingService { get; private set; }

        public static InterfaceSearchAndFilterService SearchAndFilterService { get; private set; }


        public int DashboardUser = 3;
        public int NoChatsUser = 8;

        public Window? Window => window;

        public App()
        {
            this.InitializeComponent();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(DatabaseConfig.ConnectionString)
                .Options;

            AppDbContext = new AppDbContext(options);

            // Repositories
            UserRepository = new UserRepository(); // add context
            GameRepository = new GamesRepository(AppDbContext);
            RentalRepository = new RentalRepository(AppDbContext);
            PaymentRepository = new PaymentRepository(AppDbContext);
            HistoryRepository = new RepositoryPayment(); // add context
            ConversationRepository = new ConversationRepository(); // add context

            // Services
            GlobalGeoService = new GeographicalService();
            RentalService = new RentalService(RentalRepository, GameRepository);
            ReceiptService = new ReceiptService(UserRepository, RentalService, GameRepository); // TODO: rename parameter (request => rental)
            CardPaymentService = new CardPaymentService(PaymentRepository, UserRepository, ReceiptService, RentalService); // TODO: rename parameter
            MapService = new MapService();
            ServicePayment = new ServicePayment(HistoryRepository, ReceiptService);
            CashPaymentService = new CashPaymentService(PaymentRepository, new CashPaymentMapper(), ReceiptService);
            BookingService = new BookingService(GameRepository, RentalRepository, UserRepository); // huh?
            SearchAndFilterService = new SearchAndFilterService(GameRepository, UserRepository, RentalRepository, GlobalGeoService); // astept sa gate cipicu
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            DatabaseBootstrap.Initialize();

            try
            {
                GlobalGeoService = await GeographicalService.LoadFromFileAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeographicalService initialization failed: {ex.Message}");
            }

            this.window = new MainWindow();
            this.window.Activate();
        }
    }
}
