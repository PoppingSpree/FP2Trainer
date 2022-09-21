using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(Fp2Trainer.BuildInfo.Description)]
[assembly: AssemblyDescription(Fp2Trainer.BuildInfo.Description)]
[assembly: AssemblyCompany(Fp2Trainer.BuildInfo.Company)]
[assembly: AssemblyProduct(Fp2Trainer.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + Fp2Trainer.BuildInfo.Author)]
[assembly: AssemblyTrademark(Fp2Trainer.BuildInfo.Company)]
[assembly: AssemblyVersion(Fp2Trainer.BuildInfo.Version)]
[assembly: AssemblyFileVersion(Fp2Trainer.BuildInfo.Version)]
[assembly: MelonInfo(typeof(Fp2Trainer.Fp2Trainer), Fp2Trainer.BuildInfo.Name, Fp2Trainer.BuildInfo.Version, Fp2Trainer.BuildInfo.Author, Fp2Trainer.BuildInfo.DownloadLink)]
[assembly: MelonColor()]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]