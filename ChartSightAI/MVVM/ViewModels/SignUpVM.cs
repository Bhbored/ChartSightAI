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
    public partial class SignUpVM : ObservableObject
    {
        private readonly AuthService _auth;

        [ObservableProperty] private string _email;
        [ObservableProperty] private string _password;
        [ObservableProperty] private string _confirmPassword;
        [ObservableProperty] private bool _isBusy;

        [ObservableProperty] private bool _isPasswordHidden = true;
        [ObservableProperty] private bool _isConfirmHidden = true;

        [ObservableProperty] private string _emailError;
        [ObservableProperty] private bool emailErrorVisible;
        [ObservableProperty] private string passwordError;
        [ObservableProperty] private bool passwordErrorVisible;
        [ObservableProperty] private string confirmError;
        [ObservableProperty] private bool confirmErrorVisible;

        [ObservableProperty] private bool isSignUpEnabled;

        public string PasswordEyeIcon => IsPasswordHidden ? "eye.png" : "eye_off.png";
        public string ConfirmEyeIcon => IsConfirmHidden ? "eye.png" : "eye_off.png";

        public SignUpVM(AuthService auth)
        {
            _auth = auth;
            UpdateCanSignUp();
        }

        partial void OnEmailChanged(string value) { ValidateEmail(); UpdateCanSignUp(); }
        partial void OnPasswordChanged(string value) { ValidatePassword(); ValidateConfirm(); UpdateCanSignUp(); }
        partial void OnConfirmPasswordChanged(string value) { ValidateConfirm(); UpdateCanSignUp(); }
        partial void OnIsBusyChanged(bool value) { UpdateCanSignUp(); }
        partial void OnIsPasswordHiddenChanged(bool value) => OnPropertyChanged(nameof(PasswordEyeIcon));
        partial void OnIsConfirmHiddenChanged(bool value) => OnPropertyChanged(nameof(ConfirmEyeIcon));

        [RelayCommand] private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;
        [RelayCommand] private void ToggleConfirmVisibility() => IsConfirmHidden = !IsConfirmHidden;

        [RelayCommand]
        private async Task SignUp()
        {
            if (!ValidateAll()) { UpdateCanSignUp(); return; }

            try
            {
                IsBusy = true;
                await _auth.InitializeAsync();
                var session = await _auth.SignUp(Email?.Trim(), Password);

                if (session == null)
                {
                    await Shell.Current.DisplayAlert("Sign up failed", "Could not create the account. It may already exist.", "OK");
                    return;
                }

                await Shell.Current.DisplayAlert("Welcome 🎉", "Account created successfully.", "Continue");
                await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (msg.Contains("already", StringComparison.OrdinalIgnoreCase))
                    msg = "An account with this email already exists.";
                await Shell.Current.DisplayAlert("Sign up failed", msg, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand] private Task GoToLogin() => Shell.Current.GoToAsync($"//{nameof(LoginPage)}");

        private void UpdateCanSignUp() =>
            IsSignUpEnabled = !IsBusy && IsValidEmail(Email) && IsValidPassword(Password) && PasswordsMatch();

        private bool ValidateAll() => ValidateEmail() & ValidatePassword() & ValidateConfirm();

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

        private bool ValidateConfirm()
        {
            if (!PasswordsMatch())
            {
                ConfirmError = "Passwords do not match.";
                ConfirmErrorVisible = true;
                return false;
            }
            ConfirmErrorVisible = false; ConfirmError = string.Empty; return true;
        }

        private bool PasswordsMatch() => !string.IsNullOrEmpty(Password) && Password == ConfirmPassword;

        private static bool IsValidEmail(string s) =>
            !string.IsNullOrWhiteSpace(s) &&
            Regex.IsMatch(s.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

        private static bool IsValidPassword(string s) => !string.IsNullOrEmpty(s) && s.Length >= 6;
    }
}
