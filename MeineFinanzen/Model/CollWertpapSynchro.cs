// 03.04.2018   -Model-  CollWertpapSynchro.cs 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
namespace MeineFinanzen.Model {
    // ObservableCollection: Stellt eine dynamische Datenauflistung dar,
    // die Benachrichtigungen bereitstellt,
    // wenn Elemente hinzugefügt oder entfernt werden
    // bzw. wenn die gesamte Liste aktualisiert wird.
    // public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    public class CollWertpapSynchro : ObservableCollection<WertpapSynchro>, IComparable {
        public CollWertpapSynchro() { }
        public int CompareTo(object obj) {
            if (obj == null)
                return 1;
            WertpapSynchro wp = obj as WertpapSynchro;
            if (wp != null)
                return wp.WPSISIN.CompareTo(((WertpapSynchro)obj).WPSISIN);
            throw new NotImplementedException();
        }
    }
    public class WertpapSynchro : INotifyPropertyChanged, IEditableObject {
        public float WPSAnzahl { get; set; }
        public string WPSName { get; set; }
        public string WPSURL { get; set; }
        public DateTime WPSKursZeit { get; set; }
        public string WPSISIN { get; set; }
        private double _WPSKurs;
        public double WPSKurs {
            get { return _WPSKurs; }
            set {
                _WPSKurs = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPSKurs"));
            }
        }
        public double WPSProzentAenderung { get; set; }
        public int WPSType { get; set; }
        public Single WPSSharpe { get; set; }
        public string WPXPathKurs { get; set; }
        public string WPXPathAend { get; set; }
        public string WPXPathZeit { get; set; }
        public string WPXPathSharp { get; set; }
        public string WPURLSharp { get; set; }
        public string WPSColor { get; set; }
        public string WPSVorgabeInt2 { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        private void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
    }   
    public class Person : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        private string _Name;
        public string Name {
            get { return _Name; }
            set {
                _Name = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            }
        }
        private int? _Alter;
        public int? Alter {
            get { return _Alter; }
            set {
                _Alter = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Alter"));
            }
        }
        private string _Adresse;
        public string Adresse {
            get { return _Adresse; }
            set {
                _Adresse = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Adresse"));
            }
        }
    }
    public class CustomersList : CollectionBase, IBindingList {
        private ListChangedEventArgs resetEvent = new ListChangedEventArgs(ListChangedType.Reset, -1);
        private ListChangedEventHandler onListChanged;
        public void LoadCustomers() {
            IList l = (IList)this;
            l.Add(ReadCustomer1());
            l.Add(ReadCustomer2());
            OnListChanged(resetEvent);
        }
        public Customer this[int index] {
            get {
                return (Customer)(List[index]);
            }
            set {
                List[index] = value;
            }
        }
        public int Add(Customer value) {
            return List.Add(value);
        }
        public Customer AddNew() {
            return (Customer)((IBindingList)this).AddNew();
        }
        public void Remove(Customer value) {
            List.Remove(value);
        }
        protected virtual void OnListChanged(ListChangedEventArgs ev) {
            if (onListChanged != null) {
                onListChanged(this, ev);
            }
        }
        protected override void OnClear() {
            foreach (Customer c in List) {
                c.Parent = null;
            }
        }
        protected override void OnClearComplete() {
            OnListChanged(resetEvent);
        }
        protected override void OnInsertComplete(int index, object value) {
            Customer c = (Customer)value;
            c.Parent = this;
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }
        protected override void OnRemoveComplete(int index, object value) {
            Customer c = (Customer)value;
            c.Parent = this;
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        }
        protected override void OnSetComplete(int index, object oldValue, object newValue) {
            if (oldValue != newValue) {

                Customer oldcust = (Customer)oldValue;
                Customer newcust = (Customer)newValue;

                oldcust.Parent = null;
                newcust.Parent = this;


                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
            }
        }
        // Called by Customer when it changes.
        internal void CustomerChanged(Customer cust) {

            int index = List.IndexOf(cust);

            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
        }
        // Implements IBindingList.
        bool IBindingList.AllowEdit {
            get { return true; }
        }
        bool IBindingList.AllowNew {
            get { return true; }
        }
        bool IBindingList.AllowRemove {
            get { return true; }
        }
        bool IBindingList.SupportsChangeNotification {
            get { return true; }
        }
        bool IBindingList.SupportsSearching {
            get { return false; }
        }
        bool IBindingList.SupportsSorting {
            get { return false; }
        }
        // Events.
        public event ListChangedEventHandler ListChanged {
            add {
                onListChanged += value;
            }
            remove {
                onListChanged -= value;
            }
        }
        // Methods.
        object IBindingList.AddNew() {
            Customer c = new Customer(this.Count.ToString());
            List.Add(c);
            return c;
        }
        // Unsupported properties.
        bool IBindingList.IsSorted {
            get { throw new NotSupportedException(); }
        }
        ListSortDirection IBindingList.SortDirection {
            get { throw new NotSupportedException(); }
        }
        PropertyDescriptor IBindingList.SortProperty {
            get { throw new NotSupportedException(); }
        }
        // Unsupported Methods.
        void IBindingList.AddIndex(PropertyDescriptor property) {
            throw new NotSupportedException();
        }
        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction) {
            throw new NotSupportedException();
        }
        int IBindingList.Find(PropertyDescriptor property, object key) {
            throw new NotSupportedException();
        }
        void IBindingList.RemoveIndex(PropertyDescriptor property) {
            throw new NotSupportedException();
        }
        void IBindingList.RemoveSort() {
            throw new NotSupportedException();
        }
        // Worker functions to populate the list with data.
        private static Customer ReadCustomer1() {
            Customer cust = new Customer("536-45-1245");
            cust.FirstName = "Jo";
            cust.LastName = "Brown";
            return cust;
        }
        private static Customer ReadCustomer2() {
            Customer cust = new Customer("246-12-5645");
            cust.FirstName = "Robert";
            cust.LastName = "Brown";
            return cust;
        }
    }
    public class Customer : IEditableObject {
        struct CustomerData {
            internal string id;
            internal string firstName;
            internal string lastName;
        }
        private CustomersList parent;
        private CustomerData custData;
        private CustomerData backupData;
        private bool inTxn = false;
        // Implements IEditableObject
        void IEditableObject.BeginEdit() {
            Console.WriteLine("Start BeginEdit");
            if (!inTxn) {
                this.backupData = custData;
                inTxn = true;
                Console.WriteLine("BeginEdit - " + this.backupData.lastName);
            }
            Console.WriteLine("End BeginEdit");
        }
        void IEditableObject.CancelEdit() {
            Console.WriteLine("Start CancelEdit");
            if (inTxn) {
                this.custData = backupData;
                inTxn = false;
                Console.WriteLine("CancelEdit - " + this.custData.lastName);
            }
            Console.WriteLine("End CancelEdit");
        }
        void IEditableObject.EndEdit() {
            Console.WriteLine("Start EndEdit" + this.custData.id + this.custData.lastName);
            if (inTxn) {
                backupData = new CustomerData();
                inTxn = false;
                Console.WriteLine("Done EndEdit - " + this.custData.id + this.custData.lastName);
            }
            Console.WriteLine("End EndEdit");
        }
        public Customer(string ID) : base() {
            this.custData = new CustomerData();
            this.custData.id = ID;
            this.custData.firstName = "";
            this.custData.lastName = "";
        }
        public string ID {
            get {
                return this.custData.id;
            }
        }
        public string FirstName {
            get {
                return this.custData.firstName;
            }
            set {
                this.custData.firstName = value;
            }
        }
        public string LastName {
            get {
                return this.custData.lastName;
            }
            set {
                this.custData.lastName = value;
            }
        }
        internal CustomersList Parent {
            get {
                return parent;
            }
            set {
                parent = value;
            }
        }
        private void OnCustomerChanged() {
            if (!inTxn && Parent != null) {
                Parent.CustomerChanged(this);
            }
        }
    }
}