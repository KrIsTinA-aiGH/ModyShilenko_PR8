using Shilenko_wpf1.Models;
using Shilenko_wpf1.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Shilenko_wpf1.Pages
{
    public partial class Autho : Page
    {
        int attempts = 0;

        public Autho()
        {
            InitializeComponent();
            ResetForm();
        }

        private void btnEnterGuest_Click(object sender, RoutedEventArgs e) =>
            NavigationService.Navigate(new Client(null, "Гость"));

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            attempts++;
            var login = tbLogin.Text.Trim();
            var password = tbPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                attempts--;
                return;
            }

            try
            {
                using (var db = new AutobaseEntities())
                {
                    var user = db.Users.FirstOrDefault(x => x.Email == login && x.Password == password);

                    if (attempts == 1)
                    {
                        if (user != null) LoginSuccess(user);
                        else ShowErrorAndCaptcha();
                    }
                    else if (attempts > 1)
                    {
                        if (user != null && (tbCaptcha.Visibility != Visibility.Visible || tbCaptcha.Text == tblCaptcha.Text))
                            LoginSuccess(user);
                        else
                        {
                            MessageBox.Show("Неверные данные!" + (tbCaptcha.Visibility == Visibility.Visible ? " или капча" : ""));
                            ResetForm();
                        }
                    }
                }
            }
            catch
            {
                NavigationService.Navigate(new Client(null, "Гость"));
            }
        }

        private void LoginSuccess(Users user)
        {
            var role = GetRole(user);
            MessageBox.Show($"Вы вошли как: {role}");
            NavigationService.Navigate(new Client(user, role));
        }

        private void ShowErrorAndCaptcha()
        {
            MessageBox.Show("Неверный логин или пароль!");
            ShowCaptcha();
            tbPassword.Clear();
        }

        private void ShowCaptcha()
        {
            tbCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Text = SimpleCaptcha.Create();
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;
            tbCaptcha.Clear();
        }

        private void HideCaptcha()
        {
            tbCaptcha.Visibility = Visibility.Hidden;
            tblCaptcha.Visibility = Visibility.Hidden;
        }

        private string GetRole(Users user)
        {
            var email = user.Email.ToLower();
            if (email.Contains("admin")) return "Администратор";
            if (email.Contains("manager")) return "Менеджер";
            if (email.Contains("driver")) return "Водитель";
            if (email.Contains("dispatcher")) return "Диспетчер";
            if (email.Contains("mechanic")) return "Механик";
            if (email.Contains("client")) return "Клиент";
            return "Пользователь";
        }

        private void ResetForm()
        {
            tbLogin.Clear();
            tbPassword.Clear();
            tbCaptcha.Clear();
            HideCaptcha();
            attempts = 0;
        }
    }
}