using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using HyperGames.AssetBundles;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.U2D;

public static class VariantsBuilder {
    
    [MenuItem("Assets/Clear AssetBundle info")]
    public static void AssignVariant() {
        string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        AssetImporter.GetAtPath(path).SetAssetBundleNameAndVariant(null, null);
    }

    [MenuItem("Dev/Clear All AssetBundles")]
    public static void ClearAllBundles() {
        // Clear directories
        string[] directories = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);
        
        List<string> assets = new List<string>();
        string[] bundles = AssetDatabase.GetAllAssetBundleNames();
        foreach (string bundleFullName in bundles) {
            assets.AddRange(AssetDatabase.GetAssetPathsFromAssetBundle(bundleFullName.Split('.')[0]));
        }
        
        int counter = 0;
        float max = directories.Length + assets.Count;
        
        foreach (string directory in directories) {
            string dir = "Assets" + directory.Substring(Application.dataPath.Length);
            AssetImporter importer = AssetImporter.GetAtPath(dir);
            if (importer == null) {
                continue;
            }

            ++counter;
            EditorUtility.DisplayProgressBar(
                "Clearing AssetBundle info", 
                "Removing AssetBundle names and variants from assets and directories",
                (counter / max));

            importer.SetAssetBundleNameAndVariant(null, null);
            importer.SaveAndReimport();
        }
        
        // Clear assets
        foreach (string asset in assets) {
            AssetImporter importer = AssetImporter.GetAtPath(asset);
            if (importer == null) {
                continue;
            }

            ++counter;
            EditorUtility.DisplayProgressBar(
                "Clearing AssetBundle info", 
                "Removing AssetBundle names and variants from assets and directories",
                (counter / max));

            importer.SetAssetBundleNameAndVariant(null, null);
            importer.SaveAndReimport();
        }

