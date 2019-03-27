using MahApps.Metro.Controls;
using Microsoft.Win32;
using NetPrintsEditor.Compilation;
using NetPrintsEditor.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace NetPrintsEditor
{
    /// <summary>
    /// Interaction logic for AssemblyListWindow.xaml
    /// </summary>
    public partial class AssemblyListWindow : CustomDialog
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

        private void OnAddAssemblyReferenceClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if(openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var assemblyReference = new AssemblyReference(openFileDialog.FileName);
                    
                    if (!Project.References.OfType<AssemblyReference>().Any(r =>
                        string.Equals(Path.GetFullPath(assemblyReference.AssemblyPath), Path.GetFullPath(assemblyReference.AssemblyPath), StringComparison.OrdinalIgnoreCase)))
                    {
                        Project.References.Add(new CompilationReferenceVM(assemblyReference));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add assembly at {openFileDialog.FileName}:\n\n{ex}");
                }
            }
        }

        private void OnAddSourceDirectoryReferenceClicked(object sender, RoutedEventArgs e)
        {
            var openFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var sourceDirectoryReference = new SourceDirectoryReference(openFolderDialog.SelectedPath);

                    if (!Project.References.OfType<SourceDirectoryReference>().Any(r =>
                        string.Equals(Path.GetFullPath(r.SourceDirectory), Path.GetFullPath(sourceDirectoryReference.SourceDirectory), StringComparison.OrdinalIgnoreCase)))
                    {
                        Project.References.Add(new CompilationReferenceVM(sourceDirectoryReference));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add sources at {openFolderDialog.SelectedPath}:\n\n{ex}");
                }
            }
        }

        private void OnRemoveReferenceClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CompilationReferenceVM reference)
            {
                Project.References.Remove(reference);
            }
        }
    }
}
