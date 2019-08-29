using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Platformer {

    public sealed partial class MainPage : Page {

        public MainPage() {
			InitializeComponent();
			Window.Current.CoreWindow.KeyDown += keyPressed;
			Window.Current.CoreWindow.KeyUp += keyReleased;
			System.Diagnostics.Debug.WriteLine("finished window init");
		}

	}
}
