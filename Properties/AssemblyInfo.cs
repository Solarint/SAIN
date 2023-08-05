using DrakiaXYZ.VersionChecker;
using System.Reflection;
using System.Runtime.InteropServices;
using static SAIN.AssemblyInfo;
using SAIN;

[assembly: AssemblyTitle(Title)]
[assembly: AssemblyDescription(Description)]
[assembly: AssemblyConfiguration(Configuration)]
[assembly: AssemblyCompany(Company)]
[assembly: AssemblyProduct(Product)]
[assembly: AssemblyCopyright(Copyright)]
[assembly: AssemblyTrademark(Trademark)]
[assembly: AssemblyCulture(Culture)]
[assembly: ComVisible(false)]
[assembly: Guid("d08f8f91-95cf-4aa5-b7d8-f5d58f2feabb")]
[assembly: AssemblyVersion(SAINVersion)]
[assembly: AssemblyFileVersion(SAINVersion)]
[assembly: VersionChecker(AssemblyInfo.TarkovVersion)]
