using ChartSightAI.MVVM.ViewModels;

namespace ChartSightAI.MVVM.Views;

public partial class NewPrediction : ContentPage
{
    public NewPrediction(NewPredictionVM vm)
    {
        InitializeComponent();
        BindingContext = vm;
        this.Loaded += OnPageLoaded;
    }
    protected override void OnAppearing()
    {
        if (BindingContext is NewPredictionVM vm)
        {
            _ = vm.InitializeAsync();
        }
        base.OnAppearing();
    }
    private void OnPageLoaded(object sender, EventArgs e)
    {
        Header.TranslationX = -50;
        Header.Opacity = 0;

        Body.TranslationY = -200;
        Body.Opacity = 0;
        var headerAnimation = new Animation();
        var bodyAnimation = new Animation();

        headerAnimation.Add(0, 1, new Animation(v => Header.TranslationX = v, -50, 0, Easing.CubicOut));
        headerAnimation.Add(0, 1, new Animation(v => Header.Opacity = v, 0, 1, Easing.CubicOut));

        bodyAnimation.Add(0, 1, new Animation(v => Body.TranslationY = v, -100, 0, Easing.CubicOut));
        bodyAnimation.Add(0, 1, new Animation(v => Body.Opacity = v, 0, 1, Easing.CubicOut));
        headerAnimation.Commit(this, "HeaderEntranceAnimation", length: 2000, easing: Easing.CubicOut);
        bodyAnimation.Commit(this, "BodyAnimation", length: 2000, easing: Easing.CubicOut);
    }
}