using HtmlAgilityPack;
using SuperMemoAssistant.Interop.SuperMemo.Elements.Builders;
using System;
using System.Text.RegularExpressions;

namespace SuperMemoAssistant.Plugins.AdvancedClozer
{
  public static class ReferenceParser
  {

    /// <summary>
    /// Takes element content html string and returns References object.
    /// </summary>
    /// <param name="content"></param>
    /// <returns>References object or null</returns>
    public static References GetReferences(string content)
    {

      if (string.IsNullOrEmpty(content))
        return null;

      References refs = new References();

      // Load into HtmlDocument
      var doc = new HtmlDocument();
      doc.LoadHtml(content);
      if (doc == null)
        return null;

      // Get the Html Node where SM stores references
      string referenceString = GetReferencesHtml(doc);

      // Get each reference as a string and add to the reference object
      refs.Author = GetReference(referenceString, "Author");
      refs.Comment = GetReference(referenceString, "Comment");
      // refs.Date = GetReference(referenceString, "Date"); // TODO
      refs.Email = GetReference(referenceString, "Email");
      refs.Link = GetReference(referenceString, "Link");
      refs.Source = GetReference(referenceString, "Source");
      refs.Title = GetReference(referenceString, "Title");

      return refs;

    }

    /// <summary>
    /// Get the html string containing element references.
    /// </summary>
    /// <param name="doc"></param>
    /// <returns>References html string or null</returns>
    private static string GetReferencesHtml(HtmlDocument doc)
    {

      if (doc == null)
        return null;

      var ret = doc.DocumentNode.SelectSingleNode("//supermemoreference");
      return ret?.InnerHtml;

    }

    /// <summary>
    /// Get the reference from the referenceHtml.
    /// </summary>
    /// <param name="referenceHtml"></param>
    /// <param name="refName"></param>
    /// <returns>Reference string or null</returns>
    private static string GetReference(string referenceHtml, string refName)
    {

      if (string.IsNullOrEmpty(referenceHtml))
        return string.Empty;

      string pattern = String.Format(@"#{0}: (.*?)<br>", refName);
      Regex regex = new Regex(pattern);
      Match match = regex.Match(referenceHtml);

      if (match.Success)
      {

        string reference = match.Groups[1].Value;

        // Remove any html within the reference 
        var doc = new HtmlDocument();
        doc.LoadHtml(reference);
        return doc.DocumentNode.InnerText?.Trim();

      }

      return string.Empty;

    }
  }
}
