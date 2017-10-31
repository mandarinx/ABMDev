using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using HyperGames.AssetBundles;
using HyperGames.AssetBundlesConfig;

public class LoadGameObjectCoroutines : MonoBehaviour {
    
    public string bundleName;
    public string gameObjectName;

    public Button btn;
    public AssetBundleManagerConfig cfg;

    private AssetBundleManager abman;

    private void Start() {
        btn.interactable = false;
        abman = new AssetBundleManager(cfg, gameObject);
        StartCoroutine(Load());
    }

    private IEnumerator Load() {
        yield return abman.LoadMasterManifest();
        btn.interactable = true;
        yield return new WaitForButtonClick(btn);
        Debug.Log("Load bundle "+bundleName);
        yield return abman.LoadBundle(bundleName);
        
        GameObject go;
        if (!abman.GetAsset(bundleName, gameObjectName, out go)) {
            Debug.Log("Could not get "+gameObjectName+" from "+bundleName);
            yield break;
        }
        Instantiate(go);
    }
}
