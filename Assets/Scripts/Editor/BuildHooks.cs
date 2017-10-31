using HyperGames.AssetBundles;
using UnityEditor;
using UnityEngine;
using ABM;

public class BuildHooks {
    
    [MenuItem("Dev/PreBuild")]
    public static void PreBuild() {
        LoadSO<AssetBundleManagerConfig>("Assets/Configs/AssetBundleManagerConfig.asset");
        LoadSO<ABMConfig>("Assets/Configs/ABMConfig.asset");
        
//        TestSOLoading.LoadSO();
//        VariantsBuilder.BuildVariants();
    }

    public static void PostBuild() {
    }

    private static void LoadSO<T>(string path) where T : ScriptableObject {
        Debug.Log("[SOTEST] " + 
                  (AssetDatabase.LoadAssetAtPath<T>(path) != null 
                      ? "Could" 
                      : "Couldn't") + 
                  " load " + path);
    }
}
