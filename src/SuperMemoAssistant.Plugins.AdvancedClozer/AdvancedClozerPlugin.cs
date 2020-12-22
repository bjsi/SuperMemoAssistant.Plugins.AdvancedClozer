using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Anotar.Serilog;
using mshtml;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Plugins.AdvancedClozer.UI;
using SuperMemoAssistant.Plugins.MouseoverHints.Interop;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.IO.Keyboard;
using SuperMemoAssistant.Services.Sentry;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.IO.Devices;

#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   6/12/2020 11:11:15 PM
// Modified By:  james

#endregion




namespace SuperMemoAssistant.Plugins.AdvancedClozer
{
  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class AdvancedClozerPlugin : SentrySMAPluginBase<AdvancedClozerPlugin>
  {
    #region Constructors

    /// <inheritdoc />
    public AdvancedClozerPlugin() : base("Enter your Sentry.io api key (strongly recommended)") { }

    #endregion

    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "AdvancedClozer";
    public AdvancedClozerCfg Config { get; set; }

    /// <inheritdoc />
    public override bool HasSettings => true;
    private ClozeHintWdw CurrentWdw { get; set; }
    public IMouseoverHintSvc mouseoverHintSvc { get; set; }
    private readonly char[] SentenceEndingPunct = new char[] { '.', '?', '!' };

    #endregion


    #region Methods Impl

    /// <inheritdoc />
    /// 

    private void LoadConfig()
    {
      Config = Svc.Configuration.Load<AdvancedClozerCfg>() ?? new AdvancedClozerCfg();
    }

    protected override void PluginInit()
    {

      LoadConfig();

      Svc.HotKeyManager
       .RegisterGlobal(
        "AdvancedClozer",
        "Create cloze hint",
        HotKeyScopes.SMBrowser,
        new HotKey(Key.Z, KeyModifiers.AltShift),
        CreateClozeHint
      );

      mouseoverHintSvc = GetService<IMouseoverHintSvc>();

    }

    /// <summary>
    /// Create the cloze hint.
    /// TODO: Add proper UI locking when methods are available
    /// </summary>
    private void CreateClozeHint()
    {

      /// It is not currently possible to modify an SM element without it being
      /// the currently displayed SM Element. Therefore to add the cloze hint
      /// this first generates the cloze, then navigates to
      /// the new cloze element to modify it, then returns to the original element

      // Cancel if there is no currently selected text
      if (ContentUtils.GetSelectedText().IsNullOrEmpty())
        return;

      var wdw = OpenClozeHintWdw();
      if (wdw.IsNull())
        return;

      ClozeLocation Location = wdw.Location;
      string Hint = wdw.Hint;
      bool HideCloze = wdw.HideCloze;
      bool HideContext = wdw.HideContext;

      if (Hint.IsNullOrEmpty())
        return;

      // The current element we are executing GenerateCloze on
      int restoreElementId = Svc.SM.UI.ElementWdw.CurrentElementId;

      // The newly created element
      int newClozeId = Svc.SM.UI.ElementWdw.GenerateCloze();

      if (newClozeId < 0)
      {
        string msg = "Failed to create cloze: GenerateCloze method failed.";
        MessageBox.Show(msg);
        LogTo.Debug(msg);
        return;
      }

      // Move to the new cloze, add the cloze hint
      Svc.SM.UI.ElementWdw.GoToElement(newClozeId);

      // TODO: Loop over htmlCtrls for the first control with a cloze symbol
      var htmlCtrl = ContentUtils.GetFirstHtmlControl();
      var text = htmlCtrl?.Text;
      var htmlDoc = htmlCtrl?.GetDocument();
      var body = htmlDoc?.body as IHTMLBodyElement;

      if (text.IsNullOrEmpty() || htmlCtrl.IsNull() || body.IsNull())
      {
        string msg = "Failed to create cloze: Failed to get text from the generated item";
        LogTo.Error(msg);
        return;
      }

      string replacement = Location == ClozeLocation.Inside
        ? $"[{Hint}]"
        : $"[...]({Hint})";

      htmlCtrl.Text = htmlCtrl.Text.Replace("[...]", replacement);

      if (HideCloze)
      {
        var toFind = Location == ClozeLocation.Inside
          ? replacement
          : replacement.Substring(5);

        var rng = body.createTextRange();
        if (rng.findText(toFind))
        {
          rng.moveStart("character", 1);
          rng.moveEnd("character", -1);
          mouseoverHintSvc.CreateSingleHint(rng);

          // This is necessary for the change to be saved
          // Otherwise the change is only made visually and once you move away from the element, it reverts
          htmlCtrl.Text = htmlCtrl.Text;
        }
      }

      if (HideContext)
      {
        var rng = body.createTextRange();
        if (rng.findText(replacement))
        {

          bool foundStart = false;
          bool foundEnd = false;

          // Move the beginning of the range backwards until the first character is
          // sentence ending punctuation
          while (rng.moveStart("character", -1) == -1)
          {
            if (SentenceEndingPunct.Any(x => x == rng.text[0]))
            {
              foundStart = true;
              break;
            }
          }

          // Move the end of the range backwards until the last character 
          // is sentence ending punctuation
          while (rng.moveEnd("character", 1 ) == 1)
          {
            if (SentenceEndingPunct.Any(x => x == rng.text.Last()))
            {
              foundEnd = true;
              break;
            }
          }

          if (foundStart && foundEnd)
          {
            bool ret = mouseoverHintSvc.HideContext(rng);

            // This is necessary for the change to be saved
            // Otherwise the change is only made visually and once you move away from the element, it reverts
            htmlCtrl.Text = htmlCtrl.Text;
          }
        }
      }

      // Return to the parent element
      Svc.SM.UI.ElementWdw.GoToElement(restoreElementId);
    }

    /// <summary>
    /// Opens a cloze hint window to get cloze hint options from the user.
    /// </summary>
    /// <returns>ClozeHintWdw Object or null</returns>
    private ClozeHintWdw OpenClozeHintWdw()
    {

      // return if ClozeHintWdw is already open
      if (CurrentWdw != null && !CurrentWdw.IsClosed)
        return null;

      return Application.Current.Dispatcher.Invoke(() =>
      {
        CurrentWdw = new ClozeHintWdw();
        CurrentWdw.ShowDialog(); // TODO: Don't use the window as a return value, just launch the
        // cloze creation from the window

        // Attempt to fix bug
        //Win32Interop.ClickSimulateFocus(CurrentWdw);
        //Win32Interop.SetForegroundWindow((new WindowInteropHelper(CurrentWdw)).Handle);
        //Win32Interop.SetActiveWindow((new WindowInteropHelper(CurrentWdw)).Handle);
        //FocusManager.SetFocusedElement(CurrentWdw, CurrentWdw.ClozeHintTextbox);
        //System.Windows.Input.Keyboard.Focus(CurrentWdw.ClozeHintTextbox);

        return CurrentWdw.Confirmed
          ? CurrentWdw
          : null;
      });

    }

     /// <inheritdoc />
    public override void ShowSettings()
    {
      ConfigurationWindow.ShowAndActivate(HotKeyManager.Instance, Config);
    }

    #endregion

    #region Methods
    #endregion
  }
}
