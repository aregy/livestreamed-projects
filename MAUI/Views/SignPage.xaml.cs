using CommunityToolkit.Maui.Views;
using DevExpress.Drawing;
using DevExpress.Maui.Core.Internal;
using DevExpress.Office.DigitalSignatures;
using DevExpress.Pdf;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Storage;
using SignPDF.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
namespace SignPDF.Views;

[QueryProperty(nameof(FileName), "FileName")]
public partial class SignPage : ContentPage
{
    private string _fileName;
    public string FileName {
        get {
            return _fileName;
        }
        set {
            _fileName = value;
            BindingContext = new PdfViewModel(_fileName);            
            (this.BindingContext as PdfViewModel)._ImageEdit = this.imageEdit1;
            (this.BindingContext as PdfViewModel)._SignatureDrawer = this.signatureDrawer;
        }
    }
    public SignPage()
    {
        InitializeComponent();
    }

    private void imageEdit1_ImageLoaded(object sender, EventArgs e)
    {

    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args) {
        base.OnNavigatedTo(args);
    }
}