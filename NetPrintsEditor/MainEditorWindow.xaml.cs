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
using NetPrints.Core;

namespace NetPrintsEditor
{
    /// <summary>
    /// Interaction logic for MainEditorWindow.xaml
    /// </summary>
    public partial class MainEditorWindow : MetroWindow
    {
        private readonly Dictionary<ClassEditorVM, ClassEditorWindow> classEditorWindows = new Dictionary<ClassEditorVM, ClassEditorWindow>();

        private MainEditorVM ViewModel
        {
            get => DataContext as MainEditorVM;
            set => DataContext = value;
        }

        public MainEditorWindow()
        {
            InitializeComponent();

            DataContext = new MainEditorVM(null);

            if (App.StartupArguments?.Length == 1 && App.StartupArguments[0] != null)
            {
                _ = LoadProject(App.StartupArguments[0]);
            }
        }

        private void OpenOrCreateClassEditorWindow(ClassEditorVM cls)
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

                ClassEditorWindow wnd = new ClassEditorWindow()
                {
                    DataContext = cls
                };

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
            if (sender is Button button && button.DataContext is ClassGraph classGraph)
            {
                OpenOrCreateClassEditorWindow(new ClassEditorVM(classGraph));
            }
        }

        private void OnRemoveClassButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ClassEditorVM cls)
            {
                if (classEditorWindows.ContainsKey(cls))
                {
                    classEditorWindows[cls].Close();
                    classEditorWindows.Remove(cls);
                }

                ViewModel.Project.Classes.Remove(cls.Class);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            CloseAllClassEditorWindows();
        }

        private void NewClassButtonClicked(object sender, RoutedEventArgs e)
        {
            _ = ViewModel.Project.CreateNewClass();
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
                    ClassGraph existingClass = ViewModel.Project.AddExistingClass(openFileDialog.FileName);
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
                ViewModel = await Task.Run(() => new MainEditorVM(Project.LoadFromPath(path)));
                await overlay.CloseAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await overlay.CloseAsync();
                Clipboard.SetText(ex.ToString());
                var result = await this.ShowMessageAsync("Failed to load project", $"Failed to load project at path {path}. The exception has been copied to your clipboard.\n\n{ex}",
                    MessageDialogStyle.Affirmative, new MetroDialogSettings()).ConfigureAwait(false);
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
            if (ViewModel.Project.Path == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog()
                {
                    DefaultExt = "netpp",
                    AddExtension = true,
                    Filter = "Project Files (*.netpp)|*.netpp",
                    OverwritePrompt = true,
                    FileName = $"{ViewModel.Project.Name}.netpp"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ViewModel.Project.Path = saveFileDialog.FileName;
                }
            }

            if (ViewModel.Project.Path != null)
            {
                ViewModel.Project.Save();
                return true;
            }

            return false;
        }

        private void OnCreateProjectClicked(object sender, RoutedEventArgs e)
        {
            MainEditorVM oldViewModel = ViewModel;
            ViewModel = new MainEditorVM(Project.CreateNew("MyProject", "MyNamespace"));

            if (PromptProjectSave())
            {
                ProjectFlyout.IsOpen = false;
            }
            else
            {
                // Restore old project if we didn't create a new one.
                ViewModel = oldViewModel;
            }

            if (ViewModel?.Project != null)
            {
                ViewModel.Project.Name = Path.GetFileNameWithoutExtension(ViewModel.Project.Path);
            }
        }

        private void OnCompileButtonClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.Project.CompileProject();
        }

        private void OnRunButtonClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged += OnProjectPropertyChangedWhileCompiling;
            ViewModel.Project.CompileProject();
        }

        private void OnProjectPropertyChangedWhileCompiling(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Project.IsCompiling) && !ViewModel.Project.IsCompiling)
            {
                ViewModel.PropertyChanged -= OnProjectPropertyChangedWhileCompiling;

                if (ViewModel.Project.LastCompilationSucceeded)
                {
                    ViewModel.Project.RunProject();
                }
            }
        }

        private async void OnReferencesButtonClicked(object sender, RoutedEventArgs e)
        {
            var referenceListWindow = new ReferenceListWindow()
            {
                DataContext = new ReferenceListViewModel(ViewModel.Project)
            };

            referenceListWindow.CloseButton.Click += async (sender, e) => await this.HideMetroDialogAsync(referenceListWindow).ConfigureAwait(false);

            await this.ShowMetroDialogAsync(referenceListWindow).ConfigureAwait(false);
        }
    }
}
