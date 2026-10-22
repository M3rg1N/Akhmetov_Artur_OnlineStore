using Microsoft.EntityFrameworkCore;
using OnlineStore.Helpers;
using OnlineStore.Models;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OnlineStore.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadProfile();
        }

        private void LoadProfile()
        {
            if (App.CurrentUser == null)
            {
                TBlGuestMessage.Visibility = Visibility.Visible;
                BtLogout.Content = "К авторизации";
                BtEditProfile.IsEnabled = false;
                BtSaveProfile.IsEnabled = false;
                BtVerifyPhone.IsEnabled = false;
                BtVerifyEmail.IsEnabled = false;
                BtVerifyAddress.IsEnabled = false;
                LVHistory.IsEnabled = false;
                return;
            }

            try
            {
                var db = App.Context;
                var user = db.Users.First(p => p.IdUser == App.CurrentUser.IdUser);

                TBLoginProfile.Text = user.Login;
                TBLastName.Text = user.LastName ?? "";
                TBFirstName.Text = user.FirstName ?? "";
                TBMiddleName.Text = user.MiddleName ?? "";
                TBPhoneProfile.Text = user.Phone ?? "";
                TBEmailProfile.Text = user.Email ?? "";
                TBAddress.Text = user.Address ?? "";

                if (user.IsPhoneVerified == true)
                    TBlPhoneStatus.Text = "Подтверждён";
                else
                    TBlPhoneStatus.Text = "Не подтверждён";

                if (user.IsEmailVerified == true)
                    TBlEmailStatus.Text = "Подтверждён";
                else
                    TBlEmailStatus.Text = "Не подтверждён";

                if (user.IsAddressConfirmed == true)
                    TBlAddressStatus.Text = "Подтверждён";
                else
                    TBlAddressStatus.Text = "Не подтверждён";

                BtVerifyPhone.IsEnabled = user.IsPhoneVerified != true;
                BtVerifyEmail.IsEnabled = user.IsEmailVerified != true;
                BtVerifyAddress.IsEnabled = user.IsAddressConfirmed != true;

                var orders = db.Orders.Include(p => p.OrderItems).Include(p => p.IdOrderStatusNavigation).Where(p => p.IdUser == user.IdUser).OrderByDescending(p => p.DateOrder).ToList();
                var history = new List<OrderView>();

                foreach (var order in orders)
                {
                    var item = new OrderView
                    {
                        IdOrder = order.IdOrder,
                        DateOrder = order.DateOrder,
                        TotalPrice = order.OrderItems.Sum(p => p.PriceOrderItem * p.QuantityOrderItem),
                        Status = order.IdOrderStatusNavigation.NameOrderStatus
                    };
                    history.Add(item);
                }

                LVHistory.ItemsSource = history;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить профиль: " + ex.Message);
            }
        }

        private void SetEditing(bool editing)
        {
            TextBox[] fields = { TBLastName, TBFirstName, TBMiddleName, TBPhoneProfile, TBEmailProfile, TBAddress };
            foreach (var field in fields)
                field.IsReadOnly = !editing;

            BtSaveProfile.IsEnabled = editing;
            BtEditProfile.IsEnabled = !editing;
        }

        private void BtEditProfile_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
                SetEditing(true);
        }

        private void BtSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;

            string phone = TBPhoneProfile.Text.Trim();
            string email = TBEmailProfile.Text.Trim();
            string address = TBAddress.Text.Trim();

            if (TBLastName.Text.Trim().Length > 50 || TBFirstName.Text.Trim().Length > 50 || TBMiddleName.Text.Trim().Length > 50 || phone.Length > 50 || email.Length > 50 || address.Length > 255)
            {
                MessageBox.Show("Превышена допустимая длина поля (ФИО, телефон, почта – 50; адрес – 255).");
                return;
            }

            string digits = new string(phone.Where(p => char.IsDigit(p)).ToArray());
            if (phone.Length > 0 && (digits.Length < 10 || digits.Length > 11))
            {
                MessageBox.Show("Проверьте номер телефона.");
                return;
            }

            if (email.Length > 0)
            {
                try
                {
                    var mail = new MailAddress(email);
                    if (mail.Address.ToLower() != email.ToLower())
                    {
                        MessageBox.Show("Неверный формат email.");
                        return;
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("Неверный формат email.");
                    return;
                }
            }

            try
            {
                var db = App.Context;
                var user = db.Users.First(p => p.IdUser == App.CurrentUser.IdUser);

                user.LastName = TBLastName.Text.Trim();
                user.FirstName = TBFirstName.Text.Trim();
                user.MiddleName = TBMiddleName.Text.Trim();

                if (user.Phone != phone)
                {
                    user.Phone = phone;
                    user.IsPhoneVerified = false;
                }
                if (user.Email != email)
                {
                    user.Email = email;
                    user.IsEmailVerified = false;
                }
                if (user.Address != address)
                {
                    user.Address = address;
                    user.IsAddressConfirmed = false;
                }

                db.SaveChanges();
                SetEditing(false);
                LoadProfile();
                MessageBox.Show("Данные сохранены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить профиль: " + ex.Message);
            }
        }

        private void BtVerifyPhone_Click(object sender, RoutedEventArgs e)
        {
            VerifyContact(true);
        }

        private void BtVerifyEmail_Click(object sender, RoutedEventArgs e)
        {
            VerifyContact(false);
        }

        private void VerifyContact(bool phone)
        {
            if (App.CurrentUser == null) return;
            if (BtSaveProfile.IsEnabled)
            {
                MessageBox.Show("Сначала сохраните изменения в профиле.");
                return;
            }

            try
            {
                var db = App.Context;
                var user = db.Users.First(p => p.IdUser == App.CurrentUser.IdUser);

                string value;
                string name;
                bool verified;

                if (phone)
                {
                    value = user.Phone ?? "";
                    name = "телефона";
                    verified = user.IsPhoneVerified == true;
                }
                else
                {
                    value = user.Email ?? "";
                    name = "email";
                    verified = user.IsEmailVerified == true;
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    MessageBox.Show("Сначала укажите контактные данные.");
                    return;
                }
                if (verified) return;
                if (!VerificationHelper.ConfirmCode(name)) return;

                if (phone)
                    user.IsPhoneVerified = true;
                else
                    user.IsEmailVerified = true;

                db.SaveChanges();
                LoadProfile();
                MessageBox.Show("Подтверждение успешно.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подтверждения: " + ex.Message);
            }
        }

        private void BtVerifyAddress_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;
            if (BtSaveProfile.IsEnabled)
            {
                MessageBox.Show("Сначала сохраните изменения в профиле.");
                return;
            }

            try
            {
                var db = App.Context;
                var user = db.Users.First(p => p.IdUser == App.CurrentUser.IdUser);
                if (string.IsNullOrWhiteSpace(user.Address))
                {
                    MessageBox.Show("Сначала укажите адрес доставки.");
                    return;
                }

                if (MessageBox.Show("Адрес доставки указан верно?\n" + user.Address, "Подтверждение адреса", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                user.IsAddressConfirmed = true;
                db.SaveChanges();
                LoadProfile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подтверждения адреса: " + ex.Message);
            }
        }

        private void LVHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var order = LVHistory.SelectedItem as OrderView;
            if (order != null)
                OrderDetailsHelper.Show(order.IdOrder);
        }

        private void BtLogout_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
                App.Logout(window);
        }
    }
}
