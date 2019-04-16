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
    /// Interaction logic for ReferenceListWindow.xaml
    /// </summary>
    public partial class ReferenceListWindow : CustomDialog
    {
        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(
            nameof(Project), typeof(ProjectVM), typeof(ReferenceListWindow));

        public ProjectVM Project
        {
            get => (ProjectVM)GetValue(ProjectProperty);
            set => SetValue(ProjectProperty, value);
        }

        public ReferenceListWindow(ProjectVM project)
        {
            InitializeComponent();

            Project = project;
        }

        private void OnAddAssemblyReferenceClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var assemblyReference = new AssemblyReference(openFileDialog.FileName);

                    if (!Project.Project.References.OfType<AssemblyReference>().Any(r =>
                        string.Equals(Path.GetFullPath(r.AssemblyPath), Path.GetFullPath(assemblyReference.AssemblyPath), StringComparison.OrdinalIgnoreCase)))
                    {
                        Project.Project.References.Add(assemblyReference);
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

                    if (!Project.Project.References.OfType<SourceDirectoryReference>().Any(r =>
                        string.Equals(Path.GetFullPath(r.SourceDirectory), Path.GetFullPath(sourceDirectoryReference.SourceDirectory), StringComparison.OrdinalIgnoreCase)))
                    {
                        Project.Project.References.Add(sourceDirectoryReference);
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
                Project.Project.References.Remove(reference.Reference);
            }
        }
    }
}
