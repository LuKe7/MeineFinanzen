// 04.10.2016   -Model-  CollZahlungen.cs 
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
namespace MeineFinanzen.Model {
    public class CollZahlungen : ObservableCollection<Zahlung> {
        public CollZahlungen()        //  : base()
        {
            //Add(new Zahlung("", "", ""));                      
        }
    }
    public class Zahlung : INotifyPropertyChanged, IEditableObject {
        //EntryDate;ValueDate;Value;AcctNo;BankCode;Name1;Name2;PaymtPurpose;EntryText;PrimaNotaNo;TranTypeIdCode;ZkaTranCode;TextKeyExt;BankRef;OwnerRef;SupplementaryDetails        
        public string Anzahl { get; set; }
        public string Isin { get; set; }
        public string Name { get; set; }
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
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
    }
    public class NullImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return DependencyProperty.UnsetValue;
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            // According to https://msdn.microsoft.com/en-us/library/system.windows.data.ivalueconverter.convertback(v=vs.110).aspx#Anchor_1
            // (kudos Scott Chamberlain), if you do not support a conversion 
            // back you should return a Binding.DoNothing or a 
            // DependencyProperty.UnsetValue
            return Binding.DoNothing;
            // Original code:
            // throw new NotImplementedException();
        }
    }
}