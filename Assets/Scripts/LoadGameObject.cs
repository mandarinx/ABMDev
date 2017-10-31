using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using HyperGames.AssetBundles;
using HyperGames.AssetBundlesConfig;

public class LoadGameObject : MonoBehaviour {
    
    public string bundleName;
    public string gameObjectName;

    public Button btnLoadGameObject;
    public AssetBundleManagerConfig config;

    private AssetBundleManager abman;

    private void Start() {
        btnLoadGameObject.interactable = false;
        abman = new AssetBundleManager(config, gameObject);
        StartCoroutine(Load());
    }

    private IEnumerator Load() {
        // Short version
        yield return abman.LoadMasterManifest();
        btnLoadGameObject.interactable = true;
        yield return new WaitForButtonClick(btnLoadGameObject);
        yield return abman.LoadBundle(bundleName);
        GameObject go;
        if (!abman.GetAsset(bundleName, gameObjectName, out go)) {
            Debug.Log("Could not get "+gameObjectName+" from "+bundleName);
            yield break;
        }
        Instantiate(go);
    
        // Long version
//        AssetBundleLoadStatus statusManifest = abman.LoadMasterManifest();
//        while (statusManifest.progress < 1f) {
//            Debug.Log("Loading master manifest "+statusManifest.progress);
//            if (statusManifest.error) {
//                Debug.LogError(statusManifest.errorMessage);
//                yield break;
//            }
//            yield return null;
//        }
//        Debug.Log("Loaded master manifest "+statusManifest.progress);
//        btnLoadGameObject.interactable = true;
//        yield return new WaitForButtonClick(btnLoadGameObject);
//
//        AssetBundleLoadStatus statusBundle = abman.LoadBundle(bundleName);
//        while (statusBundle.progress < 1f) {
//            Debug.Log("Loading "+bundleName+" "+statusBundle.progress);
//            if (statusBundle.error) {
//                Debug.LogError(statusBundle.errorMessage);
//                yield break;
//            }
//            yield return null;
//        }
//        Debug.Log("Loading "+bundleName+" "+statusBundle.progress);
//
//        GameObject go;
//        if (!abman.GetAsset(bundleName, gameObjectName, out go)) {
//            Debug.Log("Could not get "+gameObjectName+" from "+bundleName);
//            yield break;
//        }
//        Instantiate(go);
    }
}
