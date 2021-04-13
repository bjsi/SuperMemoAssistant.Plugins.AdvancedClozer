using mshtml;
using SuperMemoAssistant.Extensions;
using SuperMemoAssistant.Interop.SuperMemo.Content.Controls;
using SuperMemoAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
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

    public static IControlHtml GetClozeHtmlControl()
    {
      try
      {
        var ctrlGroup = Svc.SM.UI.ElementWdw.ControlGroup;
        if (ctrlGroup == null)
          return null;

        for (int i = 0; i < ctrlGroup.Count; i++)
        {
          var ctrl = ctrlGroup[i];
          if (ctrl is IControlHtml html)
          {
            var text = html.Text;
            if (text.Contains("[...]"))
              return html;
          }
        }
      }
      catch (UnauthorizedAccessException) { }
      catch (COMException) { }

      return null;
    }

    public static List<IControlHtml> GetHtmlCtrls()
    {

      var ret = new List<IControlHtml>();

      try
      {
        var ctrlGroup = Svc.SM.UI.ElementWdw.ControlGroup;
        if (ctrlGroup == null)
          return ret;

        for (int i = 0; i < ctrlGroup.Count; i++)
        {
          if (ctrlGroup[i] is IControlHtml html)
            ret.Add(html);
        }  
      }
      catch (UnauthorizedAccessException) { }
      catch (COMException) { }

      return ret;
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

    public static IControlHtml GetFocusedHtmlCtrl()
    {
      try
      {
        var ctrlGroup = Svc.SM.UI.ElementWdw.ControlGroup;
        return ctrlGroup?.FocusedControl?.AsHtml();
      }
      catch (RemotingException) { }
      catch (UnauthorizedAccessException) { }

      return null;
    }

    /// <summary>
    /// Get the HTML string content representing the first html control of the current element.
    /// </summary>
    /// <returns>HTML string or null</returns>
    public static string GetCurrentElementContent()
    {
      try
      {
        var ctrlGroup = Svc.SM.UI.ElementWdw.ControlGroup;
        var htmlCtrl = ctrlGroup?.GetFirstHtmlControl()?.AsHtml();
        return htmlCtrl?.Text;
      }
      catch (RemotingException) { }
      catch (UnauthorizedAccessException) { }

      return null;
    }

    /// <summary>
    /// Get the selection object representing the currently highlighted text in SM.
    /// </summary>
    /// <returns>IHTMLTxtRange object or null</returns>
    public static IHTMLTxtRange GetSelectionObject()
    {
      try
      {
        var ctrlGroup = Svc.SM.UI.ElementWdw.ControlGroup;
        var htmlCtrl = ctrlGroup?.FocusedControl?.AsHtml();
        var htmlDoc = htmlCtrl?.GetDocument();
        var sel = htmlDoc?.selection;

        if (!(sel?.createRange() is IHTMLTxtRange textSel))
          return null;

        return textSel;
      }
      catch (RemotingException) { }
      catch (UnauthorizedAccessException) { }

      return null;
    }
  }
}
