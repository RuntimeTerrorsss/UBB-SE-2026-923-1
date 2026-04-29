using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BookingBoardGames;
using BookingBoardGames.Data;
using BookingBoardGames.Src.Mapper;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

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

        //public static AppDbContext Context = new AppDbContext();

        public static InterfaceGeographicalService? GlobalGeoService { get; private set; }

        public static IUserRepository UserRepository { get; private set; } = new UserRepository();

        public static InterfaceGamesRepository GameRepository { get; private set; } = new GamesRepository();

        public static IRentalRepository RentalRepository { get; private set; } = new RentalRepository();

        public static IRentalService RentalService { get; private set; } = new RentalService(RentalRepository, GameRepository);

        public static IPaymentRepository PaymentRepository { get; private set; } = new PaymentRepository();

        public static ReceiptService ReceiptService { get; private set; } = new ReceiptService(UserRepository, RentalService, GameRepository);

        public static CardPaymentService CardPaymentService { get; private set; } = new CardPaymentService(PaymentRepository,
            UserRepository, ReceiptService, RentalService);

        public static MapService MapService { get; private set; } = new MapService();

        public static IRepositoryPayment HistoryRepository = new RepositoryPayment();

        public static ServicePayment ServicePayment { get; private set; } = new ServicePayment(HistoryRepository,
            ReceiptService);

        public static CashPaymentService CashPaymentService { get; private set; } = new CashPaymentService(PaymentRepository,
            new CashPaymentMapper(), ReceiptService);

        public static ConversationRepository ConversationRepository { get; private set; } = new ConversationRepository();

        // TODO add request repo instead of rental repo
        public static InterfaceBookingService BookingService { get; private set; } = new BookingService(GameRepository, RentalRepository, UserRepository);

        public static GeographicalService GeographicalService { get; private set; } = new GeographicalService();

        // TODO add request repo instead of rental repo
        public static InterfaceSearchAndFilterService SearchAndFilterService { get; private set; } = new SearchAndFilterService(GameRepository, UserRepository, ..., GeographicalService);

        public int DashboardUser = 3;
        public int NoChatsUser = 8;

        public Window? Window => window;

        public App()
        {
            this.InitializeComponent();
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
