using SuperMemoAssistant.Services;
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
using System.Windows.Threading;

namespace SuperMemoAssistant.Plugins.AdvancedClozer.UI
{
  public partial class ClozeHintWdw
  {
    
    /// <summary>
    /// True if the user presses the create button, else false.
    /// </summary>
    public bool Confirmed { get; set; } = false;

    /// <summary>
    /// The cloze hint.
    /// </summary>
    public string Hint { get; set; }

    /// <summary>
    /// Where to place the hint
    /// </summary>
    public ClozeLocation Location { get; set; }

    public bool IsClosed { get; set; }

    private AdvancedClozerCfg Config => Svc<AdvancedClozerPlugin>.Plugin.Config;
    private List<string> CfgHints => Config.ComboBoxHints?.Split('\n')?.ToList();
    public List<string> AllHints = new List<string>();

    public ClozeHintWdw()
    {

      InitializeComponent();
      FocusComboBox();
      CreateAllHints();
      ClozeHintTextbox.ItemsSource = AllHints;
      Closed += ClozeHintWdw_Closed;

    }

    private void ClozeHintWdw_Closed(object sender, EventArgs e)
    {
      IsClosed = true;
    }

    /// <summary>
    /// Set default options from the user's config.
    /// </summary>
    private void SetUserDefaults()
    {

      if (Config.clozeLocation == ClozeLocation.Naess)
        ClozeLocationNaess.IsChecked = true;
      else
        ClozeLocationNormal.IsChecked = true;

    }

    /// <summary>
    /// AllHints is CfgHints plus the reverse hint if desired.
    /// </summary>
    private void CreateAllHints()
    {

      if (CfgHints == null || CfgHints.Count == 0)
        return;

      foreach (var hint in CfgHints)
      {
        AllHints.Add(hint.Trim());

        // Eg. if yes/no is in the config, this will include no/yes
        if (Config.IncludeInverseHint && hint.Contains(Config.HintSplitChar))
        {
          AllHints.Add(string.Join(Config.HintSplitChar.ToString(), hint.Split(Config.HintSplitChar).Reverse<string>()));
        }
      }

    }

    private void OkBtn_Click(object sender, RoutedEventArgs e)
    {
      Confirmed = true;
      Hint = ClozeHintTextbox.Text.Trim();

      Location = ClozeLocationNormal.IsChecked == true
        ? ClozeLocation.Normal
        : ClozeLocation.Naess;

      if (Config.SaveClozeHintHistory)
      {
        var hintList = Config.ComboBoxHints.Split('\n').Select(item => item.Trim()).ToList();
        if (hintList.All(h => h != Hint))
        {
          hintList.Add(Hint);
          Config.ComboBoxHints = string.Join("\n", hintList);
        }
      }

      Close();
    }

    /// <summary>
    /// Attempts to focus the combobox properly.
    /// </summary>
    private void FocusComboBox()
    {
      ClozeHintTextbox.Dispatcher.BeginInvoke((Action)(() =>
      {
        ClozeHintTextbox.Focus();
      }), DispatcherPriority.Render);
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        OkBtn_Click(sender, e);
        e.Handled = true;
      }
      else if (e.Key == Key.Escape)
      {
        CancelBtn_Click(sender, e);
        e.Handled = true;
      }
    }

    private void ClozeHintTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        OkBtn_Click(sender, e);
      }
    }
  }
}
