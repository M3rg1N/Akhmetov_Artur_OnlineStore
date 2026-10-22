using Microsoft.EntityFrameworkCore;
using OnlineStore.Helpers;
using OnlineStore.Models;
using System.Net.Mail;
using System.Windows;

namespace OnlineStore
{
    public partial class MainWindow : Window
    {
        private bool isRegistration = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtSwitchRegAuth_Click(object sender, RoutedEventArgs e)
        {
            isRegistration = !isRegistration;

            if (isRegistration)
            {
                LoginGrid.Visibility = Visibility.Collapsed;
                RegisterGrid.Visibility = Visibility.Visible;
                BtRegAuth.Content = "Зарегистрироваться";
                BtSwitchRegAuth.Content = "Есть аккаунт? Войти";
            }
            else
            {
                LoginGrid.Visibility = Visibility.Visible;
                RegisterGrid.Visibility = Visibility.Collapsed;
                BtRegAuth.Content = "Войти";
                BtSwitchRegAuth.Content = "Нет аккаунта? Регистрация";
            }
        }

        private void CBShowPassword_Changed(object sender, RoutedEventArgs e)
        {
            if (CBShowPassword.IsChecked == true)
            {
                TBPasswordVisibleAuth.Text = TBPasswordAuth.Password;
                TBPasswordAuth.Visibility = Visibility.Collapsed;
                TBPasswordVisibleAuth.Visibility = Visibility.Visible;
            }
            else
            {
                TBPasswordAuth.Password = TBPasswordVisibleAuth.Text;
                TBPasswordAuth.Visibility = Visibility.Visible;
                TBPasswordVisibleAuth.Visibility = Visibility.Collapsed;
            }
        }

        private void BtRegAuth_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isRegistration)
                    Register();
                else
                    Login();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось обратиться к базе данных: " + ex.Message);
            }
        }

        private void Register()
        {
            string login = TBLoginReg.Text.Trim();
            string password = PBFirstPassword.Password;
            string repeat = PBSecondPassword.Password;
            string phone = TBPhoneReg.Text.Trim();
            string email = TBEmailReg.Text.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(repeat) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Заполните все поля регистрации!");
                return;
            }

            if (login.Length > 50 || phone.Length > 50 || email.Length > 50)
            {
                MessageBox.Show("Слишком длинный логин, телефон или email (не более 50 символов).");
                return;
            }

            if (password.Length < 8)
            {
                MessageBox.Show("Пароль должен содержать не менее 8 символов.");
                return;
            }

            if (password != repeat)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            string digits = new string(phone.Where(p => char.IsDigit(p)).ToArray());
            if (digits.Length < 10 || digits.Length > 11)
            {
                MessageBox.Show("Проверьте номер телефона.");
                return;
            }

            try
            {
                var address = new MailAddress(email);
                if (address.Address.ToLower() != email.ToLower())
                {
                    MessageBox.Show("Некорректный email.");
                    return;
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Некорректный email.");
                return;
            }

            var db = App.Context;
            var existingUser = db.Users.FirstOrDefault(p => p.Login == login);
            if (existingUser != null)
            {
                MessageBox.Show("Этот логин уже занят!");
                return;
            }

            var buyerRole = db.Roles.FirstOrDefault(p => p.NameRole.ToLower() == "покупатель");
            if (buyerRole == null)
                buyerRole = db.Roles.FirstOrDefault(p => p.NameRole.ToLower() == "клиент");

            if (buyerRole == null)
            {
                MessageBox.Show("В таблице Roles отсутствует роль «Покупатель». Добавьте её в базу данных.");
                return;
            }

            var user = new User
            {
                Login = login,
                HashPassword = PasswordHelper.Hash(password),
                Phone = phone,
                Email = email,
                IdRole = buyerRole.IdRole,
                IsPhoneVerified = false,
                IsEmailVerified = false,
                IsAddressConfirmed = false
            };

            db.Users.Add(user);
            db.SaveChanges();

            MessageBox.Show("Регистрация успешна! Теперь войдите в аккаунт.");
            TBLoginAuth.Text = login;
            PBFirstPassword.Clear();
            PBSecondPassword.Clear();
            BtSwitchRegAuth_Click(this, new RoutedEventArgs());
        }

        private void Login()
        {
            string login = TBLoginAuth.Text.Trim();
            string password = TBPasswordAuth.Password;
            if (CBShowPassword.IsChecked == true)
                password = TBPasswordVisibleAuth.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }

            var db = App.Context;
            var user = db.Users.Include(p => p.IdRoleNavigation).FirstOrDefault(p => p.Login == login);

            if (user == null || !PasswordHelper.Verify(password, user.HashPassword))
            {
                MessageBox.Show("Неверный логин или пароль.");
                return;
            }

            App.CurrentUser = user;
            OpenStore();
        }

        private void BtGuest_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            OpenStore();
        }

        private void OpenStore()
        {
            App.CartItems.Clear();
            var storeWindow = new StoreWindow();
            Application.Current.MainWindow = storeWindow;
            storeWindow.Show();
            Close();
        }
    }
}
