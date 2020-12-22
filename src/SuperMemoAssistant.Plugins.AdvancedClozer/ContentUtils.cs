using mshtml;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.AdvancedClozer
{
  public static class ContentUtils
  {
    /// <summary>
    /// Get the currently selected text in SM.
    /// </summary>
    /// <returns>string or null</returns>
    public static string GetSelectedText()
    {

      try
      {
        var ctrlGroup = Svc.SM.UI.ElementWdw.ControlGroup;
        var htmlCtrl = ctrlGroup?.FocusedControl?.AsHtml();
        var htmlDoc = htmlCtrl?.GetDocument();
        var sel = htmlDoc?.selection;

        if (!(sel?.createRange() is IHTMLTxtRange textSel))
          return null;

        return textSel.text;
      }
      catch (UnauthorizedAccessException) { }
      catch (COMException) { }

      return null;

    }

    public static IControlHtml GetFirstHtmlControl()
    {
      try
      {
        var ctrlGroup = Svc.SM.UI.ElementWdw.ControlGroup;
        return ctrlGroup?.GetFirstHtmlControl()?.AsHtml();
      }
      catch (UnauthorizedAccessException) { }
      catch (COMException) { }

      return null;
    }
  }
}
