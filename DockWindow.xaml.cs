using System;
using System.Windows;

namespace MagicLittleBox
{
    public partial class DockWindow : Window
    {
        public DockWindow()
        {
            InitializeComponent();
        }
        
        private void Clear(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }
        
        public void AppendLog(string message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => AppendLog(message));
                return;
            }

            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.CaretIndex = LogTextBox.Text.Length;
            LogTextBox.ScrollToEnd();
        }
    }
}