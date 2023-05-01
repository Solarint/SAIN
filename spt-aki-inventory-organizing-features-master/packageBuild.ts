#!/usr/bin/env node

// SPT-AKI mods folder path
const sptAkiVersion = "3.5.4";
const targetVersion = "3.5.5";
const targetVersionPostfix = `.SPT-AKI.${targetVersion}`;
//const sptAkiMods = `D:/SPT-AKI/SPTARKOV ${sptAkiVersion}/user/mods`;
const sptAkiBepinex = `D:/SPT-AKI/SPTARKOV ${sptAkiVersion}/BepInEx/plugins`;
const pluginName = "InventoryOrganizingFeatures";
const pluginPath = `client-side/${pluginName}/bin/Debug/net472`;
const pluginFileName = `${pluginName}.dll`;

// This is a simple script used to build a mod package. The script will copy necessary files to the build directory
// and compress the build directory into a zip file that can be easily shared.

const fs = require("fs-extra");
const glob = require("glob");
const zip = require("bestzip");
const path = require("path");

// Load the package.json file to get some information about the package so we can name things appropriately. This is
// atypical, and you would never do this in a production environment, but this script is only used for development so
// it's fine in this case. Some of these values are stored in environment variables, but those differ between node
// versions; the 'author' value is not available after node v14.
const { author, name:packageName, version } = require("./package.json");

// Generate the name of the package, stripping out all non-alphanumeric characters in the 'author' and 'name'.
const modName = `${author.replace(/[^a-z0-9]/gi, "")}-${packageName.replace(/[^a-z0-9]/gi, "")}-${version}`;
console.log(`Generated package name: ${modName}`);

// Delete the old build directory and compressed package file.
fs.rmSync(`${__dirname}/dist`, { force: true, recursive: true });
// Delete the old directory from SPT-AKI mods folder.
//fs.rmSync(`${sptAkiMods}/${modName}`, { force: true, recursive: true }); - comment out for now, no server side

console.log("Previous build files deleted.");

// Generate a list of files that should not be copied over into the distribution directory. This is a blacklist to ensure
// we always copy over additional files and directories that authors may have added to their project. This may need to be
// expanded upon by the mod author to allow for node modules that are used within the mod; example commented out below.
const ignoreList = [
    "node_modules/",
    // "node_modules/!(weighted|glob)", // Instead of excluding the entire node_modules directory, allow two node modules.
    "src/**/*.js",
    "types/",
    ".git/",
    ".gitea/",
    ".eslintignore",
    ".eslintrc.json",
    ".gitignore",
    ".DS_Store",
    "packageBuild.ts",
    "mod.code-workspace",
    "package-lock.json",
    "tsconfig.json",
    "README.md",
    "Readme.docx",
    "portrait_options/",
    "client-side/",
    "dist/",
    "dist_zip/"
];
const exclude = glob.sync(`{${ignoreList.join(",")}}`, { realpath: true, dot: true });

// For some reason these basic-bitch functions won't allow us to copy a directory into itself, so we have to resort to
// using a temporary directory, like an idiot. Excuse the normalize spam; some modules cross-platform, some don't...

//  - comment out for now, no server side
// fs.copySync(__dirname, path.normalize(`${__dirname}/../~${modName}`), {filter:(filePath) => 
// {
//     return !exclude.includes(filePath);
// }});
// fs.moveSync(path.normalize(`${__dirname}/../~${modName}`), path.normalize(`${__dirname}/${modName}`), { overwrite: true });

// Copy into a different structure.
//fs.copySync(path.normalize(`${__dirname}/${modName}`), path.normalize(`${__dirname}/dist/user/mods/${modName}`)); - comment out for now, no server side
fs.copySync(path.normalize(`${__dirname}/${pluginPath}/${pluginFileName}`), path.normalize(`${__dirname}/dist/BepInEx/plugins/${pluginFileName}`));

//fs.copySync(path.normalize(`${__dirname}/${modName}`), path.normalize(`${__dirname}/dist/user/mods/${modName}`));

// My personal preference to copy a ready to test version to the mods folder of SPT-AKI
//fs.copySync(path.normalize(`${__dirname}/${modName}`), path.normalize(`${sptAkiMods}/${modName}`)); - comment out for now, no server side
fs.copySync(path.normalize(`${__dirname}/${pluginPath}/${pluginFileName}`), path.normalize(`${sptAkiBepinex}/${pluginFileName}`));

console.log("Build files copied.");

// Compress the files for easy distribution. The compressed file is saved into the dist directory. When uncompressed we
// need to be sure that it includes a directory that the user can easily copy into their game mods directory.
if (!fs.existsSync(path.normalize(`${__dirname}/dist_zip`)))
{
    fs.mkdirSync(path.normalize(`${__dirname}/dist_zip`));
}

console.log(__dirname+"\\dist")

zip({
    source: "*",
    destination: `../dist_zip/${modName}${targetVersionPostfix ?? ""}.zip`,
    cwd: `${__dirname}/dist`
}).catch(function(err)
{
    console.error("A bestzip error has occurred: ", err.stack);
}).then(function()
{
    console.log(`Compressed mod package to: /dist/${modName}${targetVersionPostfix ?? ""}.zip`);

    // Now that we're done with the compression we can delete the temporary build directory.
    fs.rmSync(`${__dirname}/${modName}`, { force: true, recursive: true });
    console.log("Build successful! your zip file has been created and is ready to be uploaded to hub.sp-tarkov.com/files/");
});