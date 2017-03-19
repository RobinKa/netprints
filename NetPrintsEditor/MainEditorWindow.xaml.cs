using NetPrints.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Collections.Concurrent;
using NetPrints.Serialization;
using System.Collections.ObjectModel;
using NetPrintsEditor.ViewModels;

namespace NetPrintsEditor
{
    /// <summary>
    /// Interaction logic for MainEditorWindow.xaml
    /// </summary>
    public partial class MainEditorWindow : Window
    {
        public static DependencyProperty ClassesProperty = DependencyProperty.Register(
            nameof(Classes), typeof(ObservableCollection<ClassVM>), typeof(MainEditorWindow));

        public ObservableCollection<ClassVM> Classes
        {
            get => GetValue(ClassesProperty) as ObservableCollection<ClassVM>;
            set => SetValue(ClassesProperty, value);
        }

        private Dictionary<ClassVM, ClassEditorWindow> classEditorWindows = new Dictionary<ClassVM, ClassEditorWindow>();
        
        public MainEditorWindow()
        {
            InitializeComponent();

            ConcurrentBag<ClassVM> classes = new ConcurrentBag<ClassVM>();

            string[] files = Directory.GetFiles(".", "*.xml", SearchOption.TopDirectoryOnly);
            Parallel.ForEach(files, file =>
            {
                Class cls = SerializationHelper.LoadClass(file);
                classes.Add(new ClassVM(cls));
            });

            Classes = new ObservableCollection<ClassVM>(classes.OrderBy(c => c.Name));
        }

        private void OpenOrCreateClassWindow(ClassVM cls)
        {
            if (classEditorWindows.ContainsKey(cls))
            {
                // Bring existing window to front

                ClassEditorWindow wnd = classEditorWindows[cls];

                if (!wnd.IsVisible)
                {
                    wnd.Show();
                }

                if (wnd.WindowState == WindowState.Minimized)
                {
                    wnd.WindowState = WindowState.Normal;
                }

                wnd.Activate();
                wnd.Topmost = true;
                wnd.Topmost = false;
                wnd.Focus();
            }
            else
            {
                // Create new window

                ClassEditorWindow wnd = new ClassEditorWindow(cls);
                wnd.Closed += (w, ea) => classEditorWindows.Remove(cls);
                classEditorWindows.Add(cls, wnd);
                wnd.Show();
            }
        }

        #region UI Events
        private void OnClassButtonClicked(object sender, RoutedEventArgs e)
        {
            if(sender is Button button && button.DataContext is ClassVM cls)
            {
                OpenOrCreateClassWindow(cls);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Close all open windows
            foreach(ClassEditorWindow wnd in new List<ClassEditorWindow>(classEditorWindows.Values))
            {
                wnd.Close();
            }
        }

        private void NewClassButtonClicked(object sender, RoutedEventArgs e)
        {
            Class cls = new Class()
            {
                Name = NetPrintsUtil.GetUniqueName("MyClass", Classes.Select(c => c.Name).ToList()),
                Namespace = NetPrintsUtil.GetUniqueName("MyNamespace", Classes.Select(c => c.Namespace).ToList())
            };

            ClassVM clsVM = new ClassVM(cls);

            Classes.Add(clsVM);

            OpenOrCreateClassWindow(clsVM);
        }
        #endregion
    }
}

