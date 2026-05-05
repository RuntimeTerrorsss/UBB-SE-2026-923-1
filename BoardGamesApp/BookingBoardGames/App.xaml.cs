// <copyright file="App.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using BookingBoardGames.Data;
using BookingBoardGames.Src.Mapper;
using BookingBoardGames.Src.Repositories;
using BookingBoardGames.Src.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.


namespace BookingBoardGames
{
    //TODO: rename repositoryPayment and PaymentRepository, finish conversation repo
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static readonly string BaseApiUrl = "http://localhost:5000/api/";
        public static readonly System.Net.Http.HttpClient Client = new System.Net.Http.HttpClient { BaseAddress = new Uri(BaseApiUrl) };
        private Window? window;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// Gets the initialization of the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(DatabaseConfig.ResolveConnectionString())
                .Options;

            AppDbContext = new AppDbContext(options);

            // Repositories
            UserRepository = new UserRepository(AppDbContext);
            GameRepository = new GamesRepository(AppDbContext);
            RentalRepository = new RentalRepository(AppDbContext);
            PaymentRepository = new PaymentRepository(AppDbContext);
            HistoryRepository = new RepositoryPayment(AppDbContext);
            ConversationRepository = new ConversationRepository(AppDbContext);

            // Services
            ConversationNotifier = new ConversationNotifier();
            GlobalGeographicalService = new GeographicalService();
            RentalService = new RentalService(RentalRepository, GameRepository);
            ReceiptService = new ReceiptService(UserRepository, RentalService, GameRepository);
            CardPaymentService = new CardPaymentService((PaymentRepository)PaymentRepository, UserRepository, (ReceiptService)ReceiptService, RentalService);
            MapService = new MapService();
            ServicePayment = new ServicePayment(HistoryRepository, ReceiptService);
            CashPaymentService = new CashPaymentService(PaymentRepository, new CashPaymentMapper(), ReceiptService);
            BookingService = new BookingService(GameRepository, RentalRepository, UserRepository);
            SearchAndFilterService = new SearchAndFilterService(GameRepository, UserRepository, RentalRepository, GlobalGeographicalService);
        }

        // AppDbContext
        public static AppDbContext? AppDbContext { get; private set; }

        // Repositories
        public static IUserRepository? UserRepository { get; private set; }

        public static InterfaceGamesRepository? GameRepository { get; private set; }

        public static IRentalRepository? RentalRepository { get; private set; }

        public static IPaymentRepository? PaymentRepository { get; private set; }

        public static IRepositoryPayment? HistoryRepository { get; private set; }

        public static IConversationRepository? ConversationRepository { get; private set; }

        // Services
        public static IConversationNotifier? ConversationNotifier { get; private set; }

        public static InterfaceGeographicalService? GlobalGeographicalService { get; private set; }

        public static IRentalService? RentalService { get; private set; }

        public static IReceiptService? ReceiptService { get; private set; }

        public static ICardPaymentService? CardPaymentService { get; private set; }

        public static IMapService? MapService { get; private set; }

        public static IServicePayment? ServicePayment { get; private set; }

        public static ICashPaymentService? CashPaymentService { get; private set; }

        public static InterfaceBookingService? BookingService { get; private set; }

        public static InterfaceSearchAndFilterService? SearchAndFilterService { get; private set; }

        public int DashboardUser { get; set; } = 3;

        public int NoChatsUser { get; set; } = 8;

        public Window? Window => this.window;

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            DatabaseBootstrap.Initialize();

            this.window = new MainWindow();
            this.window.Activate();

            try
            {
                GlobalGeographicalService = await GeographicalService.LoadFromFileAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeographicalService initialization failed: {ex.Message}");
            }
        }
    }
}
