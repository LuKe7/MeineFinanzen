// 07.01.2018 KontenaufstellungHBCI4j.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MeineFinanzen.Model;
using MeineFinanzen.ViewModel;
namespace MeineFinanzen.View {
    public partial class KontenaufstellungHBCI4j : Window {
        public KontenaufstellungHBCI4j() {
            InitializeComponent();
            }
        private void Window_Loaded(Object sender, RoutedEventArgs e) {
            dgKontenaufstellung.ItemsSource = null;
            dgKontenaufstellung.ItemsSource = DgBanken.ko4js;
            dgKontenaufstellung.EnableRowVirtualization = false;
            }
        private void CloseWindow(Object sender, System.ComponentModel.CancelEventArgs e) {

            }
        private void dgvKontenaufstellung_PreviewMouseDown(Object sender, MouseButtonEventArgs e) {
            ConWrLi("---- -xx- dgvKontenaufstellung_PreviewMouseDown");
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell))
                dep = VisualTreeHelper.GetParent(dep);
            if (dep == null)
                return;
            DataGridCell cell1 = dep as DataGridCell;
            DataGridRow dgRow1 = dep as DataGridRow;
            while ((dep != null) && !(dep is DataGridRow))
                dep = VisualTreeHelper.GetParent(dep);
            dgRow1 = dep as DataGridRow;
            if (dgRow1 == null)
                return;
            //int nro = FindRowIndex(dgRow1);
            DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(dgRow1) as DataGrid;
            var item = dataGrid.ItemContainerGenerator.ItemFromContainer(dgRow1);
            //Console.WriteLine("cell1.Column.Header: {0}", cell1.Column.Header); // ist z.B. WPURLSharp
            string _ColHeader = cell1.Column.Header.ToString();            
            //ConWrLi("---- -94- boDgvRowAusgewählt=true");
            string _curName = ((WertpapSynchro)item).WPSName;
            string _curIsin = ((WertpapSynchro)item).WPSISIN;
            if (e.LeftButton == MouseButtonState.Pressed) { }        
            }
        private void InnereDatagrid_PreviewMouseDown(Object sender, MouseButtonEventArgs e) {

            }
        private void ConWrLi(string str1) {
            Console.WriteLine("{0,-50} {1}", str1, DateTime.Now.ToString("yyyy.MM.dd  HH:mm:ss.f"));
            }
        }
    }
