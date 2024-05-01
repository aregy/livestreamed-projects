using SignPDF.ViewModels;

namespace SignPDF.Views;

public partial class ViewPage : ContentPage
{
	public ViewPage(PdfViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}