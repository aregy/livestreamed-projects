using DevExpress.Maui.Core;
using DevExpress.Pdf.Native.BouncyCastle.Asn1.Pkcs;
using SignPDF.Data;
using SignPDF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
//using ThreadNetwork;
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
    string filter;
    public string Filter {
        get {
            return filter;
        }
        set {
            filter = value;
            RaisePropertiesChanged();
        }
    }
    private IList<DocumentViewModel> _Documents;
    public IList<DocumentViewModel> Documents {
        get { return _Documents; }
        set {
            if (_Documents != value) {
                _Documents = value;
                OnPropertyChanged("Documents");
            }
        }
    }
    bool isRefreshing = false;
    public bool IsRefreshing {
        get { return isRefreshing; }
        set {
            if (isRefreshing != value) {
                isRefreshing = value;
                OnPropertyChanged("IsRefreshing");
            }
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        try {
            string fileName = "yetAnotherDoc.pdf";
            var address = $"http://10.0.2.2:{MauiProgram.PORT}/api/File/{documentViewModel.Id}";
            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
            //using (Stream s = _httpClient.GetStreamAsync(address, HttpCompletionOption.ResponseHeadersRead).Result)
            //var httpResponse = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, address), HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            var httpResponse = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, address), HttpCompletionOption.ResponseHeadersRead);
            using (var fileStream = System.IO.File.OpenWrite(targetFile)) { //(targetFile, FileMode.Create)) {
                //s.CopyTo(fileStream);
                var streamToReadFrom = await httpResponse.Content.ReadAsStreamAsync();// CopyToAsync(fileStream);
                await streamToReadFrom.CopyToAsync(fileStream);
                //certificateFullPath = await CopyWorkingFilesToAppData(defaultCertificateName);
                //documentFullPath = await CopyWorkingFilesToAppData(fileName);
            }
            Page page;
            if (documentViewModel.Status == SignStatus.Requested) {
                //page = (Page)Activator.CreateInstance(typeof(SignPage), documentViewModel.Id);
                //var address = $"{typeof(SignPage).Name}?id=${documentViewModel.Id}";

                Shell.Current.GoToAsync(nameof(SignPage), true, new Dictionary<string, object>
            {
                { "FileName", fileName }
            });//(address);
               //Shell.Current.GoToAsync($"{nameof(SignPage)}");

            }
            else {
                //page = (Page)Activator.CreateInstance(typeof(NewPage2));
            }
        }
        catch (Exception e) {

        }

    }
    public List<string> Items { get; set; } = new List<string>();
    public ICommand TapCommand { get; set; }

    public HttpClient _httpClient;
    public NavigationViewModel() {
        _Documents = new List<DocumentViewModel>();
        PullToRefreshCommand = new Command(ExecutePullToRefreshCommand);
        SelectedFilters = new BindingList<FilterItem>();
        PredefinedFilters = new ObservableCollection<FilterItem>() {
                new FilterItem(){ DisplayText= "Pending", Filter = "[Status] == 0" }
            };
        SelectedFilters.ListChanged += SelectedFiltersChanged;
        TapCommand = new Command<DocumentViewModel>(DXCollectionView_Tap);
        _httpClient = new HttpClient() { BaseAddress = new Uri("http://10.0.2.2:5184/") };
        try {
            //var response = await client.GetAsync("api/File/List");

            using (Stream s = _httpClient.GetStreamAsync($"http://10.0.2.2:{MauiProgram.PORT}/api/File/List").Result)
            //using (Stream s = client.GetStreamAsync("http://10.0.2.2:7290/api/File/List").Result)
            //var dotNetStreamRef = new DotNetStreamReference(s);
            using (StreamReader sr = new StreamReader(s))
            using (Newtonsoft.Json.JsonReader reader = new Newtonsoft.Json.JsonTextReader(sr)) {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                // read the json from a stream
                // json size doesn't matter because only a small piece is read at a time from the HTTP request
                var fileTags = serializer.Deserialize<List<FileTag>>(reader);
                foreach (var fileTag in fileTags) {
                    Documents.Add(new DocumentViewModel() { Name = fileTag.Name, Status = fileTag.SignStatus, Id = fileTag.Id, SignedAt = fileTag.LastWriteTime, });
                }
            }
        }
        catch (AggregateException ex) {
            Console.WriteLine(ex);
        }
    }
}
//public class Task : INotifyPropertyChanged
//{
//    public string Description { get; private set; }
//    bool isTaskCompleted;
//    public bool IsTaskCompleted
//    {
//        get => isTaskCompleted;
//        set
//        {
//            isTaskCompleted = value;
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTaskCompleted)));
//            UpdateState();
//        }
//    }
//    Color itemColor;
//    public Color ItemColor
//    {
//        get => itemColor;
//        private set
//        {
//            itemColor = value;
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemColor)));
//        }
//    }
//    string actionText;
//    public string ActionText
//    {
//        get => actionText;
//        private set
//        {
//            actionText = value;
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActionText)));
//        }
//    }
//    string actionIcon;
//    public string ActionIcon
//    {
//        get => actionIcon;
//        private set
//        {
//            actionIcon = value;
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActionIcon)));
//        }
//    }
//    public ICommand ChangeStateCommand { get; }
//    public event PropertyChangedEventHandler PropertyChanged;
//    public Task(string description)
//    {
//        ChangeStateCommand = new Command(() => IsTaskCompleted = !IsTaskCompleted);
//        Description = description;
//        UpdateState();
//    }
//    void UpdateState()
//    {
//        ItemColor = IsTaskCompleted ? Color.FromArgb("#c6eccb") : Color.FromArgb("#e6e6e6");
//        ActionText = IsTaskCompleted ? "To Do" : "Done";
//        ActionIcon = IsTaskCompleted ? "uncompletetask" : "completetask";
//    }
//}