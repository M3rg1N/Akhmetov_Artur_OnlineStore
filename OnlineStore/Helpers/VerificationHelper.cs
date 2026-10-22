using System.Windows;

namespace OnlineStore.Helpers
{
    public static class VerificationHelper
    {
        public static bool ConfirmCode(string name)
        {
            string code = Random.Shared.Next(100000, 1000000).ToString();
            MessageBox.Show("Код подтверждения " + name + ": " + code);

            var window = new VerificationWindow(code, name);
            return window.ShowDialog() == true;
        }
    }
}
