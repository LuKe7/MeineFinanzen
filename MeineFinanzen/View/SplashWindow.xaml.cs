using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Threading;
namespace MeineFinanzen.View {
    public partial class SplashWindow : Window {
        private int mSek = 500;
        HauptFenster _mw;
        Thread loadingThread;
        Storyboard Showboard;
        Storyboard Hideboard;
        private delegate void ShowDelegate(string txt);
        private delegate void HideDelegate();
        ShowDelegate showDelegate;
        HideDelegate hideDelegate;
        public SplashWindow(HauptFenster mw) {
            _mw = mw;
            InitializeComponent();
            showDelegate = new ShowDelegate(this.showText);
            hideDelegate = new HideDelegate(this.hideText);
            Showboard = this.Resources["showStoryBoard"] as Storyboard;
            Hideboard = this.Resources["HideStoryBoard"] as Storyboard;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            loadingThread = new Thread(load);
            loadingThread.Start();
        }
        private void load() {
            Thread.Sleep(mSek);
            this.Dispatcher.Invoke(showDelegate, _mw.strSplash);
            Thread.Sleep(mSek);
            this.Dispatcher.Invoke(hideDelegate);

            Thread.Sleep(mSek);
            this.Dispatcher.Invoke(showDelegate, _mw.strSplash);
            Thread.Sleep(mSek);
            this.Dispatcher.Invoke(hideDelegate);

            Thread.Sleep(mSek);
            this.Dispatcher.Invoke(showDelegate, _mw.strSplash);
            Thread.Sleep(mSek);
            this.Dispatcher.Invoke(hideDelegate);

            Thread.Sleep(mSek);
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate () { Close(); });
        }
        private void showText(string txt) {
            txtLoading.Text = txt;
            BeginStoryboard(Showboard);
        }
        private void hideText() {
            BeginStoryboard(Hideboard);
        }

    }
}