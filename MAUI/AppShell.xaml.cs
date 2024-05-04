using SignPDF.Views;

namespace SignPDF;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(SignPage), typeof(SignPage));
    }
}
