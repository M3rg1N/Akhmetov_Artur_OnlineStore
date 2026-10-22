using Microsoft.EntityFrameworkCore;
using OnlineStore.Helpers;
using OnlineStore.Models;
using System.Windows;
using System.Windows.Controls;

namespace OnlineStore.Pages
{
    public partial class AdminPage : Page
    {
        private int editingProductId = 0;
        private List<Product> products = new List<Product>();

        public AdminPage()
        {
            InitializeComponent();

            try
            {
                var db = App.Context;
                if (!HasAccess(db))
                {
                    IsEnabled = false;
                    MessageBox.Show("У вас нет доступа к управлению магазином.");
                    return;
                }

                if (HasAccess(db, true))
                    TabUsers.Visibility = Visibility.Visible;
                else
                    TabUsers.Visibility = Visibility.Collapsed;

                CBProductCategory.ItemsSource = db.Categories.OrderBy(p => p.NameCategory).ToList();
                CBUserRole.ItemsSource = db.Roles.OrderBy(p => p.NameRole).ToList();
                CBOrderStatus.ItemsSource = db.OrderStatuses.OrderBy(p => p.IdOrderStatus).ToList();

                LoadProducts();
                LoadUsers();
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки управления: " + ex.Message);
            }
        }

        private bool HasAccess(OnlineStoreContext db, bool adminOnly = false)
        {
            if (App.CurrentUser == null) return false;

            var user = db.Users.Include(p => p.IdRoleNavigation).FirstOrDefault(p => p.IdUser == App.CurrentUser.IdUser);
            if (user == null) return false;

            string role = user.IdRoleNavigation.NameRole.ToLower();
            if (role.Contains("администратор")) return true;
            if (!adminOnly && role.Contains("менеджер")) return true;
            return false;
        }

        private void LoadProducts()
        {
            var db = App.Context;
            products = db.Products.Include(p => p.IdCategoryNavigation).OrderBy(p => p.NameProduct).ToList();
            UpdateProducts();
        }

        private void TBSearchAdmin_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void UpdateProducts()
        {
            if (LVAdminProducts == null || TBSearchAdmin == null) return;

            var result = products.ToList();
            result = result.Where(p => p.NameProduct.ToLower().Contains(TBSearchAdmin.Text.Trim().ToLower())).ToList();
            LVAdminProducts.ItemsSource = result;
        }

        private void BtNewProduct_Click(object sender, RoutedEventArgs e)
        {
            editingProductId = 0;
            TBProductName.Clear();
            TBProductPrice.Clear();
            TBProductQuantity.Clear();
            CBProductCategory.SelectedIndex = -1;
            TBProductName.Focus();
        }

        private void BtEditProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var product = button.DataContext as Product;
            if (product == null) return;

            editingProductId = product.IdProduct;
            TBProductName.Text = product.NameProduct;
            TBProductPrice.Text = product.PriceProduct.ToString();
            TBProductQuantity.Text = product.QuantityProduct.ToString();
            CBProductCategory.SelectedValuePath = "IdCategory";
            CBProductCategory.SelectedValue = product.IdCategory;
        }

