using Microsoft.EntityFrameworkCore;
using OnlineStore.Models;
using System.Windows;
using System.Windows.Controls;

namespace OnlineStore.Pages
{
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
            LVCart.ItemsSource = App.CartItems;
            UpdateCart();
        }

        private void UpdateCart()
        {
            LVCart.Items.Refresh();
            TBlCartTotal.Text = $"Итого: {App.CartItems.Sum(p => p.TotalPrice):N2} руб.";
            BtCheckout.IsEnabled = App.CartItems.Count > 0;
        }

        private void BtDecrease_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var item = button.DataContext as CartItem;
            if (item == null) return;

            if (item.Quantity > 1)
                item.Quantity--;

            UpdateCart();
        }

        private void BtIncrease_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var item = button.DataContext as CartItem;
            if (item == null) return;

            try
            {
                var db = App.Context;
                int available = db.Products.Where(p => p.IdProduct == item.IdProduct).Select(p => p.QuantityProduct).FirstOrDefault();

                if (item.Quantity >= available)
                {
                    MessageBox.Show("Больше товара на складе нет.");
                    return;
                }

                item.Quantity++;
                UpdateCart();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось проверить остаток: " + ex.Message);
            }
        }

        private void BtRemove_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var item = button.DataContext as CartItem;
            if (item == null) return;

            App.CartItems.Remove(item);
            UpdateCart();
        }

        private bool CheckProducts(OnlineStoreContext db, List<CartItem> items)
        {
            foreach (var item in items)
            {
                var product = db.Products.FirstOrDefault(p => p.IdProduct == item.IdProduct);

                if (product == null || product.QuantityProduct < item.Quantity)
                {
                    MessageBox.Show("Недостаточно товара: " + item.NameProduct + ". Обновите корзину.");
                    return false;
                }

                if (product.PriceProduct != item.PriceProduct)
                {
                    item.PriceProduct = product.PriceProduct;
                    UpdateCart();
                    MessageBox.Show("Цена товара «" + item.NameProduct + "» изменилась. Проверьте новую сумму и оформите заказ снова.");
                    return false;
                }
            }
            return true;
        }

        private void BtCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null)
            {
                MessageBox.Show("Для оформления заказа нужно войти.");
                return;
            }
            if (App.CartItems.Count == 0) return;

            try
            {
                var db = App.Context;
                var user = db.Users.FirstOrDefault(p => p.IdUser == App.CurrentUser.IdUser);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден.");
                    return;
                }

                if (user.IsPhoneVerified != true || user.IsEmailVerified != true || user.IsAddressConfirmed != true || string.IsNullOrWhiteSpace(user.Address))
                {
                    MessageBox.Show("Сначала подтвердите телефон, email и адрес доставки в профиле.");
                    return;
                }

                var status = db.OrderStatuses.FirstOrDefault(p => p.NameOrderStatus == "Оформлен");
                if (status == null)
                {
                    MessageBox.Show("Не найден статус заказа «Оформлен».");
                    return;
                }

                var items = App.CartItems.ToList();

                if (!CheckProducts(db, items)) return;

                foreach (var item in items)
                {
                    var product = db.Products.First(p => p.IdProduct == item.IdProduct);
                    product.QuantityProduct -= item.Quantity;
                }

                var order = new Order
                {
                    IdUser = user.IdUser,
                    DateOrder = DateTime.Now,
                    IdOrderStatus = status.IdOrderStatus,
                    DeliveryAddress = user.Address
                };

                foreach (var item in items)
                {
                    var orderItem = new OrderItem
                    {
                        IdProduct = item.IdProduct,
                        QuantityOrderItem = item.Quantity,
                        PriceOrderItem = item.PriceProduct
                    };
                    order.OrderItems.Add(orderItem);
                }

                db.Orders.Add(order);
                db.SaveChanges();

                App.CartItems.Clear();
                UpdateCart();
                MessageBox.Show($"Заказ №{order.IdOrder} успешно оформлен!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось оформить заказ: " + ex.Message);
            }
        }
    }
}
