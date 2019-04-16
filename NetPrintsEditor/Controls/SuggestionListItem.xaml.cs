using System.Windows.Controls;

namespace NetPrintsEditor.Controls
{
    public class SuggestionListItemBinding
    {
        public string Text { get; }
        public string IconPath { get; }

        public SuggestionListItemBinding(string text, string iconPath)
        {
            Text = text;
            IconPath = iconPath;
        }
    }

    /// <summary>
    /// Interaction logic for SuggestionListItem.xaml
    /// </summary>
    public partial class SuggestionListItem : UserControl
    {
        public SuggestionListItem()
        {
            InitializeComponent();
        }
    }
}
