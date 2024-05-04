using Microsoft.Maui.Storage;
using SignPDF.ViewModels;
namespace SignPDF.Views;

[QueryProperty(nameof(FileName), "FileName")]
public partial class ViewPage : ContentPage
{
    private string _fileName;
    public string FileName {
        get {
            return _fileName;
        }
        set {
            _fileName = value;
            BindingContext = new PdfViewModel(_fileName);
        }
    }
    public ViewPage()
	{
		InitializeComponent();
	}
}