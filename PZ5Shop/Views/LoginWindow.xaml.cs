using System.Windows;
using PZ5Shop.ViewModels;

namespace PZ5Shop.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void OnLogin(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль", "Вход", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AppState.Current.Login(username, password))
            {
                DialogResult = true;
                Close();
                return;
            }

            MessageBox.Show("Неверный логин или пароль", "Вход", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
