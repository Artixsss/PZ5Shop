using System.Windows;
using PZ5Shop.ViewModels;

namespace PZ5Shop.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void OnRegister(object sender, RoutedEventArgs e)
        {
            var lastName = LastNameBox.Text.Trim();
            var firstName = FirstNameBox.Text.Trim();
            var middleName = MiddleNameBox.Text.Trim();
            var username = UsernameBox.Text.Trim();
            var email = EmailBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все обязательные поля", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = AppState.Current.Register(lastName, firstName, middleName, username, email, password);
            if (user == null)
            {
                MessageBox.Show("Логин или email уже используются", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
