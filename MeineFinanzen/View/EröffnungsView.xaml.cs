// 21.11.2016  EröffnungsView.cs 
using System;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Security;
namespace MeineFinanzen.View {
    public partial class EröffnungsView : Window {
        public static string _filename;
        public EröffnungsView() {
            _filename = Helpers.GlobalRef.g_Ein.strBilderPfad + @"\" + Helpers.GlobalRef.g_Ein.strStartBild;
            InitializeComponent();
        }
        private void EröffnungsView_Loaded(object sender, RoutedEventArgs e) {
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(_filename, UriKind.Relative);
            bi3.EndInit();
            image1.Stretch = System.Windows.Media.Stretch.Fill;
            image1.Source = bi3;
            txtBildPath.Text = _filename;
        }
        private void Image1_Loaded(object sender, RoutedEventArgs e) {
            SetzeImageSource();
        }
        private void SetzeImageSource() {
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri(_filename);
            b.EndInit();
            image1.Source = b;
            txtBildPath.Text = _filename;
        }
        private void BtDurchsuchen_Click(object sender, RoutedEventArgs e) {
            //string[] directories = Directory.GetDirectories(Helpers.GlobalRef.g_Ein.strBilderPfad);
            //string[] files = Directory.GetFiles(Helpers.GlobalRef.g_Ein.strBilderPfad, "*.*");
            try {
                OpenFileDialog dlg = new OpenFileDialog {
                    InitialDirectory = Helpers.GlobalRef.g_Ein.strBilderPfad,
                    DefaultExt = "*",   //"jpg";
                    Filter = "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*"
                };
                //"JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|" +
                //"GIF Files (*.gif)|*.gif|Alle Files (*.*)|*.*";           
                //dlg.Filter = "PNG|*.png|DOT|*.dot|Windows Bitmap Format|*.bmp|GIF|*.gif|" +
                //    "JPEG|*.jpg|PDF|*.pdf|Scalable Vector Graphics|*.svg||Alle Files (*.*)|*.*" +
                //    "Tag Image File Format|*.tiff";
                UseDefaultExtAsFilterIndex(dlg);
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true) {
                    string pfad = dlg.FileName;
                    string bildname = "";
                    string bilderpfad = "";
                    Char delimiter = '\\';
                    String[] substrings = pfad.Split(delimiter);
                    foreach (var substring in substrings) {
                        bildname = substring;
                        if (bildname.Contains(".")) {
                            Console.WriteLine("  Bildname: {0}", bildname);
                            Helpers.GlobalRef.g_Ein.strStartBild = bildname;
                        }
                        else
                            bilderpfad += substring + @"\";
                    }
                    Helpers.GlobalRef.g_Ein.strBilderPfad = bilderpfad.Substring(0, bilderpfad.Length - 1);
                    Helpers.GlobalRef.g_Ein.strStartBild = bildname;
                    _filename = Helpers.GlobalRef.g_Ein.strBilderPfad + @"\" +
                        Helpers.GlobalRef.g_Ein.strStartBild;
                    SetzeImageSource();
                    Helpers.GlobalRef.g_Ein.SerializeWriteEinstellungen(
                        Helpers.GlobalRef.g_Ein.strEinstellungen,
                        Helpers.GlobalRef.g_Ein);
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Fehler in btDurchsuchen_Click(): " + ex);
                DialogResult = false;
            }
        }
        public static void UseDefaultExtAsFilterIndex(FileDialog dialog) {
            var ext = "*." + dialog.DefaultExt;
            var filter = dialog.Filter;
            var filters = filter.Split('|');
            for (int i = 1; i < filters.Length; i += 2) {
                if (filters[i] == ext) {
                    dialog.FilterIndex = 1 + (i - 1) / 2;
                    return;
                }
            }
        }
        private void BtAbbrechen_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            return;
        }
        private void BtOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            return;
        }
    }
    public static class PasswordHelper {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password",
            typeof(string), typeof(PasswordHelper),
            new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach",
            typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, Attach));
        private static readonly DependencyProperty IsUpdatingProperty =
           DependencyProperty.RegisterAttached("IsUpdating", typeof(bool),
           typeof(PasswordHelper));
        public static void SetAttach(DependencyObject dp, bool value) {
            dp.SetValue(AttachProperty, value);
        }
        public static bool GetAttach(DependencyObject dp) {
            return (bool)dp.GetValue(AttachProperty);
        }
        public static string GetPassword(DependencyObject dp) {
            return (string)dp.GetValue(PasswordProperty);
        }
        public static void SetPassword(DependencyObject dp, string value) {
            dp.SetValue(PasswordProperty, value);
        }
        private static bool GetIsUpdating(DependencyObject dp) {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }
        private static void SetIsUpdating(DependencyObject dp, bool value) {
            dp.SetValue(IsUpdatingProperty, value);
        }
        private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            PasswordBox passwordBox = sender as PasswordBox;
            passwordBox.PasswordChanged -= PasswordChanged;
            if (!(bool)GetIsUpdating(passwordBox)) {
                passwordBox.Password = (string)e.NewValue;
            }
            passwordBox.PasswordChanged += PasswordChanged;
        }
        private static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e) {

            if (!(sender is PasswordBox passwordBox))
                return;
            if ((bool)e.OldValue) {
                passwordBox.PasswordChanged -= PasswordChanged;
            }
            if ((bool)e.NewValue) {
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }
        private static void PasswordChanged(object sender, RoutedEventArgs e) {
            PasswordBox passwordBox = sender as PasswordBox;
            SetIsUpdating(passwordBox, true);
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }
    }  
}