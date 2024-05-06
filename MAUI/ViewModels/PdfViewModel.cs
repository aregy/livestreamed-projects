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
using System.Globalization;
using CommunityToolkit.Maui.Views;
using DevExpress.Maui.Core.Internal;
using Microsoft.Maui.Graphics.Platform;
using Newtonsoft.Json;
using System.IO;
//using DevExpress.Blazor.Internal.Upload;
//using DevExpress.Blazor.Upload.Internal;
namespace SignPDF.ViewModels;

public class PdfViewModel : BindableBase {
    private string _fileName;
    //private DocumentViewModel _documentViewModel;
    public ImageEdit _ImageEdit;
    public ImageSource _PdfPreview;
    public DrawingView _SignatureDrawer;
    const string defaultDocumentName = "JewelCityLetter.pdf";
    const string defaultCertificateName = "pfxCertificate.pfx";
    const string defaultCertificatePassword = "123";
    string certificateFullPath;
    private string documentFullPath {
        get {
            return Path.Combine(FileSystem.Current.AppDataDirectory, _fileName);
        }
    }
    private bool isSignatureViewOpened;
    private void OpenSignatureView() {
        IsSignatureViewOpened = true;
    }
    private void CloseSignatureView() {
        IsSignatureViewOpened = false;
    }
    private async void LoadDocument() {
        try {
            Ping pingSender = new Ping();
            PingReply reply = await pingSender.SendPingAsync($"{MauiProgram.BASE_ADDRESS}");
            if (reply.Status == IPStatus.Success) {
                //...
            }
        }
        catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
        try {
            //documentFullPath = await CopyWorkingFilesToAppData(fileName);
            certificateFullPath = await CopyWorkingFilesToAppData(defaultCertificateName);
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
    int i = 0;
    async void SignPdf(byte[] signatureImage) {
        using (var imageStream = await _SignatureDrawer.GetImageStream(200, 200)) {
            imageStream.Seek(0, SeekOrigin.Begin);
            Microsoft.Maui.Graphics.IImage image = PlatformImage.FromStream(imageStream, Microsoft.Maui.Graphics.ImageFormat.Jpeg);
            signatureImage = image.AsBytes(Microsoft.Maui.Graphics.ImageFormat.Png);
        }
        CloseSignatureView();
        i++;
        string pdfFileName2 = Path.Combine(Path.GetFileNameWithoutExtension(documentFullPath) + $"_Signed-{DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}-{i}.pdf");
        using var signer = new PdfDocumentSigner(documentFullPath);
        string signatureFieldName = null;

        var sig = CreateUserSignature(signatureFieldName, defaultCertificatePassword, "USA", "Jane Cooper", "Acknowledgement", signatureImage);
        signer.SaveDocument(Path.Combine(FileSystem.Current.AppDataDirectory, pdfFileName2), sig);

        //using Stream pdfStream = File.OpenRead(documentFullPath);
        //var processor = new PdfDocumentProcessor() { RenderingEngine = PdfRenderingEngine.Skia };
        //processor.LoadDocument(pdfStream);
        //
        _fileName = pdfFileName2;
        await UpdatePreview();
        try {
            UpdateServerAsync();
        }
        catch (Exception ex) {

        }
    }
    async Task UpdateServerAsync() {
        //var selectedFile = new FileInputSelectedFile();
        //selectedFile.Guid = Guid.NewGuid().ToString();
        //selectedFile.Name = _fileName;
        //selectedFile.LoadedBytes = 0;
        var file = File.Open(documentFullPath, FileMode.Open);
        int chunckSize = 20000;
        //int chunckSize = 2097152; //2MB
        int totalChunks = (int)(file.Length / chunckSize);
        if (file.Length % chunckSize != 0) {
            totalChunks++;
        }
        var fileGuid = Guid.NewGuid();
        for (int i = 0; i < totalChunks; i++) {
            long position = (i * (long)chunckSize);
            int toRead = (int)Math.Min(file.Length - position, chunckSize);
            byte[] buffer = new byte[toRead];
            await file.ReadAsync(buffer, 0, buffer.Length);

            using (MultipartFormDataContent form = new MultipartFormDataContent()) {
                form.Add(new ByteArrayContent(buffer), "imageUpload", _fileName);
                //form.Add(new StringContent(id.ToString()), "id");
                var meta = JsonConvert.SerializeObject(new SignPDF.Data.ExChunkMetadata
                {
                    FileName = _fileName,
                    Index = i,
                    TotalCount = totalChunks,
                    FileSize = (int)file.Length,
                    FileGuid = fileGuid.ToString()
                });
                form.Add(new StringContent(meta), "chunkMetadata");
                try {
                    var response = await MauiProgram._NavigationViewModel._HttpClient.PostAsync($"{MauiProgram.BASE_ADDRESS}:{MauiProgram.PORT}/api/File/UploadFile", form);//.ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode) {
                        throw new NetworkInformationException();
                    }
                }
                catch (Exception ex) {
                }
            }
        }

    }
    //protected Task<bool> PerformFileUploadStartCoreAsync(FileInputSelectedFile file) {
    //    var ChunkSize = 20000; // Given by ChunkSize = "20000" in my DevEvexBlazor > Index.razor file
    //    var metadataArgs = new FileMetadataEventArgs
    //    {
    //        RequestOptions = new ChunkJSItem
    //        {
    //            FileName = file.Name,
    //            FileSize = file.Size,
    //            FileType = file.Type,
    //            FileGuid = file.Guid,
    //            LastModified = file.LastModified,
    //            Index = ChunkSize > 0 ? Convert.ToInt64(file.LoadedBytes) / ChunkSize : 0,
    //            TotalCount = ChunkSize > 0 ? Convert.ToInt64(file.Size / ChunkSize) : 1
    //        }
    //    };
    //    ProcessCustomizeFormRequest(metadataArgs);
    //    return Task.FromResult(true);
    //}
    //protected Dictionary<string, FileJSOptions> FilesOptions { get; set; } // TODO fill with Guids
    //internal Func<ChunkMetadata, object> CustomizeFormData { get; set; } // TOOD fill with ChunkMetadatas

    //void ProcessCustomizeFormRequest(FileMetadataEventArgs args) {
    //    var chunkMetadata = new ChunkMetadata(args.RequestOptions);
    //    var customData = CustomizeFormData?.Invoke(chunkMetadata);
    //    FileJSOptions fileOptions = FilesOptions[args.RequestOptions.FileGuid];
    //    fileOptions.CustomFormDataOptions = new
    //    {
    //        CustomData = customData,
    //        FileGuid = chunkMetadata.UploadId,
    //        ChunkIndex = chunkMetadata.Index
    //    };
    //}
    //IEnumerable<PdfFormFieldFacade> GetDocumentFields() {
    //    using var processor = new PdfDocumentProcessor();
    //    processor.LoadDocument(documentFullPath);
    //    PdfDocumentFacade documentFacade = processor.DocumentFacade;
    //    PdfAcroFormFacade acroForm = documentFacade.AcroForm;
    //    return acroForm.GetFields();
    //}
    PdfSignatureBuilder CreateUserSignature(string signatureFieldName, string password, string location, string contactInfo, string reason, byte[] signatureImage) {
        var coord = _ImageEdit.GetCropAreaCoordinates();
        var dpiCorrection = 1.52;
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
        if (signatureFieldName == null)
            userSignature = new PdfSignatureBuilder(pkcs7Signature, new PdfSignatureFieldInfo(1) { SignatureBounds = new PdfRectangle(left, bottom, right, top) });
        else
            userSignature = new PdfSignatureBuilder(pkcs7Signature, signatureFieldName);
        userSignature.Location = location;
        userSignature.Location = location;
        userSignature.Name = contactInfo;
        userSignature.Reason = reason;
        if (signatureImage != null)
            userSignature.SetImageData(signatureImage);

        return userSignature;
    }
    private async Task UpdatePreview() {
        using Stream pdfStream = File.OpenRead(documentFullPath);
        var processor = new PdfDocumentProcessor() { RenderingEngine = PdfRenderingEngine.Skia };
        processor.LoadDocument(pdfStream);
        PageCount = processor.Document.Pages.Count;
        DXBitmap image = processor.CreateDXBitmap(PageNumber, 1200);
        using MemoryStream previewImageStream = new MemoryStream();
        image.Save(previewImageStream, DXImageFormat.Png);
        previewImageStream.Seek(0, SeekOrigin.Begin);
        var img = SKBitmap.Decode(previewImageStream);
        PdfPreview = (SKBitmapImageSource)img;
    }
    private void DecrementPage() {
        if (PageNumber == 1)
            return;
        PageNumber--;
        UpdatePreview();
    }
    private void IncrementPage() {
        if (PageNumber == PageCount)
            return;
        PageNumber++;
        UpdatePreview();
    }
    public ImageSource PdfPreview {
        get {
            return _PdfPreview;
        }
        set {
            _PdfPreview = value;
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
    public ICommand IncrementPageCommand {
        get;
        set;
    }
    public ICommand DecrementPageCommand {
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
    public int PageCount { get; private set; }
    public PdfViewModel(string fileName) {
        _fileName = fileName;
        LoadDocument();
        UpdatePreview();
        SignPdfCommand = new Command<byte[]>(SignPdf);
        //OpenFileCommand = new Command(OpenFile);
        OpenSignatureViewCommand = new Command(OpenSignatureView);
        CloseSignatureViewCommand = new Command(CloseSignatureView);
        //ImageLoadedCommand = new Command(OnImageLoaded);
        DecrementPageCommand = new Command(DecrementPage);
        IncrementPageCommand = new Command(IncrementPage);
    }
}