        private void BtSaveProduct_Click(object sender, RoutedEventArgs e)
        {
            string name = TBProductName.Text.Trim();
            var category = CBProductCategory.SelectedItem as Category;
            decimal price;
            int quantity;

            if (name.Length == 0 || name.Length > 100)
            {
                MessageBox.Show("Название товара должно содержать от 1 до 100 символов.");
                return;
            }
            if (category == null)
            {
                MessageBox.Show("Выберите категорию.");
                return;
            }
            if (!decimal.TryParse(TBProductPrice.Text, out price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену (число не меньше нуля).");
                return;
            }
            if (!int.TryParse(TBProductQuantity.Text.Trim(), out quantity) || quantity <= 0)
            {
                MessageBox.Show("Количество должно быть положительным целым числом.");
                return;
            }

            try
            {
                var db = App.Context;
                if (!HasAccess(db))
                {
                    MessageBox.Show("Нет прав на изменение товаров.");
                    return;
                }

                Product product;
                if (editingProductId != 0)
                {
                    var oldProduct = db.Products.FirstOrDefault(p => p.IdProduct == editingProductId);
                    if (oldProduct == null)
                    {
                        MessageBox.Show("Товар уже удалён.");
                        return;
                    }
                    product = oldProduct;
                }
                else
                {
                    product = new Product();
                    db.Products.Add(product);
                }

                product.NameProduct = name;
                product.IdCategory = category.IdCategory;
                product.PriceProduct = price;
                product.QuantityProduct = quantity;
                db.SaveChanges();

                LoadProducts();
                BtNewProduct_Click(this, new RoutedEventArgs());
                MessageBox.Show("Товар сохранён.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить товар: " + ex.Message);
            }
        }

        private void BtDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var product = button.DataContext as Product;
            if (product == null) return;

            if (MessageBox.Show("Удалить товар «" + product.NameProduct + "»?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var db = App.Context;
                if (!HasAccess(db))
                {
                    MessageBox.Show("Недостаточно прав.");
                    return;
                }
                if (db.OrderItems.Any(p => p.IdProduct == product.IdProduct))
                {
                    MessageBox.Show("Этот товар есть в истории заказов. Удалять его нельзя, иначе нарушатся связи в БД.");
                    return;
                }

                var currentProduct = db.Products.FirstOrDefault(p => p.IdProduct == product.IdProduct);
                if (currentProduct == null)
                {
                    MessageBox.Show("Товар уже удалён.");
                    return;
                }

                db.Products.Remove(currentProduct);
                db.SaveChanges();

                if (editingProductId == product.IdProduct)
                    BtNewProduct_Click(this, new RoutedEventArgs());

                LoadProducts();
                MessageBox.Show("Товар удалён.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить товар: " + ex.Message);
            }
        }

        private void LoadUsers()
        {
            if (TabUsers.Visibility != Visibility.Visible) return;

            var db = App.Context;
            LVUsers.ItemsSource = db.Users.Include(p => p.IdRoleNavigation).OrderBy(p => p.Login).ToList();
        }

        private void LVUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var user = LVUsers.SelectedItem as User;
            if (user == null) return;

            CBUserRole.SelectedValuePath = "IdRole";
            CBUserRole.SelectedValue = user.IdRole;
        }

        private void BtSaveRole_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = LVUsers.SelectedItem as User;
            var newRole = CBUserRole.SelectedItem as Role;
            if (selectedUser == null || newRole == null)
            {
                MessageBox.Show("Выберите пользователя и роль.");
                return;
            }

            try
            {
                var db = App.Context;
                if (!HasAccess(db, true))
                {
                    MessageBox.Show("Роли может назначать только администратор.");
                    return;
                }

                var user = db.Users.Include(p => p.IdRoleNavigation).FirstOrDefault(p => p.IdUser == selectedUser.IdUser);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден.");
                    return;
                }

                bool wasAdmin = user.IdRoleNavigation.NameRole.ToLower().Contains("администратор");
                bool willBeAdmin = newRole.NameRole.ToLower().Contains("администратор");

                if (wasAdmin && !willBeAdmin)
                {
                    if (App.CurrentUser != null && user.IdUser == App.CurrentUser.IdUser)
                    {
                        MessageBox.Show("Нельзя изменить собственную роль администратора.");
                        return;
                    }

                    int admins = db.Users.Count(p => p.IdRoleNavigation.NameRole.Contains("администратор"));
                    if (admins <= 1)
                    {
                        MessageBox.Show("Нельзя удалить роль у последнего администратора.");
                        return;
                    }
                }

