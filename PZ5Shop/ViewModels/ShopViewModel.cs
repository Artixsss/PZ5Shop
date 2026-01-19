using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using PZ5Shop.Data;
using PZ5Shop.Models;

namespace PZ5Shop.ViewModels
{
    public class ShopViewModel : ViewModelBase
    {
        private readonly DbService _dbService;
        private CategoryFilterItem _selectedCategory;
        private SortOption _selectedSort;

        public ShopViewModel()
        {
            _dbService = new DbService();
            Products = new ObservableCollection<Products>();
            Categories = new ObservableCollection<CategoryFilterItem>();
            SortOptions = new ObservableCollection<SortOption>();
            ProductsView = CollectionViewSource.GetDefaultView(Products);
            ProductsView.Filter = FilterProduct;
            AddToCartCommand = new RelayCommand(p => AddToCart(p as Products));
            LoadData();
        }

        public ObservableCollection<Products> Products { get; }
        public ICollectionView ProductsView { get; }
        public ObservableCollection<CategoryFilterItem> Categories { get; }
        public ObservableCollection<SortOption> SortOptions { get; }

        public CategoryFilterItem SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                ProductsView.Refresh();
            }
        }

        public SortOption SelectedSort
        {
            get => _selectedSort;
            set
            {
                _selectedSort = value;
                OnPropertyChanged();
                ApplySorting();
            }
        }

        public RelayCommand AddToCartCommand { get; }

        private void LoadData()
        {
            Products.Clear();
            foreach (var product in _dbService.GetProducts())
            {
                Products.Add(product);
            }

            Categories.Clear();
            Categories.Add(new CategoryFilterItem { Id = null, Name = "Все категории" });
            foreach (var category in _dbService.GetCategories())
            {
                Categories.Add(new CategoryFilterItem { Id = category.Id, Name = category.Name });
            }
            SelectedCategory = Categories.FirstOrDefault();

            SortOptions.Clear();
            SortOptions.Add(new SortOption { Name = "По названию", SortProperty = "Name" });
            SortOptions.Add(new SortOption { Name = "По цене", SortProperty = "Price" });
            SelectedSort = SortOptions.FirstOrDefault();
        }

        private bool FilterProduct(object item)
        {
            if (SelectedCategory == null || SelectedCategory.Id == null)
            {
                return true;
            }

            var product = item as Products;
            return product != null && product.CategoryId == SelectedCategory.Id.Value;
        }

        private void ApplySorting()
        {
            ProductsView.SortDescriptions.Clear();
            if (SelectedSort != null)
            {
                ProductsView.SortDescriptions.Add(new SortDescription(SelectedSort.SortProperty, ListSortDirection.Ascending));
            }
            ProductsView.Refresh();
        }

        private void AddToCart(Products product)
        {
            if (product == null)
            {
                return;
            }
            AppState.Current.AddToCart(product);
        }
    }
}
