using System.Collections.ObjectModel;
using System.Linq;
using PZ5Shop.Data;

namespace PZ5Shop.ViewModels
{
    public class OrdersViewModel : ViewModelBase
    {
        private readonly DbService _dbService;
        private OrderSummaryViewModel _selectedOrder;

        public OrdersViewModel()
        {
            _dbService = new DbService();
            Orders = new ObservableCollection<OrderSummaryViewModel>();
            OrderItems = new ObservableCollection<OrderItemViewModel>();
        }

        public ObservableCollection<OrderSummaryViewModel> Orders { get; }
        public ObservableCollection<OrderItemViewModel> OrderItems { get; }

        public bool IsAuthenticated => AppState.Current.IsAuthenticated;

        public OrderSummaryViewModel SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                LoadOrderItems();
            }
        }

        public void Refresh()
        {
            Orders.Clear();
            OrderItems.Clear();
            if (!AppState.Current.IsAuthenticated)
            {
                OnPropertyChanged(nameof(IsAuthenticated));
                return;
            }

            var items = _dbService.GetOrdersForUser(AppState.Current.CurrentUser.Id);
            foreach (var order in items)
            {
                Orders.Add(new OrderSummaryViewModel
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Status = order.OrderStatus != null ? order.OrderStatus.Name : string.Empty
                });
            }

            SelectedOrder = Orders.FirstOrDefault();
            OnPropertyChanged(nameof(IsAuthenticated));
        }

        private void LoadOrderItems()
        {
            OrderItems.Clear();
            if (_selectedOrder == null)
            {
                return;
            }

            var items = _dbService.GetOrderItems(_selectedOrder.Id);
            foreach (var item in items)
            {
                OrderItems.Add(new OrderItemViewModel
                {
                    ProductName = item.Products != null ? item.Products.Name : string.Empty,
                    Quantity = item.Quantity,
                    PriceAtOrder = item.PriceAtOrder
                });
            }
        }
    }
}