        EditorUtility.ClearProgressBar();
    }

    // Plugin system could have a pre method for doing preparations, and a GotBundles method for 
    // doing whatever with the bundles, and lastly a post method for finalizing stuff.
    // BuildVariants needs to initialize a plugin manager, load all plugins, execute
    // them one by one.
    // ! Pass AssetBundleConfig to pre-method?
    // !! Plugins shouldn't move or delete assets around. If they do, then BuildVariants
    // needs to scan for new bundles after each plugin run.

    // It uses the config to figure out which variants to build.
    // It looks through all the bundle names and looks for variants that matches the resolution variants
    // defined in the config. Based on the match, it makes a note of the missing vairants and calculates
    // a scaling factor for each, and uses that to scale the asset up or down.
    // Assets for valid bundles are moved to the temp directory. For each variant needed, a copy of
    // the asset is made, and the bundle name and new variant is set. The asset bundles are build using
    // the assets in the temp directory. Aassets belonging to a bundle that does not require a screen res
    // variant are left in place.
    // It doesn't matter where assets are before the bundles are build. When loading an asset from a bundle,
    // on the relative bundle path is used, like "folder/bundle.variant/asset.name"

    [MenuItem("Dev/Build Variants")]
    public static void BuildVariants() {
    
        StringBuilder log = new StringBuilder();

        const string configPath = "Assets/Configs/AssetBundleManagerConfig.asset";
        // Temp directory for storing variant assets
        string bundlesRoot = Application.dataPath + "/__BUNDLES__";

//        AssetDatabase.StartAssetEditing();

        // Load the AssetBundleConfig
        log.AppendLine("[BV] Does "+
            Application.dataPath.Replace("Assets", "")+configPath+
            " exist? "+
            (File.Exists(Application.dataPath.Replace("Assets", "")+configPath) ? "Yes" : "No"));

        AssetBundleManagerConfig config = AssetDatabase.LoadAssetAtPath<AssetBundleManagerConfig>(configPath);
        if (config == null) {
            log.AppendLine("[BV] Cannot load<AssetBundleManagerConfig> from " + configPath);
            return;
        }

        // Sort the resolution variants.
        // It doesn't matter if they are sorted in the asset, they should be anyway.
        config.resolutionVariants.Sort(new ResolutionVariantComparer());

        // Validate defined resolutionVariants with asset bundles
        log.AppendLine("[BV] Validate resolutionVariants");
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        foreach (string bundle in allBundles) {
            string ext = bundle.Substring(bundle.Length - 3);
            log.AppendLine("[BV] Bundle: "+bundle+" ext: "+ext);
            if (ext[0] != '.') {
                continue;
            }
            string variant = ext.Substring(1);
            log.AppendLine("[BV] Variant: "+variant);
            
            bool found = false;
            foreach (ResolutionVariant rv in config.resolutionVariants) {
                found |= rv.name == variant;
            }
            log.AppendLine("[BV] Found variant? "+found);
            if (found) {
                continue;
            }
            log.AppendLine("[BV] Variant "+variant+" cannot be found in AssetBundleConfig's resolutionVariants");
            return;
        }

        log.AppendLine("[BV] Clear "+bundlesRoot+" directory");
        // Create directory for storing screen res variant bundles
        if (!Directory.Exists(bundlesRoot)) {
            log.AppendLine("[BV] Create directory: " + bundlesRoot);
            Directory.CreateDirectory(bundlesRoot);
        } else {
            log.AppendLine("[BV] Empty directory: " + bundlesRoot);
            DirectoryInfo di = new DirectoryInfo(bundlesRoot);

            foreach (FileInfo file in di.GetFiles("*", SearchOption.AllDirectories)) {
                log.AppendLine("[BV] Delete file: "+file.Name);
                file.Delete(); 
            }
            foreach (DirectoryInfo dir in di.GetDirectories("*", SearchOption.AllDirectories)) {
                if (!Directory.Exists(dir.FullName)) {
                    continue;
                }
                log.AppendLine("[BV] Delete dir: "+dir.Name);
                dir.Delete(true); 
            }
        }
        AssetDatabase.Refresh();
        
        // Get all directories with a screen res variant
        log.AppendLine("[BV] Get all directories with a screen res variant");
        List<string> variantPathIndex = new List<string>();
        string[] directories = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);
        
        foreach (string directory in directories) {
            string dir = "Assets" + directory.Substring(Application.dataPath.Length);
            AssetImporter importer = AssetImporter.GetAtPath(dir);
            if (importer == null) {
                continue;
            }
            
            if (Array.IndexOf(AssetBundleManagerConfig.VARIANTS, importer.assetBundleVariant) < 0) {
                // variant is not a res variant
                continue;
            }

            log.AppendLine("[BV] Add variant dir to variantPathIndex. "+dir);
            variantPathIndex.Add(dir);
        }

        // Get a list of all bundles
        string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < bundleNames.Length; ++i) {
            
            string[] nameParts = bundleNames[i].Split('.');
            if (nameParts.Length < 2) {
                // Ignore AssetBundles without a variant
                continue;
            }

            string bundleVariant = nameParts[nameParts.Length - 1];
            if (Array.IndexOf(AssetBundleManagerConfig.VARIANTS, bundleVariant) < 0) {
                // variant is not a res variant
                // TODO: Use plugin system for handling other variants
                continue;
            }

            // This bundle has a screen res variant. This variant is the
            // one that all the others will be created from.
            string bundleName = nameParts[0];
            string bundlePath = bundlesRoot + "/" + bundleName;
            string variantPath = bundlePath + "/" + bundleVariant;
            string variantDir = "Assets" + variantPath.Substring(Application.dataPath.Length);

            log.AppendLine("[BV] Bundle: "+bundleNames[i]+" has variant: "+bundleVariant);

            // Create a subdir for bundles with screen res variants
            // Will this work for bundles with a slash in the name?
            if (!Directory.Exists(bundlePath)) {
                log.AppendLine("[BV] Create directory for bundle at: "+bundlePath);
                Directory.CreateDirectory(bundlePath);
                AssetDatabase.Refresh();
            }
            
            // Create subdir for variant
            if (!Directory.Exists(variantPath)) {
                log.AppendLine("[BV] Create directory for variant at: "+variantPath);
                Directory.CreateDirectory(variantPath);
                AssetDatabase.Refresh();
                
                // Set assetbundle info on variant subdir
                AssetImporter importer = AssetImporter.GetAtPath(variantDir);
                importer.SetAssetBundleNameAndVariant(bundleName, bundleVariant);
                log.AppendLine("[BV] Set AssetBundle name: "+bundleName+" and variant: "+bundleVariant+" on folder: "+variantDir);
            }

            string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundleNames[i]);
            log.AppendLine("[BV] Got "+assets.Length+" assets for bundle: "+bundleNames[i]);
            string[] newAssetPaths = new string[assets.Length];
            
            for (int a=0; a<assets.Length; ++a) {
                string asset = assets[a];
                // move into variant subfolder
                string[] assetParts = asset.Split('/');
                string targetDir = variantDir + "/" + assetParts[assetParts.Length - 1];
                newAssetPaths[a] = targetDir;
                log.AppendLine("[BV] Move: "+asset+" to: "+targetDir);
//                AssetDatabase.CopyAsset(asset, targetDir);
                AssetDatabase.MoveAsset(asset, targetDir);
                AssetDatabase.Refresh();
                
                // remove assetbundle data. Let parent folder handle asset bundle info
                AssetImporter importer = AssetImporter.GetAtPath(targetDir);
                if (importer == null) {
                    continue;
                }
                log.AppendLine("[BV] Clear AssetBundle info from "+targetDir);
                importer.SetAssetBundleNameAndVariant(null, null);
            }
            
            // Scale variants

            List<string> resVariants = new List<string>();
            for (int r = 0; r < config.resolutionVariants.Count; ++r) {
                resVariants.Add(config.resolutionVariants[r].name);
            }
            
            List<int> variantScales = new List<int>();
            for (int s = 0; s < resVariants.Count; ++s) {
                int si = Array.IndexOf(AssetBundleManagerConfig.VARIANTS, resVariants[s]);
                variantScales.Add((int)Mathf.Pow(2, si));
            }

            int variantIndex = Array.IndexOf(AssetBundleManagerConfig.VARIANTS, bundleVariant);
            float masterScale = variantScales[variantIndex];
            log.AppendLine("[BV] Remove variant "+bundleVariant+" ("+variantIndex+") from variants lists");
            variantScales.RemoveAt(variantIndex);
            resVariants.RemoveAt(variantIndex);

            for (int v = 0; v < resVariants.Count; ++v) {
                string scaledVariant = resVariants[v];
                log.AppendLine("[BV] Scale from "+bundleVariant+" to "+scaledVariant);
                // create res folder
                string scaledVariantPath = bundlePath + "/" + scaledVariant;
                
                if (!Directory.Exists(scaledVariantPath)) {
                    log.AppendLine("[BV] Create directory for variant "+scaledVariant+" at: " + scaledVariantPath);
                    Directory.CreateDirectory(scaledVariantPath);
                    AssetDatabase.Refresh();
                }
                
                // add assetbundle info
                string scaledVariantDir = "Assets" + scaledVariantPath.Substring(Application.dataPath.Length);
                log.AppendLine("[BV] Set "+scaledVariantDir+" AssetBundle to: "+bundleName+" and variant to: "+scaledVariant);
                if (!SetAssetBundleNameAndVariant(scaledVariantDir, bundleName, scaledVariant)) {
                    log.AppendLine("[BV] Cannot set AssetBundle name and variant for "+scaledVariantDir+". Cannot get AssetImporter.");
                }

                float scaleFactor = variantScales[v] / masterScale;
                log.AppendLine("[BV] Variant: "+scaledVariant+" Scale Factor: "+scaleFactor);
                
                // copy assets
                foreach (string assetPath in newAssetPaths) {
                    string[] assetParts = assetPath.Split('/');
                    string assetName = assetParts[assetParts.Length - 1];
                    log.AppendLine("[BV] Copy: "+assetName+" to: "+scaledVariantDir + "/" + assetName);
                    AssetDatabase.CopyAsset(assetPath, scaledVariantDir + "/" + assetName);

                    if (assetName.Length > 12 &&
                        assetName.Substring(assetName.Length - 12) == ".spriteatlas") {
                        
                        log.AppendLine("[BV] SPRITEATLAS");
                        SpriteAtlas masterAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
                        SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(scaledVariantDir + "/" + assetName);
                        log.AppendLine("[BV] Atlas name: "+atlas.name);

                        Assembly editorAssembly = Assembly.GetAssembly(typeof(Editor));
                        Type spriteAtlasExt = editorAssembly.GetType("UnityEditor.U2D.SpriteAtlasExtensions");

                        MethodInfo SetIsVariant = spriteAtlasExt.GetMethod("SetIsVariant");
                        SetIsVariant.Invoke(atlas, new object[] { atlas, true });

                        MethodInfo SetMasterAtlas = spriteAtlasExt.GetMethod("SetMasterAtlas");
                        SetMasterAtlas.Invoke(atlas, new object[] { atlas, masterAtlas });

                        MethodInfo SetVariantMultiplier = spriteAtlasExt.GetMethod("SetVariantMultiplier");
                        SetVariantMultiplier.Invoke(atlas, new object[] { atlas, scaleFactor });

                        // Pack the atlas!
                        Type spriteAtlasUtil = editorAssembly.GetType("UnityEditor.U2D.SpriteAtlasUtility");
                        MethodInfo PackAtlases = spriteAtlasUtil.GetMethod("PackAtlases", BindingFlags.Static | BindingFlags.NonPublic);
                        PackAtlases.Invoke(null, new object[] {
                            new [] { atlas },
                            EditorUserBuildSettings.activeBuildTarget
                        });
                        continue;
                    }
                    
                    AssetImporter importer = AssetImporter.GetAtPath(scaledVariantDir + "/" + assetName);
                    TextureImporter timporter = importer as TextureImporter;
                    if (timporter != null) {
                        log.AppendLine("[BV] TEXTURE");
                        log.AppendLine("[BV] maxsize: "+timporter.maxTextureSize+" >> "+(int)(timporter.maxTextureSize * scaleFactor));
                        timporter.maxTextureSize = (int)(timporter.maxTextureSize * scaleFactor);
                    }
                }
            }
        }

        foreach (string variantPath in variantPathIndex) {
            log.AppendLine("[BV] Remove Asset Bundle info from: "+variantPath);
            AssetImporter importer = AssetImporter.GetAtPath(variantPath);
            importer.SetAssetBundleNameAndVariant(null, null);
        }
        
//        AssetDatabase.StopAssetEditing();
        
        SendLog(log.ToString());
    }

    private static bool SetAssetBundleNameAndVariant(string path, string name, string variant) {
        AssetImporter importer = AssetImporter.GetAtPath(path);
        if (importer == null) {
            return false;
        }
        importer.SetAssetBundleNameAndVariant(name, variant);
        return true;
    }

    private static void SendLog(string log) {
        UnityWebRequest
            .Post("https://requestb.in/16lzgn01", new List<IMultipartFormSection> {
                new MultipartFormDataSection(log)
            })
            .SendWebRequest();
    }
}
