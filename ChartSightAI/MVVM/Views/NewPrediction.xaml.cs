using ChartSightAI.MVVM.ViewModels;

namespace ChartSightAI.MVVM.Views;

public partial class NewPrediction : ContentPage
{
	public NewPrediction(NewPredictionVM vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}