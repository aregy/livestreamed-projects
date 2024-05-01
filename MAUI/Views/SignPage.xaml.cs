using CommunityToolkit.Maui.Views;
using DevExpress.Drawing;
using DevExpress.Maui.Core.Internal;
using DevExpress.Office.DigitalSignatures;
using DevExpress.Pdf;
using Microsoft.Maui.Graphics.Platform;
using SignPDF.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SignPDF.Views;

public partial class SignPage : ContentPage
{
    public SignPage(PdfViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void imageEdit1_ImageLoaded(object sender, EventArgs e)
    {

    }
}