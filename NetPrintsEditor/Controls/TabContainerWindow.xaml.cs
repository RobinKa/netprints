using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dragablz;
using MahApps.Metro.Controls;

namespace NetPrintsEditor.Controls
{
    /// <summary>
    /// IInterTabClient implementation that creates a TabContainerWindow host window.
    /// </summary>
    public class TabContainerInterTabClient : IInterTabClient
    {
        public static TabContainerInterTabClient Instance { get; } = new TabContainerInterTabClient();

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return window is TabContainerWindow ? TabEmptiedResponse.CloseWindowOrLayoutBranch : TabEmptiedResponse.DoNothing;
        }

        INewTabHost<Window> IInterTabClient.GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            var view = new TabContainerWindow();
            view.TabControl.InterTabController.InterTabClient = interTabClient;
            
            return new NewTabHost<TabContainerWindow>(view, view.TabControl);
        }
    }

    /// <summary>
    /// Interaction logic for TabContainerWindow.xaml
    /// </summary>
    public partial class TabContainerWindow : MetroWindow
    {
        public IInterTabClient InterTabClient => TabContainerInterTabClient.Instance;

        public TabContainerWindow()
        {
            InitializeComponent();
        }
    }
}
