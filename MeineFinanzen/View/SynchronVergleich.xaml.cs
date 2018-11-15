﻿// 13.11.2018   -View-  SynchronVergleich.cs 
// Vergleich der Synchronisationsdaten von:
//  1. Subsembly
//  2. HBCI4j
//  3. Internet (Scraping)
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using MeineFinanzen.Helpers;
using MeineFinanzen.Model;
namespace MeineFinanzen.View {
    public partial class SynchronVergleich : Window {
        public SynchroVs _synchrovs = null;
        XmlSerializer ser = new XmlSerializer(typeof(CollWertpapiere));
        private delegate void EmptyDelegate();
        public ICollectionView View;
        private bool _isGroup = true;
        private bool NurPreisDifferenzen = false;
        public SynchronVergleich() {
            InitializeComponent();
            DataContext = this;
            _synchrovs = (SynchroVs)Resources["MyDataSource"];  // new SynchroVs();         
            _synchrovs.Changed += SynchroVergleichChangedHandler;
            ConWrLi("start -Statustext-           ");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            txtUnten.Clear();
            txtUnten.FontSize = 10;
            txtUnten.FontFamily = new FontFamily("Courier New, Verdana");
            PrintTxtUnten("Start -SynchronVergleich-");
            HoleDatenGesamt(sender);
        }
        private void HoleDatenGesamt(object sender) {
            _synchrovs.Clear();
            HoleDaten("PortFol_", "1", "Subsembly");
            HoleDaten("PortFolHBCI_", "2", "HBCI4j");
            HoleDaten("PortFolInt_", "3", "Internet");
            _synchrovs.Sort(x => x.WPVSort, false);
            SynchroV sv1 = null;
            SynchroV sv2 = null;
            SynchroV sv3 = null;
            int n = 0;
            int nn = 0;
            foreach (SynchroV sv in _synchrovs) {
                if (++n == 1) {
                    sv1 = sv;
                    sv1.WPVRowColor = n.ToString();
                } else if (n == 2) {
                    sv2 = sv;
                    sv2.WPVRowColor = n.ToString();
                } else if (n == 3) {
                    sv3 = sv;
                    sv3.WPVRowColor = n.ToString();
                    if (NurPreisDifferenzen)
                        sv1.WPVAnzeigen = sv2.WPVAnzeigen = sv3.WPVAnzeigen = false;
                    else
                        sv1.WPVAnzeigen = sv2.WPVAnzeigen = sv3.WPVAnzeigen = true;
                    float p1 = (sv1.WPVKurs - sv2.WPVKurs) * 100 / sv2.WPVKurs;
                    float p2 = (sv2.WPVKurs - sv3.WPVKurs) * 100 / sv3.WPVKurs;
                    float p3 = (sv1.WPVKurs - sv3.WPVKurs) * 100 / sv3.WPVKurs;
                    if (p1 > 2 || p1 < -2) {
                        sv1.WPVAnzeigen = sv2.WPVAnzeigen = sv3.WPVAnzeigen = true;
                        sv1.WPVForegroundColor = sv2.WPVForegroundColor = sv3.WPVForegroundColor = Brushes.Red;
                    } else if (p3 > 2 || p3 < -2) {
                        sv1.WPVAnzeigen = sv2.WPVAnzeigen = sv3.WPVAnzeigen = true;
                        sv1.WPVForegroundColor = sv2.WPVForegroundColor = sv3.WPVForegroundColor = Brushes.Magenta;
                    } else if (p2 > 2 || p2 < -2) {
                        sv1.WPVAnzeigen = sv2.WPVAnzeigen = sv3.WPVAnzeigen = true;
                        sv1.WPVForegroundColor = sv2.WPVForegroundColor = sv3.WPVForegroundColor = Brushes.Orange;
                    } else if (p1 == 0) {
                    } else if (p2 == 0) {
                    } else if (p3 == 0) {
                    }                   
                    string str = sv1.WPVName;
                    if (str.Length > 30)
                        str = str.Substring(0, 30);
                    //Console.WriteLine("{0,-30} Sort:{1} Kurs1:{2,7} Kurs2:{3,7} Kurs3:{4,7} Color:{5} nn:{6}",
                    //  str, sv1.WPVSort, sv1.WPVKurs, sv2.WPVKurs, sv3.WPVKurs, sv1.WPVRowColor, nn);
                    n = 0;
                    nn++;
                }
            }
            ICollectionView cvVergl = CollectionViewSource.GetDefaultView(dgSynchroVergleich.ItemsSource);
            //cvVergl.Filter = new Predicate<SynchroV>(Contains);

            //cvVergl.Filter += new FilterEventHandler(CollectionViewSourceSynchro_Filter);

            dgSynchroVergleich.ItemsSource = _synchrovs;
            UngroupButton_Click(sender, new RoutedEventArgs());
            dgSynchroVergleich.Items.Refresh();
        }
        private void HoleDaten(string strFile, string strSort, string strWomit) {
            CollWertpapiere cwp = Wertpapiere_ReadXml(strFile, out DateTime dt);
            foreach (Wertpapier wp in cwp) {
                if (wp.isSumme)
                    continue;
                Wertpapierklasse typeid = (Wertpapierklasse)wp.Type;
                if (typeid < Wertpapierklasse.MinWertpap || typeid > Wertpapierklasse.MaxWertpap)
                    continue;
                if (wp.ISIN.ToString().Length != 12)
                    continue;
                // Console.WriteLine("{0,-12} {1,-30} {2}", wp.ISIN, wp.Name, wp.Type);
                _synchrovs.Add(new SynchroV {
                    WPVAnzahl = wp.Anzahl,
                    WPVName = wp.Name,
                    WPVKursZeit = wp.KursZeit,
                    WPVAktWert = wp.AktWert,
                    WPVISIN = wp.ISIN,
                    WPVURL = wp.URL,
                    WPVKurs = (float)wp.AktKurs,
                    WPVProzentAenderung = wp.Heute,
                    WPVType = (Wertpapierklasse)wp.Type,
                    WPVSharpe = wp.Sharpe,
                    WPVBemerkung = strWomit + " " + strFile + " " + dt,
                    WPVSort = wp.ISIN + " " + strSort,
                    WPVRowColor = "0",
                    WPVAnzeigen = NurPreisDifferenzen ^ true,
                    WPVForegroundColor = Brushes.AliceBlue
                });
            }
        }
        public CollWertpapiere Wertpapiere_ReadXml(string strPortFol, out DateTime dt) {
            string s = Convert.ToString(DateTime.Now).Trim();
            string filename = GlobalRef.g_Ein.myDepotPfad + @"\KursDaten";
            DirectoryInfo ParentDirectory2 = new DirectoryInfo(filename);
            dt = DateTime.Now.AddDays(1);
            while (!File.Exists(filename)) {
                dt = dt.AddDays(-1);
                s = Convert.ToString(dt).Trim();
                filename = GlobalRef.g_Ein.myDepotPfad + @"\KursDaten\" + strPortFol
                    + s.Substring(6, 4) + s.Substring(3, 2) + s.Substring(0, 2) + ".xml";
                // In ...PortFol_JJJJMMTT.xml stehen Daten mit Subsembly geholt.
            }
            //Console.WriteLine("---- Wertpapiere_ReadXml " + filename);
            CollWertpapiere cwp = null;
            using (StreamReader _reader = new StreamReader(filename)) {
                cwp = (CollWertpapiere)ser.Deserialize(_reader);
            }
            return cwp;
        }
        private void Window_Closing(object sender, CancelEventArgs e) { }
        private void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
        }
        private void PrintTxtUnten(string str) {
            DoEvents();
            txtUnten.AppendText(Environment.NewLine + str);
            txtUnten.ScrollToEnd();
            txtUnten.InvalidateVisual();
            DoEvents();
        }
        protected void DoEvents() {
            //  if (System.Windows.Application.Current != null)
            //      System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new EmptyDelegate(delegate { }));
            //* Diese Funktion uebernimmt die Unterbrechnung zur Anzeige und Eventbearbeitung in C#, WPF beim langen Loop Berechnungen
            //* mit einer Dispatcher
            //* EmptyDelegate im Header definieren
            //* using System.Windows.Threading; im Header festlegen   
        }
        private void GroupButton_Click(object sender, RoutedEventArgs e) {
            //e.Handled = true;
            ICollectionView cvVergl = CollectionViewSource.GetDefaultView(dgSynchroVergleich.ItemsSource);
            if (cvVergl != null && cvVergl.CanGroup == true) {
                _isGroup = true;
                cvVergl.GroupDescriptions.Clear();
                cvVergl.GroupDescriptions.Add(new PropertyGroupDescription("WPVName"));
            }
        }
        private void UngroupButton_Click(object sender, RoutedEventArgs e) {
            ICollectionView cvVergl = CollectionViewSource.GetDefaultView(dgSynchroVergleich.ItemsSource);
            if (cvVergl != null && cvVergl.CanGroup == true) {
                _isGroup = false;
                cvVergl.GroupDescriptions.Clear();
            }
        }
        private void CollectionViewSourceSynchro_Filter(object sender, FilterEventArgs e) {
            /* So filtern Sie Elemente in einem DataGrid.
         * Fügen Sie einen Handler für das CollectionViewSource.Filter-Ereignis hinzu.
         * Definieren Sie im Filter-Handler die Filterlogik.
         * Der Filter wird jedes Mal angewendet, wenn die Ansicht aktualisiert wird.
         * Alternativ können Sie Elemente in einem DataGrid filtern, indem Sie eine Methode erstellen,
         * die die Filterlogik bereitstellt, und die CollectionView.Filter-Eigenschaft zum Anwenden des Filters festlegen.
         * Ein Beispiel für diese Methode finden Sie unter Gewusst wie: Filtern von Daten in einer Ansicht. */
            //if (t.BisDatum.Year == 1980)

            if (e.Item is SynchroV t)
            // If filter is turned on, filter ISINd items.
            {
                if (t.WPVKurs == 380.52)
                    e.Accepted = false;
                else
                    e.Accepted = true;
            }
        }
        private void GridSynchroVergl_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e) {
            //Detail.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));   
        }
        private void GridSynchroVergl_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return;
            DataGridCell cell1 = dep as DataGridCell;
            DataGridRow row1 = dep as DataGridRow;
            while ((dep != null) && !(dep is DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            row1 = dep as DataGridRow;
            string isi = null;
            string nam = null;
            int nro = -1;
            if (row1 != null) {
                try {
                    isi = ((SynchroV)(row1.Item)).WPVISIN;
                    nam = ((SynchroV)(row1.Item)).WPVName;
                } catch (Exception ex) {
                    MessageBox.Show("gridSynchronVergleich_PreviewMouseDown() Fehler: " + ex);
                    return;
                }
                nro = FindRowIndex(row1);
                if (e.LeftButton == MouseButtonState.Pressed) {
                    if (_isGroup) {
                        _isGroup = false;
                        return;
                    }
                    if (row1.DetailsVisibility == Visibility.Collapsed) {
                        row1.DetailsVisibility = Visibility.Visible;
                    } else {
                        row1.DetailsVisibility = Visibility.Collapsed;
                    }
                }
            }
        }
        private int FindRowIndex(DataGridRow row) {
            DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(row) as DataGrid;
            int index = dataGrid.ItemContainerGenerator.IndexFromContainer(row);
            return index;
        }
        private void BtPreisDifferenzen_Click(object sender, RoutedEventArgs e) {
            NurPreisDifferenzen = NurPreisDifferenzen ^ true;
            HoleDatenGesamt(sender);
        }
        private void Display(Collection<SynchroV> cs) {
            Console.WriteLine();
            foreach (SynchroV item in cs) {
                Console.WriteLine(item.WPVName);
            }
        }
        public void SynchroVergleichChangedHandler(object source, SynchroVergleichChangedEventArgs e) {
            if (e.ChangeType == ChangeType.Replaced) {
                //Console.WriteLine("{0} was replaced with {1}", e.ChangedItem.WPVName, e.ReplacedWith);
            } else if (e.ChangeType == ChangeType.Cleared) {
                //Console.WriteLine("The dinosaur list was cleared.");
            } else {
                //Console.WriteLine("{0} was {1}.", e.ChangedItem.WPVName, e.ChangeType);
            }
        }
        private void BtnAuslesen_Click(object sender, RoutedEventArgs e) {
            DataGridRow Row1;
            SynchroV sv1 = null;
            SynchroV sv2 = null;
            SynchroV sv3 = null;
            int n = 0;
            for (int nro = 0; nro < dgSynchroVergleich.Items.Count - 1; nro++) {
                Row1 = DataGridHelper.GetRow(dgSynchroVergleich, nro);
                string sort = ((SynchroV)Row1.Item).WPVRowColor;
                if (++n == 1) {
                    sv1 = ((SynchroV)Row1.Item);
           
                } else if (n == 2)
                    sv2 = ((SynchroV)Row1.Item);
                else if (n == 3) {
                    sv3 = ((SynchroV)Row1.Item);
                    /* if (sv1.WPVAnzeigen && sv2.WPVAnzeigen && sv3.WPVAnzeigen)
                        Console.WriteLine("Anzeigen:       Name: {0,-50} FC: {1,8} Sort: {2}", sv1.WPVName, sv1.WPVForegroundColor, sv1.WPVSort + sv2.WPVSort + sv3.WPVSort);
                    else
                        Console.WriteLine("Nicht Anzeigen: Name: {0,-50} FC: {1,8} Sort: {2}", sv1.WPVName, sv1.WPVForegroundColor, sv1.WPVSort + sv2.WPVSort + sv3.WPVSort); */
                    n = 0;
                }
            }
        }
    }
    public class CompleteConverterSynchroVergl : IValueConverter {
        // This converter changes the value of a Tasks Complete status from true/false to a string value of
        // "Complete"/"Active" for use in the row group header.
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool complete = (bool)value;
            if (complete)
                return "Complete";
            else
                return "Active";
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string strComplete = (string)value;
            if (strComplete == "Complete")
                return true;
            else
                return false;
        }
    }
}