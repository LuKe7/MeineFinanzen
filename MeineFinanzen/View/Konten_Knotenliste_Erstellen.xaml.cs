// 20.01.2018 -Konten_Knotenliste_Erstellen-
// 05.01.2018 List PortFol.
using System;
using System.Windows;
using System.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using DataSetAdminNS;
using MeineFinanzen.Model;
using System.Collections.Generic;
namespace MeineFinanzen.View {
    public partial class Konten_Knotenliste_Erstellen : Window {
        public List<PortFol> liPortFol = new List<PortFol>();
        bool cbKeinSharpe_IsChecked = false;
        int nInteractive = 0;
        GetFromXpath getxp;
        public CollWertpapSynchro _wertpapsynchro = null;
        HauptFenster _mw;
        DataGridRow dgRow1 = null;
        string _ColHeader = "";             // durch dgvUrls_PreviewMouseDown() gesetzt.
        bool boDgvRowAusgewählt = false;
        //bool _navigiert = false;
        float _kurs = 0;
        float _aend = 0;
        float _sharpe = 0;
        DateTime _zeit = new DateTime();

        string _xpkurs = "";
        string _xpzeit = "";
        string _xpaend = "";
        string _xpsharp = "";

        string _curName = null;
        string _curUrl = null;
        string _curIsin = null;
        string _curUrlSharpe = null;

