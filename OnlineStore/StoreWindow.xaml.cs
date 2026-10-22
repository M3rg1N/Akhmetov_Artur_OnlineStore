using OnlineStore.Pages;
using System.Windows;

namespace OnlineStore
{
    public partial class StoreWindow : Window
    {
        public StoreWindow()
        {
            InitializeComponent();

            if (App.CurrentUser == null)
                BtCart.Visibility = Visibility.Collapsed;
            else
                BtCart.Visibility = Visibility.Visible;

            if (App.CanManage)
                BtAdmin.Visibility = Visibility.Visible;
            else
                BtAdmin.Visibility = Visibility.Collapsed;

            FrStore.Navigate(new CatalogPage());
        }

        private void BtCatalog_Click(object sender, RoutedEventArgs e)
        {
            FrStore.Navigate(new CatalogPage());
        }

        private void BtCart_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
                FrStore.Navigate(new CartPage());
        }

        private void BtProfile_Click(object sender, RoutedEventArgs e)
        {
            FrStore.Navigate(new ProfilePage());
        }

        private void BtAdmin_Click(object sender, RoutedEventArgs e)
        {
            if (App.CanManage)
                FrStore.Navigate(new AdminPage());
        }
    }
}
