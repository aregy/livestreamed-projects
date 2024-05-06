using DevExpress.Maui.Core;
using DevExpress.Pdf.Native.BouncyCastle.Asn1.Pkcs;
using SignPDF.Data;
using SignPDF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SignPDF.ViewModels;
public class NavigationViewModel : BindableBase {
    public ObservableCollection<FilterItem> PredefinedFilters {
        get;
        set;
    }
    public BindingList<FilterItem> SelectedFilters {
        get;
        set;
    }
    string _filter;
    public string Filter {
        get {
            return _filter;
        }
        set {
            _filter = value;
            RaisePropertiesChanged();
        }
    }
    private IList<DocumentViewModel> _documents;
    public IList<DocumentViewModel> Documents {
        get { return _documents; }
        set {
            if (_documents != value) {
                _documents = value;
                OnPropertyChanged("Documents");
            }
        }
    }
    bool _isRefreshing = false;
    public bool IsRefreshing {
        get { return _isRefreshing; }
        set {
            if (_isRefreshing != value) {
                _isRefreshing = value;
                OnPropertyChanged("IsRefreshing");
            }
        }
    }
    public event PropertyChangedEventHandler _PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") {
        _PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    ICommand pullToRefreshCommand = null;
    public ICommand PullToRefreshCommand {
        get { return pullToRefreshCommand; }
        set {
            if (pullToRefreshCommand != value) {
                pullToRefreshCommand = value;
                OnPropertyChanged("PullToRefreshCommand");
            }
        }
    }
    void ExecutePullToRefreshCommand() {
        Task.Run(() => {
            Thread.Sleep(1000);
            this.UpdateDocuments();
            IsRefreshing = false;
        });
    }
    int _count = 0;
    private void UpdateDocuments() {
        //this._Documents.Add(new DocumentViewModel { Name = $"Doc #{++_count}", Status = (SignStatus)(_count % 3) });
    }
    private void SelectedFiltersChanged(object sender, ListChangedEventArgs e) {
        if (SelectedFilters.Count > 0)
            Filter = "[Status] == 0";
        else
            Filter = string.Empty;
    }
    private async void DXCollectionView_Tap(DocumentViewModel documentViewModel) {

        string fileName = documentViewModel.Name;
        var address = $"{MauiProgram.BASE_ADDRESS}:{MauiProgram.PORT}/api/File/{documentViewModel.Id}";
        string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
        try {
            var httpResponse = await _HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, address), HttpCompletionOption.ResponseHeadersRead);
            using (var fileStream = System.IO.File.OpenWrite(targetFile)) { //(targetFile, FileMode.Create)) {
                var streamToReadFrom = await httpResponse.Content.ReadAsStreamAsync();// CopyToAsync(fileStream);
                await streamToReadFrom.CopyToAsync(fileStream);
                //certificateFullPath = await CopyWorkingFilesToAppData(defaultCertificateName);
                //documentFullPath = await CopyWorkingFilesToAppData(fileName);
            }
        }
        catch (Exception e) {

        }
        Page page;
        if (documentViewModel.Status == SignStatus.Requested) {
            //page = (Page)Activator.CreateInstance(typeof(SignPage), documentViewModel.Id);
            //var address = $"{typeof(SignPage).Name}?id=${documentViewModel.Id}";

            await Shell.Current.GoToAsync(nameof(SignPage), true, new Dictionary<string, object>
            {
                { "FileName", fileName }
            });

        }
        else {
            await Shell.Current.GoToAsync(nameof(ViewPage), true, new Dictionary<string, object>
            {
                { "FileName", fileName }
            });
        }
    }
    public List<string> Items { get; set; } = new List<string>();
    public ICommand TapCommand { get; set; }
    public HttpClient _HttpClient;
    private async Task FetchDocumentsAsync() {
        _HttpClient = new HttpClient() { BaseAddress = new Uri($"{MauiProgram.BASE_ADDRESS}:{MauiProgram.PORT}/") };
        try {
            using (Stream s = _HttpClient.GetStreamAsync($"{MauiProgram.BASE_ADDRESS}:{MauiProgram.PORT}/api/File/List").Result)
            using (StreamReader sr = new StreamReader(s))
            using (Newtonsoft.Json.JsonReader reader = new Newtonsoft.Json.JsonTextReader(sr)) {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                var fileTags = serializer.Deserialize<List<ExFileTag>>(reader);
                foreach (var fileTag in fileTags) {
                    Documents.Add(new DocumentViewModel() { Name = fileTag.Name, Status = fileTag.SignStatus, Id = fileTag.Id, SignedAt = fileTag.LastWriteTime, });
                }
            }
        }
        catch (AggregateException ex) {
            Console.WriteLine(ex);
        }
    }
    public NavigationViewModel() {
        MauiProgram._NavigationViewModel = this;
        _documents = new List<DocumentViewModel>();
        PullToRefreshCommand = new Command(ExecutePullToRefreshCommand);
        SelectedFilters = new BindingList<FilterItem>();
        PredefinedFilters = new ObservableCollection<FilterItem>() {
                new FilterItem(){ DisplayText= "Pending", Filter = "[Status] == 0" }
            };
        SelectedFilters.ListChanged += SelectedFiltersChanged;
        TapCommand = new Command<DocumentViewModel>(DXCollectionView_Tap);
        FetchDocumentsAsync();
    }
}