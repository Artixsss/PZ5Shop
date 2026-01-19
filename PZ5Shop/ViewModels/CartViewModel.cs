using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using PZ5Shop.Views;

namespace PZ5Shop.ViewModels
{
    public class CartViewModel : ViewModelBase
    {
        public CartViewModel()
        {
            AppState.Current.CartItems.CollectionChanged += OnCartItemsChanged;
            foreach (var item in AppState.Current.CartItems)
            {
                item.PropertyChanged += OnCartItemPropertyChanged;
            }

            IncreaseCommand = new RelayCommand(p => ChangeQuantity(p as CartItemViewModel, 1));
            DecreaseCommand = new RelayCommand(p => ChangeQuantity(p as CartItemViewModel, -1));
            RemoveCommand = new RelayCommand(p => RemoveItem(p as CartItemViewModel));
            CheckoutCommand = new RelayCommand(_ => Checkout());
        }

        public System.Collections.ObjectModel.ObservableCollection<CartItemViewModel> Items => AppState.Current.CartItems;

        public int UniqueCount => Items.Count;

        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);

        public RelayCommand IncreaseCommand { get; }
        public RelayCommand DecreaseCommand { get; }
        public RelayCommand RemoveCommand { get; }
        public RelayCommand CheckoutCommand { get; }

        private void OnCartItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CartItemViewModel item in e.NewItems)
                {
                    item.PropertyChanged += OnCartItemPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (CartItemViewModel item in e.OldItems)
                {
                    item.PropertyChanged -= OnCartItemPropertyChanged;
                }
            }

            OnPropertyChanged(nameof(UniqueCount));
            OnPropertyChanged(nameof(TotalAmount));
        }

        private void OnCartItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItemViewModel.Quantity) || e.PropertyName == nameof(CartItemViewModel.TotalPrice))
            {
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        private void ChangeQuantity(CartItemViewModel item, int delta)
        {
            if (item == null)
            {
                return;
            }

            var newQuantity = item.Quantity + delta;
            AppState.Current.UpdateQuantity(item.Product, newQuantity);
            OnPropertyChanged(nameof(UniqueCount));
            OnPropertyChanged(nameof(TotalAmount));
        }

        private void RemoveItem(CartItemViewModel item)
        {
            if (item == null)
            {
                return;
            }

            AppState.Current.RemoveFromCart(item.Product);
            OnPropertyChanged(nameof(UniqueCount));
            OnPropertyChanged(nameof(TotalAmount));
        }

        private void Checkout()
        {
            if (!AppState.Current.IsAuthenticated)
            {
                var loginWindow = new LoginWindow { Owner = Application.Current.MainWindow };
                var result = loginWindow.ShowDialog();
                if (result != true)
                {
                    return;
                }
            }

            if (!AppState.Current.IsAuthenticated)
            {
                return;
            }

            if (Items.Count == 0)
            {
                MessageBox.Show("Корзина пуста", "Оформление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var order = AppState.Current.Checkout();
            if (order != null)
            {
                MessageBox.Show("Заказ оформлен", "Оформление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
