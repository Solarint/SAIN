using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Editor
{
    public class EditorCustomization
    {
        [JsonProperty]
        public string FontName = null;
        [JsonProperty]
        public int FontSize = 12;
    }
}
