using Forge.Forms;
using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace SuperMemoAssistant.Plugins.AdvancedClozer
{

  [Form(Mode = DefaultFields.None)]
  [Title("Watcher Settings",
  IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
  "Cancel",
  IsCancel = true)]
  [DialogAction("save",
  "Save",
  IsDefault = true,
  Validates = true)]
  public class AdvancedClozerCfg : CfgBase<AdvancedClozerCfg>, INotifyPropertyChangedEx
  {

    [Title("Advanced Clozer Plugin")]
    [Heading("By Jamesb | Experimental Learning")]

    [Heading("Features")]
    [Text(@"- Easily create cloze hint cards.
- Resurrect the parent topics of cloze cards.
- Create clozes with the starting interval of items.")]

    [Heading("Support")]
    [Text("If you would like to support my projects, check out my Patreon or buy me a coffee.")]

    [Action("patreon", "Patreon", Placement = Placement.Before, LinePosition = Position.Left)]
    [Action("coffee", "Coffee", Placement = Placement.Before, LinePosition = Position.Left)]

    [Heading("Links")]
    [Action("github", "GitHub", Placement = Placement.Before, LinePosition = Position.Left)]
    [Action("feedback", "Feedback Site", Placement = Placement.Before, LinePosition = Position.Left)]
    [Action("blog", "Blog", Placement = Placement.Before, LinePosition = Position.Left)]
    [Action("youtube", "YouTube", Placement = Placement.Before, LinePosition = Position.Left)]
    [Action("twitter", "Twitter", Placement = Placement.Before, LinePosition = Position.Left)]

    [Heading("Settings")]

    [Field(Name = "Default Cloze Location?")]
    [SelectFrom(typeof(ClozeLocation),
                SelectionType = SelectionType.RadioButtonsInline)]
    public ClozeLocation clozeLocation { get; set; } = ClozeLocation.Outside;

    [Field(Name = "Cloze Hint Autocomplete List")]
    [MultiLine]
    public string ComboBoxHints { get; set; } = string.Join("\n", new List<string>()
                                                {
                                                  "is/is not",
                                                  "can/cannot",
                                                  "increase/decrease",
                                                  "should/should not",
                                                  "up/down",
                                                  "better/worse",
                                                  "faster/slower",
                                                  "bigger/smaller",
                                                  "high/low",
                                                });

    [Field(Name = "Automatically include inverse hint?")]
    public bool IncludeInverseHint { get; set; } = true;

    [Field(Name = "Hint split character")]
    public char HintSplitChar { get; set; } = '/';

    [Field(Name = "Save cloze hint history?")]
    public bool SaveClozeHintHistory { get; set; } = true;

    [Field(Name = "Default resurrected parent priority?")]
    public double ResurrectedParentPriority { get; set; } = 30;

    [JsonIgnore]
    public bool IsChanged { get; set; }

    public override string ToString()
    {
      return "Advanced Clozer";
    }

    public override void HandleAction(IActionContext actionContext)
    {

      string patreon = "https://www.patreon.com/experimental_learning";
      string coffee = "https://buymeacoffee.com/experilearning";
      string github = "https://github.com/bjsi/SuperMemoAssistant.Plugins.HtmlTables";
      string feedback = "https://feedback.experimental-learning.com/";
      string youtube = "https://www.youtube.com/channel/UCIaS9XDdQkvIjASBfgim1Uw";
      string twitter = "https://twitter.com/experilearning";
      string blog = "https://www.experimental-learning.com/";

      string action = actionContext.Action as string;
      if (action == "patreon")
        openLinkDefaultBrowser(patreon);
      else if (action == "github")
        openLinkDefaultBrowser(github);
      else if (action == "coffee")
        openLinkDefaultBrowser(coffee);
      else if (action == "feedback")
        openLinkDefaultBrowser(feedback);
      else if (action == "youtube")
        openLinkDefaultBrowser(youtube);
      else if (action == "twitter")
        openLinkDefaultBrowser(twitter);
      else if (action == "blog")
        openLinkDefaultBrowser(blog);
      else
        base.HandleAction(actionContext);
    }

    // Hack
    private DateTime LastLinkOpen { get; set; } = DateTime.MinValue;

    private void openLinkDefaultBrowser(string url)
    {
      var diffInSeconds = (DateTime.Now - LastLinkOpen).TotalSeconds;
      if (diffInSeconds > 1)
      {
        LastLinkOpen = DateTime.Now;
        Process.Start(url);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
