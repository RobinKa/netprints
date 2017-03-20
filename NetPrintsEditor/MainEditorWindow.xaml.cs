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
using Microsoft.Win32;

namespace NetPrintsEditor
{
    /// <summary>
    /// Interaction logic for MainEditorWindow.xaml
    /// </summary>
    public partial class MainEditorWindow : Window
    {
        public static DependencyProperty ProjectProperty = DependencyProperty.Register(
            nameof(Project), typeof(ProjectVM), typeof(MainEditorWindow));

        public ProjectVM Project
        {
            get => (ProjectVM)GetValue(ProjectProperty);
            set => SetValue(ProjectProperty, value);
        }

        private Dictionary<ClassVM, ClassEditorWindow> classEditorWindows = new Dictionary<ClassVM, ClassEditorWindow>();

        public MainEditorWindow()
        {
            InitializeComponent();

            Project = new ProjectVM(null);
        }
        
        private void OpenOrCreateClassEditorWindow(ClassVM cls)
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

        public void CloseAllClassEditorWindows()
        {
            // Close all open windows
            foreach (ClassEditorWindow wnd in new List<ClassEditorWindow>(classEditorWindows.Values))
            {
                wnd.Close();
            }
        }

        #region UI Events
        private void OnClassButtonClicked(object sender, RoutedEventArgs e)
        {
            if(sender is Button button && button.DataContext is ClassVM cls)
            {
                OpenOrCreateClassEditorWindow(cls);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            CloseAllClassEditorWindows();
        }

        private void NewClassButtonClicked(object sender, RoutedEventArgs e)
        {
            ClassVM newClass = Project.CreateNewClass();
        }

        private void ExistingClassButtonClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Class Files (*.xml)|*.xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ClassVM existingClass = Project.AddExistingClass(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load existing class at path {openFileDialog.FileName}:\n\n{ex}", 
                        "Failed to load existing class", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        private void OnOpenProjectClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Project Files (*.xml)|*.xml"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Project = ProjectVM.LoadFromPath(openFileDialog.FileName);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Failed to load project at path {openFileDialog.FileName}:\n\n{ex}", 
                        "Failed to load project", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnSaveProjectClicked(object sender, RoutedEventArgs e)
        {
            // Open the save dialog if no path is set yet
            if (Project.Path == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog()
                {
                    DefaultExt = "xml",
                    AddExtension = true,
                    Filter = "Project Files (*.xml)|*.xml",
                    OverwritePrompt = true,
                    FileName = $"{Project.Name}.xml"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    Project.Path = saveFileDialog.FileName;
                }
            }

            if(Project.Path != null)
            {
                Project.Save();
            }
        }

        private void OnCreateProjectClicked(object sender, RoutedEventArgs e)
        {
            Project = ProjectVM.CreateNew("MyProject", "MyNamespace");
        }
    }
}

