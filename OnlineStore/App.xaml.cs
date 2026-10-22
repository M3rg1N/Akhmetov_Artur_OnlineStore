using OnlineStore.Models;
using System.Windows;

namespace OnlineStore
{
    public partial class App : Application
    {
        public static OnlineStoreContext Context { get; } = new OnlineStoreContext();
        public static User? CurrentUser { get; set; }
        public static List<CartItem> CartItems { get; } = new List<CartItem>();

        public static bool IsAdmin
        {
            get
            {
                if (CurrentUser == null || CurrentUser.IdRoleNavigation == null) return false;
                return CurrentUser.IdRoleNavigation.NameRole.ToLower().Contains("администратор");
            }
        }

        public static bool IsManager
        {
            get
            {
                if (CurrentUser == null || CurrentUser.IdRoleNavigation == null) return false;
                return CurrentUser.IdRoleNavigation.NameRole.ToLower().Contains("менеджер");
            }
        }

        public static bool CanManage { get { return IsAdmin || IsManager; } }

        public static void Logout(Window currentWindow)
        {
            CurrentUser = null;
            CartItems.Clear();
            var loginWindow = new MainWindow();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            currentWindow.Close();
        }
    }
}
