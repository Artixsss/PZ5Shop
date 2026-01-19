using PZ5Shop.Models;

namespace PZ5Shop.ViewModels
{
    public class CartItemViewModel : ViewModelBase
    {
        private int _quantity;

        public CartItemViewModel(Products product, int quantity)
        {
            Product = product;
            _quantity = quantity;
        }

        public Products Product { get; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value)
                {
                    return;
                }
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public decimal TotalPrice => Product == null ? 0m : Product.Price * Quantity;
    }
}
