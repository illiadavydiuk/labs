using System.Windows;
using Practice.Data.Entities;
using Practice.Services.Interfaces;

namespace Practice.Windows
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly User _user;
        private readonly dynamic _identityService; 

        public ChangePasswordWindow(User user, dynamic identityService)
        {
            InitializeComponent();
            _user = user;
            _identityService = identityService;
        }

        private async void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            if (TxtNewPass.Password.Length < 4)
            {
                MessageBox.Show("Пароль занадто короткий"); return;
            }
            if (TxtNewPass.Password != TxtConfirmPass.Password)
            {
                MessageBox.Show("Паролі не співпадають"); return;
            }

            using (var ctx = new Practice.Data.Context.AppDbContext())
            {
                var dbUser = await ctx.Users.FindAsync(_user.UserId);
                dbUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TxtNewPass.Password);
                dbUser.IsPasswordChangeRequired = false;
                await ctx.SaveChangesAsync();
            }

            DialogResult = true;
            Close();
        }
    }
}