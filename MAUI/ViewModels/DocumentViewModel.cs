
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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SignPDF.ViewModels
{
    public class FilterItem
    {
        public string DisplayText { get; set; }
        public string Filter { get; set; }
    }
    public enum SignStatus { Requested, Declined, Signed }
    internal class DocumentViewModel
    {
        public ICommand TapCommand { get; set; }
        public ICommand ChangeStateCommand { get; }
        public string Name { get; set; }
        public SignStatus Status { get; set; }
        public DateTime? SignedAt { get; set; }
        public bool IsPending
        {
            get
            {
                return Status == SignStatus.Requested;
            }
        }
        public DocumentViewModel()
        {
            TapCommand = new Command(TapAction);
        }
        private void TapAction()
        {
            this.Status = SignStatus.Declined;
        }
        public void Add()
        {

        }

    }
    internal class NavigationViewModel : BindableBase
    {
        public string Describer { get; set; } = "One Family";
        public void Add()
        {

        }
        public ObservableCollection<FilterItem> PredefinedFilters
        {
            get;
            set;
        }
        public BindingList<FilterItem> SelectedFilters
        {
            get;
            set;
        }
        string filter;
        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
                RaisePropertiesChanged();
            }
        }
        private IList<DocumentViewModel> _Documents;
        public IList<DocumentViewModel> Documents
        {
            get { return _Documents; }
            set
            {
                if (_Documents != value)
                {
                    _Documents = value;
                    OnPropertyChanged("Documents");
                }
            }
        }

        bool isRefreshing = false;
        public bool IsRefreshing
        {
            get { return isRefreshing; }
            set
            {
                if (isRefreshing != value)
                {
                    isRefreshing = value;
                    OnPropertyChanged("IsRefreshing");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        ICommand pullToRefreshCommand = null;
        public ICommand PullToRefreshCommand
        {
            get { return pullToRefreshCommand; }
            set
            {
                if (pullToRefreshCommand != value)
                {
                    pullToRefreshCommand = value;
                    OnPropertyChanged("PullToRefreshCommand");
                }
            }
        }
        void ExecutePullToRefreshCommand()
        {
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                this.UpdateDocuments();
                IsRefreshing = false;
            });
        }
        int _count = 0;
        private void UpdateDocuments()
        {
            this._Documents.Add(new DocumentViewModel { Name = $"Doc #{++_count}", Status = (SignStatus)(_count % 3) });
        }
        private void SelectedFiltersChanged(object sender, ListChangedEventArgs e)
        {
            if (SelectedFilters.Count > 0)
                Filter = "[Status] == 0";
            else
                Filter = string.Empty;
        }
        private void DXCollectionView_Tap(DocumentViewModel documentViewModel)
        {
            Page page;
            if (documentViewModel.Status == SignStatus.Requested)
            {
                page = (Page)Activator.CreateInstance(typeof(MainPage));
            }
            else
            {
                page = (Page)Activator.CreateInstance(typeof(NewPage2));
            }
            if (page != null)
            {
                Shell.Current.Navigation.PushAsync(page);
            }
            else
            {
                throw new Exception("documentViewModel does not correspond to further document action");
            }

        }
        public List<string> Items { get; set; } = new List<string>();
        public ICommand TapCommand { get; set; }

        public NavigationViewModel()
        {
            _Documents = new List<DocumentViewModel>();
            _Documents.Add(new DocumentViewModel { Name = "Jewel City Letter", Status = SignStatus.Requested });
            _Documents.Add(new DocumentViewModel { Name = "Maccabee Town Center", Status = SignStatus.Signed });
            _Documents.Add(new DocumentViewModel { Name = "Form 6166", Status = SignStatus.Requested });
            _Documents.Add(new DocumentViewModel { Name = "Media City Letter", Status = SignStatus.Requested });
            _Documents.Add(new DocumentViewModel { Name = "Mattel Town Center", Status = SignStatus.Signed });
            _Documents.Add(new DocumentViewModel { Name = "Form 8802", Status = SignStatus.Requested });
            PullToRefreshCommand = new Command(ExecutePullToRefreshCommand);
            SelectedFilters = new BindingList<FilterItem>();
            PredefinedFilters = new ObservableCollection<FilterItem>() {
                new FilterItem(){ DisplayText= "Pending", Filter = "[Status] == 0" }
            };
            SelectedFilters.ListChanged += SelectedFiltersChanged;
            TapCommand = new Command<DocumentViewModel>(DXCollectionView_Tap);
            System.Net.Http.HttpClient client = new HttpClient();
            
            using (Stream s = client.GetStreamAsync("http://10.0.2.2:5184/api/File/List").Result)
            using (StreamReader sr = new StreamReader(s))
            using (Newtonsoft.Json.JsonReader reader = new Newtonsoft.Json.JsonTextReader(sr))
            {   
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                // read the json from a stream
                // json size doesn't matter because only a small piece is read at a time from the HTTP request
                var p = serializer.Deserialize<List<FileTag>>(reader);
                this.Items.Clear();
                foreach (var k in p)
                {
                    Console.WriteLine(k.Name, k.ServerPath);
                    Items.Add(k.Name.ToString());
                }

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
}
