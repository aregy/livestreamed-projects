using DevExpress.Drawing;
using DevExpress.Pdf;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp;
using System.Windows.Input;
using DevExpress.Maui.Core;
using DevExpress.Office.DigitalSignatures;
//using GameKit;
using DevExpress.Maui.Editors;
using DevExpress.XtraCharts.Native;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;
using SignPDF.Data;
using Microsoft.Maui.Storage;
using System.Net.NetworkInformation;
namespace SignPDF.ViewModels;

public class PdfViewModel : BindableBase {
    private string _fileName;
    private DocumentViewModel _documentViewModel;
    private ImageEdit imageEdit;
    public ImageSource pdfPreview;
    const string defaultDocumentName = "JewelCityLetter.pdf";
    const string defaultCertificateName = "pfxCertificate.pfx";
    const string defaultCertificatePassword = "123";
    string certificateFullPath;
    string documentFullPath;
    bool isSignatureViewOpened;
    public ImageSource PdfPreview {
        get {
            return pdfPreview;
        }
        set {
            pdfPreview = value;
            RaisePropertyChanged();
        }
    }
    public int PageNumber { get; set; } = 1;
    public bool IsSignatureViewOpened {
        get {
            return isSignatureViewOpened;
        }
        set {
            isSignatureViewOpened = value;
            RaisePropertyChanged();
        }
    }
    public ICommand SignPdfCommand {
        get; set;
    }
    public ICommand OpenFileCommand {
        get;
        set;
    }
    public ICommand OpenSignatureViewCommand {
        get;
        set;
    }
    public ICommand CloseSignatureViewCommand {
        get;
        set;
    }
    public ICommand ImageLoadedCommand {
        get;
        set;
    }
    HttpClient _httpClient;
    public PdfViewModel(string fileName) {
        _fileName = fileName;
        //_documentViewModel = documentViewModel;
        LoadDocument();
        UpdatePreview();
        //SignPdfCommand = new Command<byte[]>(SignPdf);
        //OpenFileCommand = new Command(OpenFile);
        //OpenSignatureViewCommand = new Command(OpenSignatureView);
        //CloseSignatureViewCommand = new Command(CloseSignatureView);
        //ImageLoadedCommand = new Command(OnImageLoaded);
    }
    private void OnImageLoaded(object args) {
        if (args is ImageEdit)
            imageEdit = args as ImageEdit;
    }
    private void OpenSignatureView() {
        IsSignatureViewOpened = true;
    }
    private void CloseSignatureView() {
        IsSignatureViewOpened = false;
    }
    private async void LoadDocument() {
        try {
            Ping pingSender = new Ping();
            PingReply reply = await pingSender.SendPingAsync("10.0.2.2");
            if (reply.Status == IPStatus.Success) {
                //...
            }
        }
        catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
        try {
            documentFullPath = Path.Combine(FileSystem.Current.AppDataDirectory, _fileName);
            //string fileName = "yetAnotherDoc.pdf";
            //var address = $"http://10.0.2.2:{MauiProgram.PORT}/api/File/{_fileName}";
            //string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
            //using (Stream s = _httpClient.GetStreamAsync($"http://10.0.2.2:{MauiProgram.PORT}/api/File/{_fileName}").Result)
            //using (var fileStream = new FileStream(fileName, FileMode.Create)) {
            //    s.CopyTo(fileStream);
            //    certificateFullPath = await CopyWorkingFilesToAppData(defaultCertificateName);
            //    documentFullPath = await CopyWorkingFilesToAppData(fileName);
            //}
        }
        catch (Exception e) {

        }
    }
    public async Task<string> CopyWorkingFilesToAppData(string fileName) {
        using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(fileName);
        string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
        using FileStream outputStream = File.OpenWrite(targetFile);
        fileStream.CopyTo(outputStream);
        return targetFile;
    }
    async void SignPdf(byte[] signatureImage) {
        CloseSignatureView();
        string signedPdfFullName = Path.Combine(FileSystem.Current.AppDataDirectory, Path.GetFileNameWithoutExtension(documentFullPath) + "_Signed1.pdf");
        IEnumerable<PdfFormFieldFacade> fields = GetDocumentFields();
        using var signer = new PdfDocumentSigner(documentFullPath);
        string signatureFieldName = null;
        //var signatureField = fields.FirstOrDefault(f => f.Type == PdfFormFieldType.Signature) as PdfSignatureFormFieldFacade;
        var signatureField = fields.FirstOrDefault(_ => false);
        //if (signatureField == null) { 
        if (true) {
            //await Shell.Current.DisplayAlert("No Signature Fields Found", "A new signature field with a default position will be created", "OK");
        }
        else {
            signatureFieldName = signatureField.FullName;
            signer.ClearSignatureField(signatureFieldName);
        }
        signer.SaveDocument(signedPdfFullName, CreateUserSignature(signatureFieldName, defaultCertificatePassword, "USA", "Jane Cooper", "Acknowledgement", signatureImage));
        documentFullPath = signedPdfFullName;
        UpdatePreview();
    }
    IEnumerable<PdfFormFieldFacade> GetDocumentFields() {
        using var processor = new PdfDocumentProcessor();
        processor.LoadDocument(documentFullPath);
        PdfDocumentFacade documentFacade = processor.DocumentFacade;
        PdfAcroFormFacade acroForm = documentFacade.AcroForm;
        return acroForm.GetFields();
    }
    PdfSignatureBuilder CreateUserSignature(string signatureFieldName, string password, string location, string contactInfo, string reason, byte[] signatureImage) {
        var coord = imageEdit.GetCropAreaCoordinates();
        var dpiCorrection = 1.52;// 200 / 72;
        var left = coord.Left - coord.Width;
        var bottom = 1200 - coord.Bottom;
        var right = coord.Right + coord.Width;
        var top = 1200 - coord.Top;
        left /= dpiCorrection;
        bottom /= dpiCorrection;
        right /= dpiCorrection;
        top /= dpiCorrection;
        Pkcs7Signer pkcs7Signature = new Pkcs7Signer(certificateFullPath, password, HashAlgorithmType.SHA256);
        PdfSignatureBuilder userSignature;
        if (signatureFieldName == null) {
            //userSignature = new PdfSignatureBuilder(pkcs7Signature, new PdfSignatureFieldInfo(1) { SignatureBounds = new PdfRectangle(394, 254, 482, 286) });
            userSignature = new PdfSignatureBuilder(pkcs7Signature, new PdfSignatureFieldInfo(1) { SignatureBounds = new PdfRectangle(left, bottom, right, top) });
        }
        else
            userSignature = new PdfSignatureBuilder(pkcs7Signature, signatureFieldName);
        userSignature.Location = location;
        userSignature.Name = contactInfo;
        userSignature.Reason = reason;
        if (signatureImage != null) {
            userSignature.SetImageData(signatureImage);
        }
        return userSignature;
    }
    private void UpdatePreview() {
        using Stream pdfStream = File.OpenRead(documentFullPath);
        var processor = new PdfDocumentProcessor() { RenderingEngine = PdfRenderingEngine.Skia };
        processor.LoadDocument(pdfStream);
        DXBitmap image = processor.CreateDXBitmap(PageNumber, 1200);
        using MemoryStream previewImageStream = new MemoryStream();
        image.Save(previewImageStream, DXImageFormat.Png);
        previewImageStream.Seek(0, SeekOrigin.Begin);
        var img = SKBitmap.Decode(previewImageStream);
        PdfPreview = (SKBitmapImageSource)img;
    }

    private async void OpenFile() {
        await PickAndShow(new PickOptions
        {
            PickerTitle = "Select a PDF file",
            FileTypes = FilePickerFileType.Pdf
        });
    }
    public async Task PickAndShow(PickOptions options) {
        try {
            var result = await FilePicker.Default.PickAsync(options);
            if (result != null) {
                if (result.FileName.EndsWith("pdf", StringComparison.OrdinalIgnoreCase)) {
                    var stream = await result.OpenReadAsync();
                    documentFullPath = result.FullPath;
                    UpdatePreview();
                }
            }
        }
        catch {
            // The user canceled or something went wrong
        }
    }
}