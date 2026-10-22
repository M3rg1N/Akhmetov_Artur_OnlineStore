using System.Windows;

namespace OnlineStore.Helpers
{
    public partial class VerificationWindow : Window
    {
        private string correctCode;

        public VerificationWindow(string code, string name)
        {
            InitializeComponent();
            correctCode = code;
            Title = "Подтверждение " + name;
            TBCode.Focus();
        }

        private void BtConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (TBCode.Text.Trim() != correctCode)
            {
                MessageBox.Show("Неверный код.");
                return;
            }
            DialogResult = true;
        }
    }
}
