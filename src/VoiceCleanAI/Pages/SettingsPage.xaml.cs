using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using System.Linq;

namespace VoiceCleanAI.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        // 言語設定の反映
        string currentLang = ApplicationLanguages.PrimaryLanguageOverride;
        if (string.IsNullOrEmpty(currentLang)) currentLang = ApplicationLanguages.Languages[0];
        
        foreach (ComboBoxItem item in LanguageComboBox.Items)
        {
            if (item.Tag.ToString() == currentLang)
            {
                LanguageComboBox.SelectedItem = item;
                break;
            }
        }

        // テーマ設定の反映
        var currentTheme = MainWindow.Instance?.RootElement.RequestedTheme ?? ElementTheme.Default;
        foreach (RadioButton rb in ThemeRadioButtons.Items)
        {
            if (rb.Tag.ToString() == currentTheme.ToString())
            {
                rb.IsChecked = true;
                break;
            }
        }
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageComboBox.SelectedItem is ComboBoxItem item)
        {
            string lang = item.Tag.ToString()!;
            if (ApplicationLanguages.PrimaryLanguageOverride != lang)
            {
                ApplicationLanguages.PrimaryLanguageOverride = lang;
                // MainWindowをリロードして反映
                MainWindow.Instance?.Reload();
            }
        }
    }

    private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeRadioButtons.SelectedItem is RadioButton rb)
        {
            string themeStr = rb.Tag.ToString()!;
            if (Enum.TryParse(themeStr, out ElementTheme theme))
            {
                if (App.MainWindow is MainWindow window)
                {
                    window.RootElement.RequestedTheme = theme;
                }
            }
        }
    }
}