                user.IdRole = newRole.IdRole;
                db.SaveChanges();
                LoadUsers();
                MessageBox.Show("Роль сохранена.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось изменить роль: " + ex.Message);
            }
        }

        private void LoadOrders()
        {
            var db = App.Context;
            var orders = db.Orders.Include(p => p.IdUserNavigation).Include(p => p.IdOrderStatusNavigation).Include(p => p.OrderItems).OrderByDescending(p => p.DateOrder).ToList();
            var result = new List<OrderView>();

            foreach (var order in orders)
            {
                var item = new OrderView
                {
                    IdOrder = order.IdOrder,
                    Login = order.IdUserNavigation.Login,
                    DateOrder = order.DateOrder,
                    TotalPrice = order.OrderItems.Sum(p => p.PriceOrderItem * p.QuantityOrderItem),
                    Status = order.IdOrderStatusNavigation.NameOrderStatus,
                    IdOrderStatus = order.IdOrderStatus
                };
                result.Add(item);
            }

            LVOrders.ItemsSource = result;
            CBOrderStatus.SelectedIndex = -1;
        }

        private void LVOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var order = LVOrders.SelectedItem as OrderView;
            if (order == null) return;

            CBOrderStatus.SelectedValuePath = "IdOrderStatus";
            CBOrderStatus.SelectedValue = order.IdOrderStatus;
        }

        private void BtOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var order = button.DataContext as OrderView;
            if (order != null)
                OrderDetailsHelper.Show(order.IdOrder);
        }

        private bool IsReturned(string? status)
        {
            if (status == null) return false;
            string name = status.ToLower();
            return name == "отменён" || name == "возврат";
        }

        private bool ChangeStock(OnlineStoreContext db, Order order, bool addStock)
        {.
            foreach (var item in order.OrderItems)
            {
                var product = db.Products.FirstOrDefault(p => p.IdProduct == item.IdProduct);
                if (product == null) return false;
                if (!addStock && product.QuantityProduct < item.QuantityOrderItem) return false;
            }

            foreach (var item in order.OrderItems)
            {
                var product = db.Products.First(p => p.IdProduct == item.IdProduct);
                if (addStock)
                    product.QuantityProduct += item.QuantityOrderItem;
                else
                    product.QuantityProduct -= item.QuantityOrderItem;
            }
            return true;
        }

        private void BtSaveOrderStatus_Click(object sender, RoutedEventArgs e)
        {
            var selected = LVOrders.SelectedItem as OrderView;
            var newStatus = CBOrderStatus.SelectedItem as OrderStatus;
            if (selected == null || newStatus == null)
            {
                MessageBox.Show("Выберите заказ и новый статус.");
                return;
            }

            try
            {
                var db = App.Context;
                if (!HasAccess(db))
                {
                    MessageBox.Show("Недостаточно прав.");
                    return;
                }

                var order = db.Orders.Include(p => p.OrderItems).Include(p => p.IdOrderStatusNavigation).FirstOrDefault(p => p.IdOrder == selected.IdOrder);
                if (order == null)
                {
                    MessageBox.Show("Заказ не найден.");
                    return;
                }
                if (order.IdOrderStatus == newStatus.IdOrderStatus) return;

                bool oldReturned = IsReturned(order.IdOrderStatusNavigation.NameOrderStatus);
                bool newReturned = IsReturned(newStatus.NameOrderStatus);

                if (oldReturned != newReturned && !ChangeStock(db, order, newReturned))
                {
                    MessageBox.Show("Не удалось изменить остатки на складе. Проверьте количество товаров.");
                    return;
                }

                order.IdOrderStatus = newStatus.IdOrderStatus;
                db.SaveChanges();
                LoadOrders();
                MessageBox.Show("Статус заказа изменён.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось изменить статус: " + ex.Message);
            }
        }

        private void BtDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            var selected = button.DataContext as OrderView;
            if (selected == null) return;

            if (MessageBox.Show($"Удалить заказ №{selected.IdOrder} и его состав?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                var db = App.Context;
                if (!HasAccess(db))
                {
                    MessageBox.Show("Недостаточно прав.");
                    return;
                }

                var order = db.Orders.Include(p => p.OrderItems).Include(p => p.IdOrderStatusNavigation).FirstOrDefault(p => p.IdOrder == selected.IdOrder);
                if (order == null)
                {
                    MessageBox.Show("Заказ уже удалён.");
                    return;
                }

                if (!IsReturned(order.IdOrderStatusNavigation.NameOrderStatus) && !ChangeStock(db, order, true))
                {
                    MessageBox.Show("Не удалось вернуть товар на склад.");
                    return;
                }

                db.OrderItems.RemoveRange(order.OrderItems);
                db.Orders.Remove(order);
                db.SaveChanges();
                LoadOrders();
                MessageBox.Show("Заказ удалён.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить заказ: " + ex.Message);
            }
        }
    }
}
