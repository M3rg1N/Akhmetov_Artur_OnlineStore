using OnlineStore.Models;
using System.Windows;
using System.Windows.Controls;

namespace OnlineStore.Pages
{
    public partial class CatalogPage : Page
    {
        private List<Product> products = new List<Product>();

        public CatalogPage()
        {
            InitializeComponent();

            try
            {
                var db = App.Context;
                products = db.Products.OrderBy(p => p.NameProduct).ToList();

                var categories = db.Categories.OrderBy(p => p.NameCategory).ToList();
                categories.Insert(0, new Category { IdCategory = 0, NameCategory = "Все категории" });
                CBCategory.ItemsSource = categories;
                CBCategory.SelectedIndex = 0;

                UpdateProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить товары: " + ex.Message);
            }
        }

        private void TBSearchProduct_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void CBCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void UpdateProducts()
        {
            if (LVProducts == null || TBSearchProduct == null || CBCategory == null) return;

            var result = products.ToList();
            var category = CBCategory.SelectedItem as Category;

            if (category != null && category.IdCategory != 0)
                result = result.Where(p => p.IdCategory == category.IdCategory).ToList();

            result = result.Where(p => p.NameProduct.ToLower().Contains(TBSearchProduct.Text.Trim().ToLower())).ToList();
            LVProducts.ItemsSource = result;
        }

        private void BtAddCart_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null)
            {
                MessageBox.Show("Чтобы добавить товар в корзину, войдите или зарегистрируйтесь.");
                return;
            }

            var button = sender as Button;
            if (button == null) return;
            var product = button.DataContext as Product;
            if (product == null) return;

            var item = App.CartItems.FirstOrDefault(p => p.IdProduct == product.IdProduct);

            if (item == null)
            {
                if (product.QuantityProduct < 1)
                {
                    MessageBox.Show("Товара нет в наличии.");
                    return;
                }

                var newItem = new CartItem
                {
                    IdProduct = product.IdProduct,
                    NameProduct = product.NameProduct,
                    PriceProduct = product.PriceProduct,
                    Quantity = 1
                };
                App.CartItems.Add(newItem);
            }
            else
            {
                if (item.Quantity >= product.QuantityProduct)
                {
                    MessageBox.Show("Больше товара на складе нет.");
                    return;
                }
                item.Quantity++;
            }

            MessageBox.Show("Товар добавлен в корзину.");
        }
    }
}
