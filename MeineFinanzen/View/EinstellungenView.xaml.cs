// 28.06.2018  EinstellungenView.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;
using System.ComponentModel;
namespace MeineFinanzen.View {
    public partial class EinstellungenView : Window {
        static XmlSerializer xmlserializer = new XmlSerializer(typeof(Model.Einstellungen));        
        bool isDataDirty = false;
        public EinstellungenView() {
            InitializeComponent();                                             
            Helpers.GlobalRef.g_Ein.DeSerializeReadEinstellungen(
                Helpers.GlobalRef.g_Ein.strEinstellungen, out Helpers.GlobalRef.g_Ein);
            this.DataContext = Helpers.GlobalRef.g_Ein;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            tabControl1.SelectedItem = tabNavigation;
            tabNavigation.Visibility = Visibility.Visible;
            btÜbernehmen.Foreground = Brushes.Gray; // Wenn Änderungen: .Foreground = Brushes.Black; .Background = Brushes.LightGray;                       
        }
        private void EinstellungenWindow_Closed(object sender, EventArgs e) {
            if (isDataDirty)
                Helpers.GlobalRef.g_Ein.SerializeWriteEinstellungen(
                    Helpers.GlobalRef.g_Ein.strEinstellungen, Helpers.GlobalRef.g_Ein);               
        }
        void EinstellungenWindow_Closing(object sender, CancelEventArgs e) {
            if (isDataDirty) {
                string msg = "Daten wurden geändert. Sichern vor dem Schließen?";
                MessageBoxResult result =
                  MessageBox.Show(msg, "Daten Einstellungen", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) {
                    isDataDirty = false;
                }
            }
        }     
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {  }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            TreeView tv = (TreeView)sender;
        }
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e) {
            TreeViewItem tvi = (TreeViewItem)sender;
            if (tvi.Header.ToString() == "Allgemein") {
                tabControl1.SelectedItem = tabAllgemein;
                tabAllgemein.Visibility = Visibility.Visible;
            }
            else if (tvi.Header.ToString() == "Navigation") {
                tabControl1.SelectedItem = tabNavigation;
                tabNavigation.Visibility = Visibility.Visible;
            }
            else if (tvi.Header.ToString() == "Bearbeiten") {
                tabControl1.SelectedItem = tabBearbeiten;
                tabBearbeiten.Visibility = Visibility.Visible;
            }
            else if (tvi.Header.ToString() == "Ansicht") {
                tabControl1.SelectedItem = tabAnsicht;
                tabAnsicht.Visibility = Visibility.Visible;
            }
            else if (tvi.Header.ToString() == "Finanzübersicht") {
                tabControl1.SelectedItem = tabFinanzübersicht;
                tabFinanzübersicht.Visibility = Visibility.Visible;
            }
            else if (tvi.Header.ToString() == "Wertpapiere") {
                tabControl1.SelectedItem = tabWertpapiere;
                tabWertpapiere.Visibility = Visibility.Visible;
            }
            else if (tvi.Header.ToString() == "Aktualisieren") {
                tabControl1.SelectedItem = tabAktualisieren;
                tabAktualisieren.Visibility = Visibility.Visible;
                tvi.ExpandSubtree();
            }
            else if (tvi.Header.ToString() == "Kurs") {
                tabControl1.SelectedItem = tabAktualisieren;
                tabAktualisieren.Visibility = Visibility.Visible;
            }
            else if (tvi.Header.ToString() == "KursZeit") {
                tabControl1.SelectedItem = tabAktualisieren;
                tabAktualisieren.Visibility = Visibility.Visible;
            }
            else if (tvi.Header.ToString() == "KursAend") {
                tabControl1.SelectedItem = tabAktualisieren;
                tabAktualisieren.Visibility = Visibility.Visible;
            }
            else if (tvi.Header.ToString() == "Sharpe") {
                tabControl1.SelectedItem = tabAktualisieren;
                tabAktualisieren.Visibility = Visibility.Visible;
            }
            else {
                MessageBox.Show("TreeViewItem_Selected() Fehler, keinen Item gefunden!!!!:" + tvi.Header.ToString());
            }
        }
        public void alleTabsHidden() {
            tabAllgemein.Visibility = Visibility.Hidden;
            tabNavigation.Visibility = Visibility.Hidden;
            tabBearbeiten.Visibility = Visibility.Hidden;
            tabAnsicht.Visibility = Visibility.Hidden;
            tabWertpapiere.Visibility = Visibility.Hidden;
            tabFinanzübersicht.Visibility = Visibility.Hidden;
            tabAktualisieren.Visibility = Visibility.Hidden;
        }
        private void btHilfe_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("btHilfe_Click() NOCH");
        }
        private void btÜbernehmen_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
        private void btAbbrechen_Click(object sender, RoutedEventArgs e) {
            isDataDirty = false;
            this.Close();
        }
        private void btOk_Click(object sender, RoutedEventArgs e) {
            isDataDirty = true;
            this.Close();
        }        
        private void cbFinanzübersicht_Checked(object sender, RoutedEventArgs e) {
            //MessageBox.Show("cbFinanzübersicht_Checked Zustand: Checked");
        }
        private void cbFinanzübersicht_Unchecked(object sender, RoutedEventArgs e) {
            //MessageBox.Show("cbFinanzübersicht_Unchecked Zustand: Unchecked");
        }
        private void cbWertpapiere_Checked(object sender, RoutedEventArgs e) {
        }
        private void cbWertpapiere_Unchecked(object sender, RoutedEventArgs e) {
        }
    }
}