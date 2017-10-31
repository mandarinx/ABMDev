using System.Collections;
using UnityEngine;
using HyperGames.AssetBundles;
using HyperGames.AssetBundlesConfig;
using UnityEngine.Assertions;
using UnityEngine.U2D;
using UnityEngine.UI;

public class LoadVariant : MonoBehaviour {

    [SerializeField]
    private string bundleName;
    
    public Button btnLoadManifest;
    public Button btnLoadBundle;
    public AssetBundleManagerConfig cfg;
    
    private AssetBundleManager abman;

    private void Start() {
        Assert.IsNotNull(cfg, "AssetBundleConfig is null");
        Assert.IsNotNull(cfg.resolutionVariants, "AssetBundleConfig.resolutionVariants is null");
        
        btnLoadBundle.interactable = false;
        VariantsResolver variants = new VariantsResolver(cfg);
        variants.RegisterResolutionVariants();
        abman = new AssetBundleManager(cfg, gameObject);
        abman.AddVariantsResolver(variants);
        StartCoroutine(Load());
    }

    private IEnumerator Load() {
        yield return new WaitForButtonClick(btnLoadManifest);
        yield return abman.LoadMasterManifest();
        btnLoadManifest.interactable = false;
        btnLoadBundle.interactable = true;
        yield return new WaitForButtonClick(btnLoadBundle);
        
        AssetBundleLoadStatus bundleStatus = abman.LoadBundle(bundleName);
        while (bundleStatus.progress < 1f) {
            Debug.Log("PROGRESS: "+bundleStatus.progress);
            if (bundleStatus.error) {
                Debug.LogWarning(bundleStatus.errorMessage);
                yield break;
            }
            yield return null;
        }

        Debug.Log("Asset Bundle loaded, get assets");
        GameObject redhead = new GameObject("Redhead");
        SpriteAtlas atlas;
        if (!abman.GetAsset(bundleName, "redhead", out atlas)) {
            Debug.Log("Cannot find SpriteAtlas redhead in "+bundleName);
        }
        redhead.AddComponent<SpriteRenderer>().sprite = atlas.GetSprite("redhead-talking-3");
    }
}
