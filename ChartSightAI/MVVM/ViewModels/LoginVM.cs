using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChartSightAI.MVVM.Views;
using ChartSightAI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class LoginVM : ObservableObject
    {
        private readonly AuthService _auth;

        [ObservableProperty] private string email;
        [ObservableProperty] private string password;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private bool isPasswordHidden = true;

        [ObservableProperty] private string emailError;
        [ObservableProperty] private bool emailErrorVisible;
        [ObservableProperty] private string passwordError;
        [ObservableProperty] private bool passwordErrorVisible;
        [ObservableProperty] private bool _isLoginEnabled;

        public string PasswordEyeIcon => IsPasswordHidden ? "eye.png" : "eye_off.png";

        public LoginVM(AuthService auth)
        {
            _auth = auth;
            UpdateCanLogin();
        }

        partial void OnEmailChanged(string value) { ValidateEmail(); UpdateCanLogin(); }
        partial void OnPasswordChanged(string value) { ValidatePassword(); UpdateCanLogin(); }
        partial void OnIsBusyChanged(bool value) { UpdateCanLogin(); }
        partial void OnIsPasswordHiddenChanged(bool value) => OnPropertyChanged(nameof(PasswordEyeIcon));

        [RelayCommand] private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;

        [RelayCommand]
        private async Task Login()
        {
            if (!ValidateAll()) { UpdateCanLogin(); return; }

            try
            {
                IsBusy = true;
                await _auth.InitializeAsync();
                var session = await _auth.Login(Email?.Trim(), Password);

                if (session == null)
                {
                    await Shell.Current.DisplayAlert("Login failed", "No account found or wrong email/password.", "OK");
                    return;
                }

                await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (msg.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("credentials", StringComparison.OrdinalIgnoreCase))
                    msg = "No account found or wrong email/password.";

                await Shell.Current.DisplayAlert("Login failed", msg, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand] private Task GoToSignUp() => Shell.Current.GoToAsync($"//{nameof(SignUpPage)}");

        private void UpdateCanLogin() =>
            IsLoginEnabled = !IsBusy && IsValidEmail(Email) && IsValidPassword(Password);

        private bool ValidateAll() => ValidateEmail() & ValidatePassword();

        private bool ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email is required.";
                EmailErrorVisible = true;
                return false;
            }
            if (!IsValidEmail(Email))
            {
                EmailError = "Enter a valid email (name@example.com).";
                EmailErrorVisible = true;
                return false;
            }
            EmailErrorVisible = false; EmailError = string.Empty; return true;
        }

        private bool ValidatePassword()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Password is required.";
                PasswordErrorVisible = true;
                return false;
            }
            if (!IsValidPassword(Password))
            {
                PasswordError = "At least 6 characters.";
                PasswordErrorVisible = true;
                return false;
            }
            PasswordErrorVisible = false; PasswordError = string.Empty; return true;
        }

        private static bool IsValidEmail(string s) =>
            !string.IsNullOrWhiteSpace(s) &&
            Regex.IsMatch(s.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

        private static bool IsValidPassword(string s) => !string.IsNullOrEmpty(s) && s.Length >= 6;
    }
}
