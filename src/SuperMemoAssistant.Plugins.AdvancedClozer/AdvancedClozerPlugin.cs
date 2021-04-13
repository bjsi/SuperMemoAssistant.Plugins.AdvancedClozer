using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Anotar.Serilog;
using HtmlAgilityPack;
using mshtml;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.Plugins;
using SuperMemoAssistant.Interop.SuperMemo.Content.Contents;
using SuperMemoAssistant.Interop.SuperMemo.Content.Models;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Models;
using SuperMemoAssistant.Plugins.AdvancedClozer.UI;
using SuperMemoAssistant.Services;
using SuperMemoAssistant.Services.IO.HotKeys;
using SuperMemoAssistant.Services.IO.Keyboard;
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
  public class AdvancedClozerPlugin : SMAPluginBase<AdvancedClozerPlugin>
  {
    #region Constructors

    /// <inheritdoc />
    public AdvancedClozerPlugin() { }

    #endregion

    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "AdvancedClozer";
    public AdvancedClozerCfg Config { get; private set; }

    /// <inheritdoc />
    public override bool HasSettings => true;
    private ClozeHintWdw CurrentWdw { get; set; }

    #endregion


    #region Methods Impl

    /// <inheritdoc />
    /// 

    private async Task LoadConfig()
    {
      Config = await Svc.Configuration.Load<AdvancedClozerCfg>() ?? new AdvancedClozerCfg();
    }

    protected override void PluginInit()
    {

      LoadConfig().Wait();

      Svc.HotKeyManager
       .RegisterGlobal(
        "AdvancedClozer",
        "Create cloze hint",
        HotKeyScope.SMBrowser,
        new HotKey(Key.Z, KeyModifiers.AltShift),
        CreateClozeHint
      )
       .RegisterGlobal(
        "ResurrectParent",
        "Bring the parent of a cloze back to life",
        HotKeyScope.SMBrowser,
        new HotKey(Key.R, KeyModifiers.AltShift),
        ResurrectParent
      )
       .RegisterGlobal(
        "ClozeAsItem",
        "Create a cloze as an item.",
        HotKeyScope.SMBrowser,
        new HotKey(Key.I, KeyModifiers.AltShift),
        CreateItemCloze
      );


      Application.Current.Dispatcher.Invoke(() =>
      {
        var wdw = new ClozeHintWdw();
        wdw.Width = 1;
        wdw.Height = 1;
        wdw.ShowAndActivate();
        wdw.Close();
      });

    }

    [LogToErrorOnException]
    private void CreateItemCloze()
    {
      try
      {
        var parentEl = Svc.SM.UI.ElementWdw.CurrentElement;
        if (parentEl == null || parentEl.Type == ElementType.Item)
          return;

        var selObj = ContentUtils.GetSelectionObject();
        string selText = selObj?.htmlText;
        if (selObj == null || string.IsNullOrEmpty(selText))
          return;

        var htmlCtrl = ContentUtils.GetFocusedHtmlCtrl();
        var htmlDoc = htmlCtrl?.GetDocument();
        if (htmlDoc == null)
          return;

        selObj.pasteHTML("[...]");
        string questionChild = htmlDoc.body.innerHTML.Replace("[...]", "<SPAN class=cloze>[...]</SPAN>");

        int MaxTextLength = 2000000000;
        selObj.moveEnd("character", MaxTextLength);
        selObj.moveStart("character", -MaxTextLength);

        selObj.findText("[...]");
        selObj.select();

        selObj.pasteHTML("<SPAN class=clozed>" + selText + "</SPAN>");
        string parentContent = htmlDoc.body.innerHTML;

        htmlCtrl.Text = parentContent;

        var references = ReferenceParser.GetReferences(parentContent);
        CreateSMElement(RemoveReferences(questionChild), selText, references);

      }
      catch (RemotingException) { }
    }

    [LogToErrorOnException]
    private void CreateSMElement(string question, string answer, References refs)
    {

      var contents = new List<ContentBase>();
      var parent = Svc.SM.UI.ElementWdw.CurrentElement;
      var ctrlGroup = Svc.SM.UI.ElementWdw.ControlGroup;

      contents.Add(new TextContent(true, question));
      contents.Add(new TextContent(true, answer, displayAt: AtFlags.NonQuestion));

      if (parent == null)
      {
        LogTo.Error("Failed to CreateSMElement because parent element was null");
        return;
      }

      bool success = Svc.SM.Registry.Element.Add(
        out _,
        ElemCreationFlags.ForceCreate,
        new ElementBuilder(ElementType.Item, contents.ToArray())
          .WithParent(parent)
          .WithLayout("Item")
          .WithPriority(30)
          .DoNotDisplay()
          .WithReference((_) => refs)
      );

      if (success)
      {
        LogTo.Debug("Successfully created SM Element");
      }
      else
      {
        LogTo.Error("Failed to CreateSMElement");
      }
    }

    private void ResurrectParent()
    {
      var currentElement = Svc.SM.UI.ElementWdw.CurrentElement;
      var parentOfCurrent = currentElement.Parent;

      var htmls = ContentUtils.GetHtmlCtrls();
      if (htmls == null || htmls.Count < 2)
      {
        LogTo.Debug("Failed to resurrect. There are less than 2 html ctrls");
        return;
      }

      var fst = htmls.FirstOrDefault();
      var lst = htmls.LastOrDefault();

      if (!fst.Text.Contains("[...]"))
      {
        LogTo.Debug("Failed to resurrect. There was no cloze marker found in the first html ctrl");
        return;
      }

      var answer = lst.Text;

      // Replace cloze marker with answer text.

      var doc = new HtmlDocument();
      doc.LoadHtml(fst.Text);
      var cloze = doc.DocumentNode.SelectSingleNode("//span[@class='cloze']");
      if (cloze == null)
      {
        LogTo.Debug("Failed to resurrect. Failed to find the cloze span.");
        return;
      }

      cloze.Attributes["class"].Remove();
      cloze.InnerHtml = answer;

      var question = doc.DocumentNode.OuterHtml;
      var refs = ReferenceParser.GetReferences(question);
      question = RemoveReferences(question);

      if (parentOfCurrent == null)
      {
        LogTo.Debug("Failed to resurrect parent. Parent element of current was null.");
        return;
      }

      bool ret = Svc.SM.Registry.Element.Add(
        out var value,
        ElemCreationFlags.ForceCreate,
        new ElementBuilder(ElementType.Topic, new ContentBase[] { new TextContent(true, question) })
        .WithParent(parentOfCurrent)
        .WithPriority(Config.ResurrectedParentPriority)
        .WithReference(_ => refs)
        .DoNotDisplay());

      if (ret)
      {
        LogTo.Debug("Successfully resurrected parent of cloze");
      }
      else
      {
        LogTo.Error("Failed to resurrect parent of cloze");
      }
    }

    private string RemoveReferences(string htmlText)
    {
      var idx = htmlText.IndexOf("HR SuperMemo", System.StringComparison.InvariantCultureIgnoreCase);
      if (idx >= 0)
        return htmlText.Substring(0, idx + 1);
      return htmlText;
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
      var htmlCtrl = ContentUtils.GetClozeHtmlControl();
      var text = htmlCtrl?.Text;
      var htmlDoc = htmlCtrl?.GetDocument();
      var body = htmlDoc?.body as IHTMLBodyElement;

      if (text.IsNullOrEmpty() || htmlCtrl.IsNull() || body.IsNull())
      {
        string msg = "Failed to create cloze: Failed to get text from the generated item";
        MessageBox.Show(msg);
        LogTo.Error(msg);
        return;
      }

      string replacement = Location == ClozeLocation.Inside
        ? $"[{Hint}]"
        : $"[...]({Hint})";

      htmlCtrl.Text = htmlCtrl.Text.Replace("[...]", replacement);

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
