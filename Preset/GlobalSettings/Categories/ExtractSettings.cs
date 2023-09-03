using System.ComponentModel;
using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings
{
    public class ExtractSettings
    {
        [Default(true)]
        public bool EnableExtractsGlobal = true;
    }
}