using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp.Utils;
using System.Windows.Interop;
using System.Diagnostics;

namespace PerMonitorDPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var ws = new WindowSettings(100, 100, 400, 300, WindowState.Normal);
            //var ws = new WindowSettings(-800, 156, 400, 300, WindowState.Normal);
            //var ws = new WindowSettings(3750, 251, 400, 300, WindowState.Normal);
            //var ws = new WindowSettings(4235, 311, 400, 300, WindowState.Maximized);

            var wsh = new WindowSettingsHelper();
            wsh.Set(this, ws);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var wsh = new WindowSettingsHelper();
            var ws = wsh.Get(this);
            PrintSettings(ws);
        }

        private void PrintSettings(WindowSettings ws)
        {
            var buf = new StringBuilder();
            buf.Append($"var ws = new WindowSettings({ws.Left},{ws.Top},{ws.Width},{ws.Height},WindowState.{ws.State});");
            Debug.WriteLine(buf);
        }
    }
}