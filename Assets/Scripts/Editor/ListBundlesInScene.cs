using UnityEngine;
using UnityEditor;

public class ListBundlesInScene {

    [MenuItem("Dev/List Asset Bundles in Scene")]
    public static void List() {
        SpriteRenderer[] renderers = Object.FindObjectsOfType<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; ++i) {
            string path = AssetDatabase.GetAssetPath(renderers[i].sprite);
            string assetBundleName = AssetDatabase.GetImplicitAssetBundleName(path);
            Debug.Log(renderers[i].sprite.name + " belongs to asset bundle "+assetBundleName);
        }
    }
}
