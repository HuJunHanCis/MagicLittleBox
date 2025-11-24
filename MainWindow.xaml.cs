using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Abb.Egm;
using Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Abb.Egm;
using Serilog;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using Google.Protobuf;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace MagicLittleBox
{
    public partial class MainWindow
    {
        
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