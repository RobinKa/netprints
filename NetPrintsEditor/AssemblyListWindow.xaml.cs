using Microsoft.Win32;
using NetPrintsEditor.ViewModels;
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
                    string fullName = ReflectionUtil.GetAssemblyFullNameFromPath(openFileDialog.FileName);

                    if (!Project.Assemblies.Contains(fullName))
                    {
                        Project.Assemblies.Add(fullName);
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
            if (sender is Button button && button.DataContext is string assemblyName)
            {
                Project.Assemblies.Remove(assemblyName);
            }
        }
    }
}
