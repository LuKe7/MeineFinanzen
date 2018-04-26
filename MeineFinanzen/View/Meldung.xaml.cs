using System.Windows;
namespace MeineFinanzen.View {
    public partial class Meldung : Window {
        public string MeldungsText { get; set; }
        public Meldung() {
            InitializeComponent();
            DataContext = this;
            Topmost = true;
        }
    }
}