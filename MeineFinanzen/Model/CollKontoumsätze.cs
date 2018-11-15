// 03.10.2016   -Model-  CollKontoumsätze.cs 
using System.Collections.ObjectModel;
using System.ComponentModel;
namespace MeineFinanzen.Model {
    public class CollKontoumsätze : ObservableCollection<Kontoumsatz> { }
    public class Kontoumsatz : INotifyPropertyChanged, IEditableObject {
        //EntryDate;ValueDate;Value;AcctNo;BankCode;Name1;Name2;PaymtPurpose;EntryText;PrimaNotaNo;TranTypeIdCode;ZkaTranCode;TextKeyExt;BankRef;OwnerRef;SupplementaryDetails        
        public string Kontonummer { get; set; }
        public string EntryDate { get; set; }
        public string ValueDate { get; set; }
        public string Value { get; set; }
        public string AcctNo { get; set; }
        public string BankCode { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string PaymtPurpose { get; set; }
        public string EntryText { get; set; }
        public string PrimaNotaNo { get; set; }
        public string TranTypeIdCode { get; set; }
        public string ZkaTranCode { get; set; }
        public string TextKeyExt { get; set; }
        public string BankRef { get; set; }
        public string OwnerRef { get; set; }
        public string SupplementaryDetails { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
    }
}