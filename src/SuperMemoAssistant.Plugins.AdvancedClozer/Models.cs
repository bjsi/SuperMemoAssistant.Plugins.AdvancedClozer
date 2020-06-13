using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.AdvancedClozer
{
  public enum ClozeLocation
  {
    Normal,
    Naess
  }

  public enum ClozeStyle
  {
    Normal,
    Spoiler
  }

  public class ClozeHintOptions
  {

    public string Hint { get; set; }
    public ClozeLocation ClozeLocation { get; set; }

    public ClozeHintOptions(string Hint, ClozeLocation ClozeLocation)
    {

      this.Hint = Hint;
      this.ClozeLocation = ClozeLocation;

    }
  }
}
