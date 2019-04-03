using MahApps.Metro.Controls;
using Microsoft.Win32;
using NetPrintsEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;

namespace NetPrintsEditor
{
    /// <summary>
    /// Interaction logic for MainEditorWindow.xaml
    /// </summary>
    public partial class MainEditorWindow : MetroWindow
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

            if (App.StartupArguments != null && App.StartupArguments.Length == 1 && App.StartupArguments[0] != null)
            {
                _ = LoadProject(App.StartupArguments[0]);
            }
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
                wnd.WindowState = WindowState.Maximized;
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

        private void OnRemoveClassButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ClassVM cls)
            {
                if(classEditorWindows.ContainsKey(cls))
                {
                    classEditorWindows[cls].Close();
                    classEditorWindows.Remove(cls);
                }

                Project.Classes.Remove(cls);
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
                Filter = "Class Files (*.netpc)|*.netpc"
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

        private async Task LoadProject(string path)
        {
            var overlay = await this.ShowProgressAsync("Loading project", path);
            overlay.SetIndeterminate();

            try
            {
                Project = await Task.Run(() => ProjectVM.LoadFromPath(path));
                await overlay.CloseAsync();
            }
            catch (Exception ex)
            {
                await overlay.CloseAsync();
                var result = await this.ShowMessageAsync("Failed to load project", $"Failed to load project at path {path}. The exception has been copied to your clipboard.\n\n{ex}",
                    MessageDialogStyle.Affirmative, new MetroDialogSettings());
                Clipboard.SetText(ex.ToString());
            }
        }

        private void OnProjectButtonClicked(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = false;
            ProjectFlyout.IsOpen = !ProjectFlyout.IsOpen;
        }

        private void OnSettingsButtonClicked(object sender, RoutedEventArgs e)
        {
            ProjectFlyout.IsOpen = false;
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
        }

        private void OnOpenProjectClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Project Files (*.netpp)|*.netpp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ProjectFlyout.IsOpen = false;
                _ = LoadProject(openFileDialog.FileName);
            }
        }

        private void OnSaveProjectClicked(object sender, RoutedEventArgs e)
        {
            ProjectFlyout.IsOpen = false;
            PromptProjectSave();
        }

        /// <summary>
        /// Prompts a project save dialog if the project path was not
        /// already set. Then saves the project and all its classes
        /// into that directory.
        /// </summary>
        private bool PromptProjectSave()
        {
            // Open the save dialog if no path is set yet
            if (Project.Path == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog()
                {
                    DefaultExt = "netpp",
                    AddExtension = true,
                    Filter = "Project Files (*.netpp)|*.netpp",
                    OverwritePrompt = true,
                    FileName = $"{Project.Name}.netpp"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    Project.Path = saveFileDialog.FileName;
                }
            }

            if (Project.Path != null)
            {
                Project.Save();
                return true;
            }

            return false;
        }

        private void OnCreateProjectClicked(object sender, RoutedEventArgs e)
        {
            ProjectVM oldProject = Project;
            Project = ProjectVM.CreateNew("MyProject", "MyNamespace");

            if (PromptProjectSave())
            {
                ProjectFlyout.IsOpen = false;
            }
            else
            {
                // Restore old project if we didn't create a new one.
                Project = oldProject;
            }

            if (Project != null && Project.Project != null)
            {
                Project.Name = Path.GetFileNameWithoutExtension(Project.Path);
            }
        }

        private void OnCompileButtonClicked(object sender, RoutedEventArgs e)
        {
            Project.CompileProject();
        }

        private void OnRunButtonClicked(object sender, RoutedEventArgs e)
        {
            Project.PropertyChanged += OnProjectPropertyChangedWhileCompiling;
            Project.CompileProject();
        }

        private void OnProjectPropertyChangedWhileCompiling(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Project.IsCompiling) && !Project.IsCompiling)
            {
                Project.PropertyChanged -= OnProjectPropertyChangedWhileCompiling;

                if (Project.LastCompilationSucceeded)
                {
                    Project.RunProject();
                }
            }
        }

        private async void OnReferencesButtonClicked(object sender, RoutedEventArgs e)
        {
            var referenceListWindow = new ReferenceListWindow(Project);
            referenceListWindow.CloseButton.Click += async (sender, e) => await this.HideMetroDialogAsync(referenceListWindow);

            await this.ShowMetroDialogAsync(referenceListWindow);
        }
    }
}
