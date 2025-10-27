using ChartSightAI.MVVM.ViewModels;

namespace ChartSightAI.MVVM.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeVM vm)
    {
        InitializeComponent();
        this.Loaded += OnPageLoaded;
        BindingContext = vm;
    }
    private void OnPageLoaded(object sender, EventArgs e)
    {
        HeaderLayout.TranslationX = -50;
        HeaderLayout.Opacity = 0;

        Body.TranslationY = -100;
        Body.Opacity = 0;
        Settings.Opacity = 0;

        var headerAnimation = new Animation();
        var bodyAnimation = new Animation();
        var settingsAnimation = new Animation();

        headerAnimation.Add(0, 1, new Animation(v => HeaderLayout.TranslationX = v, -50, 0, Easing.CubicOut));
        headerAnimation.Add(0, 1, new Animation(v => HeaderLayout.Opacity = v, 0, 1, Easing.CubicOut));

        bodyAnimation.Add(0, 1, new Animation(v => Body.TranslationY = v, -100, 0, Easing.CubicOut));
        bodyAnimation.Add(0, 1, new Animation(v => Body.Opacity = v, 0, 1, Easing.CubicOut));

        settingsAnimation.Add(0, 1, new Animation(v => Settings.Opacity = v, 0, 1, Easing.CubicOut));

        headerAnimation.Commit(this, "HeaderEntranceAnimation", length: 2000, easing: Easing.CubicOut);
        bodyAnimation.Commit(this, "BodyAnimation", length: 2000, easing: Easing.CubicOut);
        settingsAnimation.Commit(this, "SettingAnimation", length: 1000, easing: Easing.CubicInOut);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(NewPrediction)}");
    }
}