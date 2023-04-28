using System;
using System.Linq;
using System.Reflection;

namespace DrakiaXYZ.VersionChecker
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class TarkovVersion : Attribute
    {
        private int version;
        public TarkovVersion() : this(0) { }
        public TarkovVersion(int version)
        {
            this.version = version;
        }

        public static int BuildVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(TarkovVersion), false)
                    ?.Cast<TarkovVersion>()?.FirstOrDefault()?.version ?? 0;
            }
        }
    }
}
