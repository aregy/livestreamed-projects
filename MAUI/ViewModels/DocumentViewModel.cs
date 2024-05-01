using SignPDF.Data;
using System.Windows.Input;

namespace SignPDF.ViewModels;
public class FilterItem {
    public string DisplayText { get; set; }
    public string Filter { get; set; }
}
public class DocumentViewModel {
    public ICommand TapCommand { get; set; }
    public ICommand ChangeStateCommand { get; }
    public SignStatus Status { get; set; }
    public DateTime? SignedAt { get; set; }
    public string Name { get; set; }
    public Guid Id { get; set; }
    public bool IsPending {
        get {
            return Status == SignStatus.Requested;
        }
    }
    public DocumentViewModel() {
        TapCommand = new Command(TapAction);
    }
    private void TapAction() {
        this.Status = SignStatus.Declined;
    }
}