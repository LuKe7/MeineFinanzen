// 23.10.2018   -Model-  CollWertpapSynchro.cs 
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            if (obj is WertpapSynchro wp)
                return wp.WPSISIN.CompareTo(((WertpapSynchro)obj).WPSISIN);
            throw new NotImplementedException();
        }
    }
    public enum Wertpapierklasse {
        GeldKto = 10,
        MinWertpap = 20,
        MischFonds_Welt = 22,
        AktienFonds_SchwellenL = 33,
        AktienFonds_LateinA = 34,
        Aktien_ETF_Europa   = 41,
        Aktien_fonds_Europa = 51,
        Aktien_fonds_Asien  = 52,
        Aktien_fonds_Welt   = 53,
        Anleih_RentFon_Eur  = 54,
        Anleih_RentFon_Int  = 55,
        ImmobFonds_Offen    = 56,
        Geldmarktfond       = 57,
        Zertifikat_auf_Aktien = 63,
        Anleihe = 70,
        MaxWertpap = 79,       
        GeschlFond = 80
    }
    public class WertpapSynchro : INotifyPropertyChanged, IEditableObject {
        public float WPSAnzahl { get; set; }
        public string WPSName { get; set; }
        public string WPSURL { get; set; }
        public DateTime WPSKursZeit { get; set; }
        public string WPSISIN { get; set; }
        private Single _WPSKurs;
        public Single WPSKurs {
            get { return _WPSKurs; }
            set {
                _WPSKurs = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WPSKurs"));
            }
        }
        public Single WPSProzentAenderung { get; set; }
        public Wertpapierklasse WPSType { get; set; }
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
            PropertyChanged?.Invoke(this, e);
        }
        private void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
    }
}