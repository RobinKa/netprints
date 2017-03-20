using Microsoft.Win32;
using NetPrintsEditor.Compilation;
using NetPrintsEditor.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NetPrintsEditor
{
    /// <summary>
    /// Interaction logic for AssemblyListWindow.xaml
    /// </summary>
    public partial class AssemblyListWindow : Window
    {
        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(
            nameof(Project), typeof(ProjectVM), typeof(AssemblyListWindow));

        public ProjectVM Project
        {
            get => (ProjectVM)GetValue(ProjectProperty);
            set => SetValue(ProjectProperty, value);
        }

        public AssemblyListWindow(ProjectVM project)
        {
            InitializeComponent();

            Project = project;
        }

        private void OnAddAssemblyButtonClicked(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnAddAssemblyFromPathButtonClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if(openFileDialog.ShowDialog() == true)
            {
                try
                {
                    LocalAssemblyName localAssemblyName = LocalAssemblyName.FromPath(openFileDialog.FileName);
                    
                    if (!Project.Assemblies.Any(n => n.Name == localAssemblyName.Name || n.Path == localAssemblyName.Path))
                    {
                        Project.Assemblies.Add(localAssemblyName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add assembly at {openFileDialog.FileName}:\n\n{ex}");
                }
            }
        }

        private void OnRemoveAssemblyButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is LocalAssemblyName assemblyName)
            {
                Project.Assemblies.Remove(assemblyName);
            }
        }
    }
}