        public PortFol _foundRow = new PortFol();
        int posx = -1;      //  e.ClientMousePosition.X;
        int posy = -1;
        System.Windows.Forms.HtmlElement _elem1 = null;
        DataTable dtPortFol = new DataTable();
        PortFol pofo = new PortFol();
        public Konten_Knotenliste_Erstellen() { }
        public Konten_Knotenliste_Erstellen(HauptFenster mw) {
            conWrLi("---- -91- Konten_Knotenliste_Erstellen");
            _mw = mw;
            _wertpapsynchro = (CollWertpapSynchro)mw.Resources["wertpapsynchro"];
            InitializeComponent();
            getxp = new GetFromXpath();
            wb1.ScriptErrorsSuppressed = true;
            wb1.ScrollBarsEnabled = true;
            }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            conWrLi("---- -92- Konten_Knotenliste_Erstellen Window_Loaded");
            dtPortFol = DataSetAdmin.dsHier.Tables["tblPortFol"];       // dtPortFol geholt.
            liPortFol = dtPortFol.ToCollection<PortFol>();              // liPortFol erstellen.
            allesReset();       // Mit neu laden.
            }
        private void NavigiereZu(string address) {
            if (string.IsNullOrEmpty(address))
                return;
            if (address.Equals("about:blank"))
                return;
            if (!address.StartsWith("http://") && !address.StartsWith("https://"))
                address = "http://" + address;
            //addTextStr("NavigiereZu: " + address);
            //wb1.Document.Click -= new System.Windows.Forms.HtmlElementEventHandler(wb1_Document_Click);
            wb1.Navigate(new Uri(address));
            nInteractive = 0;
            //_navigiert = true;
            }
        private void CloseWindow(object sender, System.ComponentModel.CancelEventArgs e) {
            }
        private void wb1_DocumentTitleChanged(object sender, EventArgs e) {
            Title = wb1.DocumentTitle;
            addTextStr("wb1_DocumentTitleChanged: " + wb1.DocumentTitle);
            }
        private void wb1_Document_Click(Object sender, System.Windows.Forms.HtmlElementEventArgs e) {
            if (e.ClientMousePosition.IsEmpty) {
                _elem1 = null;
                posx = -1;
                posy = -1;
                }
            else {
                posx = e.ClientMousePosition.X;
                posy = e.ClientMousePosition.Y;
                _elem1 = wb1.Document.GetElementFromPoint(e.ClientMousePosition); // Ruft das an den angegebenen Clientkoordinaten befindliche HTML-Element ab.                            
                if (_elem1 != null)
                    addTextStr("wb1_Document_Click: " + _elem1.InnerText);
                else
                    addTextStr("wb1_Document_Click: null");
                }
            }
        /* //Console.WriteLine("_elem1.InnerText: {0}", _elem1.InnerText);
            //string htmlstr = wb1.DocumentText;
            //System.Windows.Forms.HtmlElement _elem7 = wb1.Document.GetElementById("q_price");
            //Console.WriteLine("_elem7.InnerText: {0}", _elem7.InnerText);
            //      Regex regex = new Regex(@" id=", RegexOptions.IgnoreCase);
            //      MatchCollection matchIds = regex.Matches(htmlstr);                
            //for (int ctr = 0; ctr < matchIds.Count; ctr++)
            //    Console.WriteLine("{0}. {1}", ctr, matchIds[ctr].Value);                               
            IHTMLDocument2 HTMLDocument = (IHTMLDocument2)webBrowser1.Document.DomDocument;            
            IHTMLElementCollection all = HTMLDocument.all;
            txtBox.Text = "leer";
            foreach (HtmlElement elem in webBrowser1.Document.All)
            {
                 txtBox.Text += elem.Name + Environment.NewLine;
            }

            foreach (IHTMLAnchorElement el in all)
            {
                txtBox.Text += el.name + Environment.NewLine;
               Console.WriteLine("{0}", el.name);
            }   
            //foreach (System.Windows.Forms.HtmlElement pageElement in wb1.Document.All) {
            //    if (pageElement.Name.Length > 0)
            //        Console.WriteLine("{0}", pageElement.Name);             */
        private void wb1_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e) {
            if (wb1.ReadyState == System.Windows.Forms.WebBrowserReadyState.Complete) {
                addTextStr("wb1-DocumentCompleted: " + wb1.ReadyState);
                }
            else if (wb1.ReadyState == System.Windows.Forms.WebBrowserReadyState.Interactive) {
                nInteractive++;
                addTextStr("wb1-DocumentCompleted: " + nInteractive + " " + wb1.ReadyState);
                }
            else if (wb1.ReadyState == System.Windows.Forms.WebBrowserReadyState.Loading) {
                addTextStr("wb1-DocumentCompleted: " + wb1.ReadyState);
                }
            else {
                addTextStr("wb1-DocumentCompleted: " + wb1.ReadyState + " unbekannt!!!");
                }

            if (wb1.Document != null)
                addTextStr("wb1_DocumentCompleted: " + wb1.Document.Title);
            else
                addTextStr("wb1_DocumentCompleted");
            wb1.Document.Click += new System.Windows.Forms.HtmlElementEventHandler(wb1_Document_Click);
            }
        private void wb1_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e) { }
        private void btOk_Click(object sender, RoutedEventArgs e) {
            if (!boDgvRowAusgewählt)
                return;
            if (_foundRow == null)
                return;
            if (cbKeinSharpe.IsChecked == true) {
                _foundRow.WPUrlSharpe = "";
                _foundRow.WPSharpe = 0;
                }
            addTextStr("btOk() _foundRow[\"WPKurs\"]: " + _foundRow.WPKurs.ToString());
            allesReset();
            wb1.Document.Click -= new System.Windows.Forms.HtmlElementEventHandler(wb1_Document_Click);
            }
        private void btReset_Click(object sender, RoutedEventArgs e) {
            allesReset();
            }
        private void allesReset() {
            _foundRow = null;
            _wertpapsynchro = (CollWertpapSynchro)Resources["wertpapsynchro"];
            if (dgvUrls.Items.Count > 1)
                _wertpapsynchro.Clear();
            dgvUrls.UpdateLayout();
            foreach (PortFol wp in liPortFol) {
                if (wp.WPISIN.Length != 12)
                    continue;
                PortFol row = liPortFol.Find(x => x.WPISIN.Contains(wp.WPISIN));
                _wertpapsynchro.Add(new WertpapSynchro {
                    WPSAnzahl = wp.WPAnzahl,
                    WPSName = wp.WPName,
                    WPSKursZeit = wp.WPStand,
                    WPSISIN = wp.WPISIN,

                    WPSURL = wp.WPUrlText,
                    WPSKurs = wp.WPKurs,
                    WPSProzentAenderung = wp.WPProzentAenderung,
                    WPSType = wp.WPTypeID,
                    WPSSharpe = wp.WPSharpe,

                    WPURLSharp = wp.WPUrlSharpe,
                    WPXPathKurs = wp.WPXPathKurs,
                    WPXPathAend = wp.WPXPathAend,
                    WPXPathZeit = wp.WPXPathZeit,
                    WPXPathSharp = wp.WPXPathSharp
                    });
                }
            dgvUrls.ItemsSource = _wertpapsynchro;
            dgvUrls.UpdateLayout();
            _elem1 = null;
            boDgvRowAusgewählt = false;
            txtUrl.Text = "";
            txtKurs.Text = "";
            txtKursZeit.Text = "";
            txtÄnderung.Text = "";
            txtUrlSharpe.Text = "";
            txtSharpe.Text = "";
            cbUrl.IsChecked = false;
            cbKurs.IsChecked = false;
            cbKursZeit.IsChecked = false;
            cbProzentAenderung.IsChecked = false;
            cbUrlSharpe.IsChecked = false;
            cbKeinSharpe.IsChecked = false;
            cbSharpe.IsChecked = false;
            NavigiereZu("https://www.google.de/");
            }
        private void btGoHome_Click(object sender, RoutedEventArgs e) {
            //allesReset();
            wb1.Navigate(new Uri("https://www.google.de/"));
            addTextStr("btBrowserGoHome_Click https://www.google.de/");
            }
        private void btURLneu_Click(object sender, RoutedEventArgs e) {
            URL_Neu();
            }
        private void btBeenden_Click(object sender, RoutedEventArgs e) {
            // liPortFol-Daten nach DataSetAdmin.dsHier.Tables["tblPortFol"];
            dtPortFol = DataSetAdmin.dsHier.Tables["tblPortFol"];       // nochmal rausziehen, da evtl. geändert.
            string strISIN = "";
            foreach (DataRow dtrow in dtPortFol.Rows) {
                strISIN = (string)dtrow["WPISIN"];
                if (strISIN.Length != 12)
                    continue;
                PortFol lirow = liPortFol.Find(x => x.WPISIN.Contains(strISIN));
                if (lirow.WPStand != (DateTime)dtrow["WPStand"]) {
                    Console.WriteLine("++++ Stand alt: {0,-12} meu: {1,-12}", (DateTime)dtrow["WPStand"], lirow.WPStand);
                    dtrow["WPStand"] = lirow.WPStand;
                    }
                if (lirow.WPUrlText != (string)dtrow["WPUrlText"])
                    dtrow["WPUrlText"] = lirow.WPUrlText;

                if (lirow.WPSharpe != (float)dtrow["WPSharpe"]) {
                    Console.WriteLine("++++ Sharpe alt: {0,-12} meu: {1,-12}", (float)dtrow["WPSharpe"], lirow.WPSharpe);
                    dtrow["WPSharpe"] = lirow.WPSharpe;
                    }
                if (lirow.WPXPathSharp != (string)dtrow["WPXPathSharp"])
                    dtrow["WPXPathSharp"] = lirow.WPXPathSharp;

                if (lirow.WPUrlSharpe != (string)dtrow["WPUrlSharpe"])
                    dtrow["WPUrlSharpe"] = lirow.WPUrlSharpe;

                if (lirow.WPXPathAend != (string)dtrow["WPXPathAend"])
                    dtrow["WPXPathAend"] = lirow.WPXPathAend;

                if (lirow.WPProzentAenderung != (float)dtrow["WPProzentAenderung"])
                    dtrow["WPProzentAenderung"] = lirow.WPProzentAenderung;

                if (lirow.WPKurs != (float)dtrow["WPKurs"]) {
                    Console.WriteLine("++++ Kurs alt: {0,-12} meu: {1,-12}", (float)dtrow["WPKurs"], lirow.WPKurs);
                    dtrow["WPKurs"] = lirow.WPKurs;
                    }
                if (lirow.WPXPathKurs != (string)dtrow["WPXPathKurs"])
                    dtrow["WPXPathKurs"] = lirow.WPXPathKurs;

                if (dtrow["WPXKursX"] != dtrow["WPXPathKurs"])
                    dtrow["WPXKursX"] = lirow.WPXKursX;

                if (lirow.WPXKursY != (int)dtrow["WPXKursY"])
                    dtrow["WPXKursY"] = lirow.WPXKursY;

                if (lirow.WPXPathZeit != (string)dtrow["WPXPathZeit"])
                    dtrow["WPXPathZeit"] = lirow.WPXPathZeit;

                if (lirow.WPXZeitX != (int)dtrow["WPXZeitX"])
                    dtrow["WPXZeitX"] = lirow.WPXZeitX;

                if (lirow.WPXZeitY != (int)dtrow["WPXZeitY"])
                    dtrow["WPXZeitY"] = lirow.WPXZeitY;
                }
            DataSetAdmin.dtPortFol = dtPortFol;
            DataSetAdmin.DatasetSichernInXml("MeineFinanzen");
            Close();
            }
        private void dgvUrls_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            //conWrLi("---- -93- dgvUrls_PreviewMouseDown");
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return;
            DataGridCell cell1 = dep as DataGridCell;
            dgRow1 = dep as DataGridRow;
            while ((dep != null) && !(dep is DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            dgRow1 = dep as DataGridRow;
            if (dgRow1 == null)
                return;
            //int nro = FindRowIndex(dgRow1);
            DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(dgRow1) as DataGrid;
            var item = dataGrid.ItemContainerGenerator.ItemFromContainer(dgRow1);
            //Console.WriteLine("cell1.Column.Header: {0}", cell1.Column.Header); // ist z.B. WPURLSharp
            _ColHeader = cell1.Column.Header.ToString();
            boDgvRowAusgewählt = true;
            //conWrLi("---- -94- boDgvRowAusgewählt=true");
            _curName = ((WertpapSynchro)item).WPSName;
            _curIsin = ((WertpapSynchro)item).WPSISIN;
            _curUrl = ((WertpapSynchro)item).WPSURL;
            _curUrlSharpe = ((WertpapSynchro)item).WPURLSharp;
            txtUrl.Text = _curUrl;
            if (e.RightButton == MouseButtonState.Pressed) {
                // NOCH GridKlick gk = new View.GridKlick(this, _curIsin, nro, nwp);
                // gk.ShowDialog();
                }
            else if (e.LeftButton == MouseButtonState.Pressed) {
                txtKurs.Text = ((WertpapSynchro)item).WPSKurs.ToString();
                }
            txtKursZeit.Text = ((WertpapSynchro)item).WPSKursZeit.ToString();
            txtÄnderung.Text = ((WertpapSynchro)item).WPSProzentAenderung.ToString();
            txtUrlSharpe.Text = ((WertpapSynchro)item).WPURLSharp;
            txtSharpe.Text = ((WertpapSynchro)item).WPSSharpe.ToString();
            SetzefoundRow();
            if (e.LeftButton == MouseButtonState.Pressed) { }
            NavigiereZu(_curUrl);
            }
        private void cbUrlLaden_Click(object sender, RoutedEventArgs e) {
            NavigiereZu(_curUrl);
            }
        private void cbUrl_Click(object sender, RoutedEventArgs e) {
            URL_Neu();
            }
        private void cbKurs_Click(object sender, RoutedEventArgs e) {
            cbKurs.IsChecked = false;
            if (!boDgvRowAusgewählt || _elem1 == null)
                return;
            try {
                cbKurs.IsChecked = true;
                var savedId = _elem1.Id;                       // element.id sichern nach saveid
                System.Windows.Forms.HtmlElement element = _elem1;
                element.Id = Guid.NewGuid().ToString();         // setze neue element.id  
                var uniqueId = element.Id;                      // neue id sichern                
                var doc = new HtmlAgilityPack.HtmlDocument();   // doc initialisieren
                doc.LoadHtml(wb1.Document.GetElementsByTagName("body")[0].OuterHtml); // doc laden
                element.Id = savedId;                           // element.id zurücksetzen
                var node = doc.GetElementbyId(uniqueId);        // node über neue element.id holen
                var xpath = node.XPath;
                _xpkurs = xpath;
                // Mit XPath können Sie einen bestimmten einzelnen Knoten oder alle Knoten suchen, die bestimmte Kriterien erfüllen. 
                _kurs = getxp.GetPriceFromXpath(xpath, wb1, new Uri(_curUrl), _foundRow);
                _curUrl = wb1.Url.ToString();
                txtUrl.Text = _curUrl;
                txtKurs.Text = _elem1.InnerText;
                _foundRow.WPUrlText = _curUrl;
                _foundRow.WPXPathKurs = _xpkurs;
                _foundRow.WPKurs = _kurs;

                _foundRow.WPXKursX = posx;
                _foundRow.WPXKursY = posy;

                ((WertpapSynchro)(dgRow1.Item)).WPSURL = _curUrl;
                ((WertpapSynchro)(dgRow1.Item)).WPXPathKurs = _xpkurs;
                ((WertpapSynchro)(dgRow1.Item)).WPSKurs = _kurs;
                addTextStr("cbKurs neu: " + _kurs.ToString());
                }
            catch (Exception ex) {
                MessageBox.Show("Fehler in cbKurs_Click(): " + ex);
                }
            }
        private void cbKursZeit_Click(object sender, RoutedEventArgs e) {
            cbKursZeit.IsChecked = false;
            if (!boDgvRowAusgewählt || _elem1 == null)
                return;
            cbKursZeit.IsChecked = true;
            var savedId = _elem1.Id;                       // element.id sichern nach saveid
            System.Windows.Forms.HtmlElement element = _elem1;
            element.Id = Guid.NewGuid().ToString();         // setze neue element.id  
            var uniqueId = element.Id;                      // neue id sichern                
            var doc = new HtmlAgilityPack.HtmlDocument();   // doc initialisieren
            doc.LoadHtml(wb1.Document.GetElementsByTagName("body")[0].OuterHtml); // doc laden
            element.Id = savedId;                           // element.id zurücksetzen
            var node = doc.GetElementbyId(uniqueId);        // node über neue element.id holen
            var xpath = node.XPath;
            _xpzeit = xpath;
            // Mit XPath können Sie einen bestimmten einzelnen Knoten oder alle Knoten suchen, die bestimmte Kriterien erfüllen.             
            _curUrl = wb1.Url.ToString();
            _zeit = getxp.GetZeitFromXpath(xpath, wb1, new Uri(_curUrl), _foundRow);
            txtKursZeit.Text = _elem1.InnerText;

            _foundRow.WPUrlText = _curUrl;
            _foundRow.WPXPathZeit = _xpzeit;
            _foundRow.WPStand = _zeit;

            _foundRow.WPXZeitX = posx;
            _foundRow.WPXZeitY = posy;

            ((WertpapSynchro)(dgRow1.Item)).WPSURL = _curUrl;
            ((WertpapSynchro)(dgRow1.Item)).WPXPathZeit = _xpzeit;
            ((WertpapSynchro)(dgRow1.Item)).WPSKursZeit = _zeit;
            }
        private void cbProzentAenderung_Click(object sender, RoutedEventArgs e) {
            cbProzentAenderung.IsChecked = false;
            if (!boDgvRowAusgewählt || _elem1 == null)
                return;
            cbProzentAenderung.IsChecked = true;
            txtÄnderung.Text = _elem1.InnerText;
            var savedId = _elem1.Id;
            System.Windows.Forms.HtmlElement element = _elem1;
            element.Id = Guid.NewGuid().ToString();
            var uniqueId = element.Id;
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(wb1.Document.GetElementsByTagName("body")[0].OuterHtml);
            element.Id = savedId;
            var node = doc.GetElementbyId(uniqueId);
            var xpath = node.XPath;
            _xpaend = xpath;
            _aend = getxp.GetAendFromXpath(xpath, wb1, new Uri(_curUrl), _foundRow);
            _curUrl = wb1.Url.ToString();
            txtUrl.Text = _curUrl;
            txtÄnderung.Text = _aend.ToString();
            _foundRow.WPUrlText = _curUrl;
            _foundRow.WPXPathAend = _xpaend;
            _foundRow.WPProzentAenderung = _aend;
            ((WertpapSynchro)(dgRow1.Item)).WPSURL = _curUrl;
            ((WertpapSynchro)(dgRow1.Item)).WPXPathAend = _xpaend;
            ((WertpapSynchro)(dgRow1.Item)).WPSProzentAenderung = _aend;
            addTextStr("cbProzentAenderung neu: " + _aend.ToString());
            }
        private void cbUrlSharpeLaden_Click(object sender, RoutedEventArgs e) {
            NavigiereZu(_curUrlSharpe);
            }
        private void cbUrlSharpe_Click(object sender, RoutedEventArgs e) {
            cbUrlSharpe.IsChecked = false;
            if (!boDgvRowAusgewählt)
                return;
            cbUrlSharpe.IsChecked = true;
            PortFol foundRow = liPortFol.Find(x => x.WPISIN.Contains(_curIsin));
            if (foundRow == null)
                return;
            txtUrlSharpe.Text = wb1.Url.ToString();
            _curUrlSharpe = txtUrlSharpe.Text.Trim();
            foundRow.WPUrlSharpe = _curUrlSharpe;
            }
        private void cbSharpe_Click(object sender, RoutedEventArgs e) {
            cbSharpe.IsChecked = false;
            if (!boDgvRowAusgewählt || _elem1 == null)
                return;
            cbSharpe.IsChecked = true;
            var savedId = _elem1.Id;
            System.Windows.Forms.HtmlElement element = _elem1;
            element.Id = Guid.NewGuid().ToString();
            var uniqueId = element.Id;
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(wb1.Document.GetElementsByTagName("body")[0].OuterHtml);
            element.Id = savedId;
            var node = doc.GetElementbyId(uniqueId);
            var xpath = node.XPath;
            _xpsharp = xpath;
            _sharpe = getxp.GetSharpFromXpath(xpath, wb1, new Uri(_curUrl), _foundRow);
            _curUrl = wb1.Url.ToString();
            txtUrlSharpe.Text = _curUrl;
            txtSharpe.Text = _sharpe.ToString();
            _foundRow.WPSharpe = _sharpe;
            _foundRow.WPXPathSharp = _xpsharp;
            _foundRow.WPUrlSharpe = _curUrl;                            // neu
            ((WertpapSynchro)(dgRow1.Item)).WPURLSharp = _curUrl;       // neu
            ((WertpapSynchro)(dgRow1.Item)).WPXPathSharp = _xpsharp;
            ((WertpapSynchro)(dgRow1.Item)).WPSSharpe = _sharpe;            
            }
        private void cbKeinSharpe_Click(object sender, RoutedEventArgs e) {
            if (cbKeinSharpe_IsChecked == true) {
                cbKeinSharpe.IsChecked = false;
                }
            else {
                _foundRow.WPUrlSharpe = "";
                _foundRow.WPSharpe = 0;
                cbKeinSharpe_IsChecked = true;
                }
            }
        private void URL_Neu() {
            cbUrl.IsChecked = true;
            _curUrl = wb1.Url.ToString();
            txtUrl.Text = _curUrl;
            PortFol foundRow = liPortFol.Find(x => x.WPISIN.Contains(_curIsin));
            if (foundRow == null)
                return;
            foundRow.WPUrlText = _curUrl;
            }
        public void conWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
            }
        private int FindRowIndex(DataGridRow row) {
            DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(row) as DataGrid;
            int index = dataGrid.ItemContainerGenerator.IndexFromContainer(row);
            return index;
            }
        private void SetzefoundRow() {
            _foundRow = liPortFol.Find(x => x.WPISIN.Contains(_curIsin));
            }
        public DataGridCell GetCell(int row, int column) {
            DataGridRow rowContainer = GetRow(row);
            if (rowContainer != null) {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null) {
                    dgvUrls.ScrollIntoView(rowContainer, dgvUrls.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                    }
                return cell;
                }
            return null;
            }
        public DataGridRow GetRow(int index) {
            DataGridRow row = (DataGridRow)dgvUrls.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null) {
                dgvUrls.UpdateLayout();
                dgvUrls.ScrollIntoView(dgvUrls.Items[index]);
                row = (DataGridRow)dgvUrls.ItemContainerGenerator.ContainerFromIndex(index);
                }
            return row;
            }
        public static T GetVisualChild<T>(Visual parent) where T : Visual {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++) {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) {
                    child = GetVisualChild<T>(v);
                    }
                if (child != null) {
                    break;
                    }
                }
            return child;
            }
        private void addTextStr(string str) {
            if (str.Length > 0)
                txtBox.AppendText(Environment.NewLine + str);
            txtBox.ScrollToEnd();
            txtBox.InvalidateVisual();
            }
        }
    public class Person : INotifyPropertyChanged {
        // Ereignis
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
            }

        // Eigenschaften
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
    }