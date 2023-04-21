using System;
using System.Collections.Generic;
using System.Diagnostics;
using DrakiaXYZ.VersionChecker;

namespace Solarint.VersionCheck
{
    public static class GClassReferences
    {
        public static readonly Dictionary<int, string> RecoilDataClassNames = new Dictionary<int, string>
        {
            { 22617, "GClass545" }, // spt 3.5.5
            { 22173, "GClass543" }, // spt 3.5.1 - 3.5.4
        };

        public static readonly Dictionary<int, string> ShootDataClassNames = new Dictionary<int, string>
        {
            { 22617, "GClass546" }, // spt 3.5.5
            { 22173, "GClass544" }, // spt 3.5.1 - 3.5.4
        };

        public static readonly Lazy<Type> recoilClass = new Lazy<Type>(() => GetDataType(RecoilDataClassNames, CheckBuildVersion()));
        public static Type DynamicRecoilClass => recoilClass.Value;

        public static readonly Lazy<Type> shootClass = new Lazy<Type>(() => GetDataType(ShootDataClassNames, CheckBuildVersion()));
        public static Type ShootClass => shootClass.Value;

        public static int CheckBuildVersion()
        {
            int buildVersion = TarkovVersion.BuildVersion;
            return buildVersion;
        }

        public static Type GetDataType(Dictionary<int, string> classNames, int buildVersion)
        {
            if (classNames.TryGetValue(buildVersion, out string className))
            {
                return Type.GetType(className);
            }
            else
            {
                throw new Exception($"No class found for game version {buildVersion}");
            }
        }
    }
}