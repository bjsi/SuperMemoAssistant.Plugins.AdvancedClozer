using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Sys.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
  public class AdvancedClozerCfg : INotifyPropertyChangedEx
  {

    [Field(Name = "Default Cloze Location")]
    [SelectFrom(typeof(ClozeLocation),
                SelectionType = SelectionType.RadioButtonsInline)]
    public ClozeLocation clozeLocation { get; set; } = ClozeLocation.Naess;

    [Field(Name = "Default Cloze Style")]
    [SelectFrom(typeof(ClozeStyle),
                SelectionType = SelectionType.RadioButtonsInline)]
    public ClozeStyle clozeStyle { get; set; } = ClozeStyle.Normal;

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
    public bool IncludeInverseHint { get; set; } = false;

    [Field(Name = "Hint split character")]
    public char HintSplitChar { get; set; } = '/';

    [Field(Name = "Save cloze hint history?")]
    public bool SaveClozeHintHistory { get; set; } = false;

    [JsonIgnore]
    public bool IsChanged { get; set; }

    public override string ToString()
    {
      return "AdvancedClozer";
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
