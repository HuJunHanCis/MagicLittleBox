using System.Windows;
using System.Windows.Input;

namespace MagicLittleBox
{
    public partial class MainWindow
    {

        // Legacy code from version 0.1.2 is preserved in
        // Legacy/MainWindow.Legacy_0.1.2.cs for reference during refactors.
        
        private void OnWindowDragMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
}
}

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            var element = FocusManager.GetFocusedElement(this);
            if (element != null && !element.IsMouseOver)
            {
                FocusManager.SetFocusedElement(this, this);
            }
            base.OnPreviewMouseDown(e);
        }
        public MainWindow()
        {
            InitializeComponent();
        }

    }
}
