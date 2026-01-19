using System;
using System.Collections.ObjectModel;
using System.Linq;
using PZ5Shop.Data;
using PZ5Shop.Models;

namespace PZ5Shop.ViewModels
{
    public class AppState : ViewModelBase
    {
        private static readonly Lazy<AppState> InstanceValue = new Lazy<AppState>(() => new AppState());
        private readonly DbService _dbService;
        private readonly SessionStorage _sessionStorage;
        private Users _currentUser;

        public static AppState Current => InstanceValue.Value;

        public event EventHandler OrdersChanged;

        public Users CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAuthenticated));
                OnPropertyChanged(nameof(UserDisplayName));
            }
        }

        public bool IsAuthenticated => CurrentUser != null;

        public string UserDisplayName => CurrentUser == null ? string.Empty : (CurrentUser.LastName + " " + CurrentUser.FirstName).Trim();

        public ObservableCollection<CartItemViewModel> CartItems { get; }

        private AppState()
        {
            _dbService = new DbService();
            _sessionStorage = new SessionStorage();
            CartItems = new ObservableCollection<CartItemViewModel>();
        }

        public void Initialize()
        {
            var userId = _sessionStorage.LoadUserId();
            if (userId.HasValue)
            {
                var user = _dbService.GetUserById(userId.Value);
                if (user != null)
                {
                    CurrentUser = user;
                    LoadCartFromDb();
                    return;
                }
            }

            CurrentUser = null;
            CartItems.Clear();
        }

        public bool Login(string username, string password)
        {
            var user = _dbService.GetUserByCredentials(username, password);
            if (user == null)
            {
                return false;
            }

            var guestCart = CartItems.ToList();
            CurrentUser = user;
            _sessionStorage.SaveUserId(user.Id);
            LoadCartFromDb();

            if (guestCart.Count > 0)
            {
                foreach (var item in guestCart)
                {
                    AddToCart(item.Product, item.Quantity);
                }
            }

            return true;
        }

        public Users Register(string lastName, string firstName, string middleName, string username, string email, string password)
        {
            if (_dbService.UsernameExists(username) || _dbService.EmailExists(email))
            {
                return null;
            }

            var guestCart = CartItems.ToList();
            var user = _dbService.CreateUser(lastName, firstName, middleName, username, email, password);
            CurrentUser = user;
            _sessionStorage.SaveUserId(user.Id);
            LoadCartFromDb();

            if (guestCart.Count > 0)
            {
                foreach (var item in guestCart)
                {
                    AddToCart(item.Product, item.Quantity);
                }
            }

            return user;
        }

        public void Logout()
        {
            CurrentUser = null;
            _sessionStorage.SaveUserId(null);
            CartItems.Clear();
        }

        public void AddToCart(Products product, int quantity = 1)
        {
            if (product == null || quantity <= 0)
            {
                return;
            }

            var existing = CartItems.FirstOrDefault(c => c.Product.Id == product.Id);
            if (existing != null)
            {
                var newQuantity = existing.Quantity + quantity;
                existing.Quantity = newQuantity;
                if (IsAuthenticated)
                {
                    _dbService.UpsertCartItem(CurrentUser.Id, product.Id, newQuantity);
                }
                return;
            }

            var newItem = new CartItemViewModel(product, quantity);
            CartItems.Add(newItem);
            if (IsAuthenticated)
            {
                _dbService.UpsertCartItem(CurrentUser.Id, product.Id, quantity);
            }
        }

        public void UpdateQuantity(Products product, int newQuantity)
        {
            if (product == null)
            {
                return;
            }

            var existing = CartItems.FirstOrDefault(c => c.Product.Id == product.Id);
            if (existing == null)
            {
                return;
            }

            if (newQuantity <= 0)
            {
                RemoveFromCart(product);
                return;
            }

            existing.Quantity = newQuantity;
            if (IsAuthenticated)
            {
                _dbService.UpsertCartItem(CurrentUser.Id, product.Id, newQuantity);
            }
        }

        public void RemoveFromCart(Products product)
        {
            if (product == null)
            {
                return;
            }

            var existing = CartItems.FirstOrDefault(c => c.Product.Id == product.Id);
            if (existing != null)
            {
                CartItems.Remove(existing);
            }

            if (IsAuthenticated)
            {
                _dbService.RemoveCartItem(CurrentUser.Id, product.Id);
            }
        }

        public void ClearCart()
        {
            CartItems.Clear();
            if (IsAuthenticated)
            {
                _dbService.ClearCart(CurrentUser.Id);
            }
        }

        public Orders Checkout()
        {
            if (!IsAuthenticated)
            {
                return null;
            }

            var lines = CartItems.Select(c => new CartLine
            {
                ProductId = c.Product.Id,
                Quantity = c.Quantity,
                UnitPrice = c.Product.Price
            }).ToList();

            if (lines.Count == 0)
            {
                return null;
            }

            var order = _dbService.CreateOrder(CurrentUser.Id, lines);
            ClearCart();
            OrdersChanged?.Invoke(this, EventArgs.Empty);
            return order;
        }

        public void LoadCartFromDb()
        {
            CartItems.Clear();
            if (!IsAuthenticated)
            {
                return;
            }

            var items = _dbService.GetCartItems(CurrentUser.Id);
            foreach (var item in items)
            {
                if (item.Products != null)
                {
                    CartItems.Add(new CartItemViewModel(item.Products, item.Quantity));
                }
            }
        }
    }
}
