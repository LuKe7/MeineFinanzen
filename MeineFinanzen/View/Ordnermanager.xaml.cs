using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace MeineFinanzen.View {
    public partial class Ordnermanager : Window {
        public Stack<string> dirs;
        public string BildPfad77 { get; set; }
        private object dummyNode = null;
        public System.Collections.Specialized.StringCollection log = new System.Collections.Specialized.StringCollection();
        public Ordnermanager() {
            InitializeComponent();
            BildPfad77 = @"D:\Visual Studio 2015\Projects\MeineFinanzenProjekte\MeineFinanzen\MeineFinanzen\MeineBilder\DiBa.png";
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            string[] drives = Environment.GetLogicalDrives();
            foreach (string dr in drives) {
                DriveInfo di = new DriveInfo(dr);
                DirectoryInfo rootDir = di.RootDirectory;
                TraverseTree(rootDir.ToString());
            }
            Console.WriteLine("---- Files with restricted access ----");
            foreach (string s in log) {
                Console.WriteLine(s);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
        }
        public void TraverseTree(string root) {
            Console.WriteLine("---- Root: ({0}) ----", root);
            TreeViewItem item = new TreeViewItem {
                Header = root,
                Tag = root,
                IsExpanded = true,
                FontWeight = FontWeights.Normal
            };
            item.Items.Add(dummyNode);
            item.Expanded += new RoutedEventHandler(Folder_Expanded);
            treeView.Items.Add(item);
            Console.WriteLine("item.Header: {0} item.Tag: {1}", item.Header, item.Tag);

            //TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode) {
                item.Items.Clear();
                try {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString())) {
                        TreeViewItem subitem = new TreeViewItem {
                            Header = s.Substring(s.LastIndexOf("\\") + 1),
                            Tag = s,
                            FontWeight = FontWeights.Normal
                        };
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(Folder_Expanded);
                        item.Items.Add(subitem);
                    }
                } catch (Exception) { }
            }

            dirs = new Stack<string>();
            dirs.Push(root);                    // Das 1. Verzeichnis ist: 'C:\\'.
            string currentDir;
            while (dirs.Count > 0 && dirs.Count < 100) {
                currentDir = dirs.Pop();
                try {
                    dirs = MachEs(currentDir);
                } catch {
                    log.Add("currentDir: " + currentDir);
                    DirectoryInfo dInfo = new DirectoryInfo(currentDir);
                    //DirectorySecurity dSecurity = dInfo.GetAccessControl();
                    //Console.WriteLine("Kein Zugriff: " + dInfo);
                    continue;
                }
            }
        }
        Stack<string> MachEs(string currDir) {
            string[] subDirs = Directory.GetDirectories(currDir);
            foreach (string str in subDirs)
                if (str.Contains("MeineFinanzen"))
                    Console.WriteLine("---- MeineFinanzen in subDirs ----  " + str);            
            string[] files = Directory.GetFiles(currDir);            
            foreach (string file in files) {
                if (file.Length > 240)
                    continue;
                FileInfo fi = new FileInfo(file);
                if (fi.Name.Contains("MeineFinanzen")) {
                    Console.WriteLine("---- MeineFinanzen in fi : {0,-60}: {1,8}, {2}", fi.Name, fi.Length, fi.CreationTime);


                    TreeViewItem item = new TreeViewItem {
                        Header = fi.Name,
                        Tag = fi.Length,
                        IsExpanded = true,
                        FontWeight = FontWeights.Normal
                    };
                    item.Items.Add(dummyNode);
                    item.Expanded += new RoutedEventHandler(Folder_Expanded2);
                    treeView2.Items.Add(item);
                    Console.WriteLine("item.Header2: {0} item.Tag: {1}", item.Header, item.Tag);
                }
            }
            foreach (string str in subDirs) {
                try {
                    if (str.Contains("MeineFinanzen")) {
                        if (str.Contains("Kopie"))
                            continue;
                        //Console.WriteLine("MeineFinanzen: {0,-60}", str);
                        TreeViewItem item = new TreeViewItem {
                            Header = str,
                            Tag = str.Length,
                            IsExpanded = true,
                            FontWeight = FontWeights.Normal
                        };
                        item.Items.Add(dummyNode);
                        item.Expanded += new RoutedEventHandler(Folder_Expanded2);
                        treeView2.Items.Add(item);
                        //Console.WriteLine("item.Header2: {0} item.Tag: {1}", item.Header, item.Tag);
                    }
                    dirs.Push(str);
                } catch {
                    log.Add(str);
                }
            }
            return dirs;
        }
        void Folder_Expanded(object sender, RoutedEventArgs e) {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode) {
                item.Items.Clear();
                try {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString())) {
                        TreeViewItem subitem = new TreeViewItem {
                            Header = s.Substring(s.LastIndexOf("\\") + 1),
                            Tag = s,
                            FontWeight = FontWeights.Normal
                        };
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(Folder_Expanded);
                        item.Items.Add(subitem);
                    }
                } catch (Exception) { }
            }
        }
        void Folder_Expanded2(object sender, RoutedEventArgs e) {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode) {
                item.Items.Clear();
                try {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString())) {
                        TreeViewItem subitem = new TreeViewItem {
                            Header = s.Substring(s.LastIndexOf("\\") + 1),
                            Tag = s,
                            FontWeight = FontWeights.Normal
                        };
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(Folder_Expanded);
                        item.Items.Add(subitem);
                    }
                } catch (Exception) { }
            }
        }
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
        }
        private void TreeView_SelectedItemChanged2(object sender, RoutedPropertyChangedEventArgs<object> e) {

        }
    }
    public class HeaderToImageConverter : IValueConverter {
        public static HeaderToImageConverter Instance = new HeaderToImageConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is string gr))
                return false;        
            BitmapImage source = null;
            try {
                if ((value as string).Contains(@"\")) {
                    Uri uri = new Uri("pack://application:,,,/Images/diskdrive.png");
                    source = new BitmapImage(uri);
                } else {
                    Uri uri = new Uri("pack://application:,,,/Images/folder.png");
                    source = new BitmapImage(uri);
                }
            } catch {
                return false;
            }
            return source;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException("Cannot convert back");
        }
    }
    public class IconPathConverter7 : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value is string && ((string)value).Length == 0)
                return (ImageSource)null;
            if (!File.Exists((string)value)) {
                return (ImageSource)null;
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
    public class RecursiveFileSearch {
        static System.Collections.Specialized.StringCollection log = new System.Collections.Specialized.StringCollection();

        public static void Main2() {
            // Start with drives if you have to search the entire computer.
            string[] drives = System.Environment.GetLogicalDrives();
            foreach (string dr in drives) {
                System.IO.DriveInfo di = new System.IO.DriveInfo(dr);

                // Here we skip the drive if it is not ready to be read. This
                // is not necessarily the appropriate action in all scenarios.
                if (!di.IsReady) {
                    Console.WriteLine("The drive {0} could not be read", di.Name);
                    continue;
                }
                System.IO.DirectoryInfo rootDir = di.RootDirectory;
                WalkDirectoryTree(rootDir);
            }

            // Write out all the files that could not be processed.
            Console.WriteLine("Files with restricted access:");
            foreach (string s in log) {
                Console.WriteLine(s);
            }
            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key");
            Console.ReadKey();
        }

        static void WalkDirectoryTree(System.IO.DirectoryInfo root) {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e) {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                log.Add(e.Message);
            } catch (System.IO.DirectoryNotFoundException e) {
                Console.WriteLine(e.Message);
            }

            if (files != null) {
                foreach (System.IO.FileInfo fi in files) {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    Console.WriteLine(fi.FullName);
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs) {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo);
                }
            }
        }
    }
}