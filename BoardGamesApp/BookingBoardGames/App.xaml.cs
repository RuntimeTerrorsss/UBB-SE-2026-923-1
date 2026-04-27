using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BookingBoardGames.Src.Mapper;
using BookingBoardGames.Src.Models;
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
using SearchAndBook.Services;
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

        public static InterfaceGeographicalService? GlobalGeoService { get; private set; }

        public static UserRepository UserRepository { get; private set; } = new UserRepository();

        public static IGameRepository GameRepository { get; private set; } = new GameRepository();

        public static IRequestRepository RequestRepository { get; private set; } = new RequestRepository();

        public static IRequestService RequestService { get; private set; } = new RequestService(RequestRepository, GameRepository);

        public static PaymentRepository PaymentRepository { get; private set; } = new PaymentRepository();

        public static ReceiptService ReceiptService { get; private set; } = new ReceiptService(UserRepository, RequestService, GameRepository);

        public static CardPaymentService CardPaymentService { get; private set; } = new CardPaymentService(PaymentRepository,
            UserRepository, ReceiptService, RequestService);

        public static MapService MapService { get; private set; } = new MapService();

        public static IRepositoryPayment HistoryRepository = new RepositoryPayment();

        public static ServicePayment ServicePayment { get; private set; } = new ServicePayment(HistoryRepository,
            ReceiptService);

        public static CashPaymentService CashPaymentService { get; private set; } = new CashPaymentService(PaymentRepository,
            new CashPaymentMapper(), ReceiptService);

        public static ConversationRepository ConversationRepository { get; private set; } = new ConversationRepository();

        public int DashboardUser = 3;
        public int NoChatsUser = 8;

        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.window = new MainWindow();
            this.window.Activate();
        }
    }
}
