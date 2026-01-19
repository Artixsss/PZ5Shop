using System.ComponentModel;
using System.Windows;
using PZ5Shop.Views;

namespace PZ5Shop.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            AppState.Current.Initialize();
            ShopViewModel = new ShopViewModel();
            CartViewModel = new CartViewModel();
            OrdersViewModel = new OrdersViewModel();
            CurrentView = ShopViewModel;

            NavigateShopCommand = new RelayCommand(_ => CurrentView = ShopViewModel);
            NavigateCartCommand = new RelayCommand(_ => CurrentView = CartViewModel);
            NavigateOrdersCommand = new RelayCommand(_ => CurrentView = OrdersViewModel, _ => IsAuthenticated);
            ShowLoginCommand = new RelayCommand(_ => ShowLogin());
            ShowRegisterCommand = new RelayCommand(_ => ShowRegister());
            LogoutCommand = new RelayCommand(_ => Logout());

            AppState.Current.PropertyChanged += OnStateChanged;
            AppState.Current.OrdersChanged += OnOrdersChanged;

            if (IsAuthenticated)
            {
                OrdersViewModel.Refresh();
            }
        }

        public ShopViewModel ShopViewModel { get; }
        public CartViewModel CartViewModel { get; }
        public OrdersViewModel OrdersViewModel { get; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public bool IsAuthenticated => AppState.Current.IsAuthenticated;
        public string UserDisplayName => AppState.Current.UserDisplayName;

        public RelayCommand NavigateShopCommand { get; }
        public RelayCommand NavigateCartCommand { get; }
        public RelayCommand NavigateOrdersCommand { get; }
        public RelayCommand ShowLoginCommand { get; }
        public RelayCommand ShowRegisterCommand { get; }
        public RelayCommand LogoutCommand { get; }

        private void OnStateChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppState.CurrentUser) || e.PropertyName == nameof(AppState.IsAuthenticated))
            {
                OnPropertyChanged(nameof(IsAuthenticated));
                OnPropertyChanged(nameof(UserDisplayName));
                OrdersViewModel.Refresh();
                if (!IsAuthenticated && CurrentView == OrdersViewModel)
                {
                    CurrentView = ShopViewModel;
                }
            }
        }

        private void OnOrdersChanged(object sender, System.EventArgs e)
        {
            OrdersViewModel.Refresh();
        }

        private void ShowLogin()
        {
            var window = new LoginWindow { Owner = Application.Current.MainWindow };
            var result = window.ShowDialog();
            if (result == true)
            {
                OrdersViewModel.Refresh();
            }
        }

        private void ShowRegister()
        {
            var window = new RegisterWindow { Owner = Application.Current.MainWindow };
            var result = window.ShowDialog();
            if (result == true)
            {
                OrdersViewModel.Refresh();
            }
        }

        private void Logout()
        {
            AppState.Current.Logout();
        }
    }
}
