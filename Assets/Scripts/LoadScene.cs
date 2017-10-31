using System.Collections;
using UnityEngine;
using HyperGames.AssetBundles;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour {

    [SerializeField]
    private string bundleName;
    [SerializeField]
    private string sceneName;
    
    public Button btnLoadManifest;
    public Button btnLoadBundle;
    public AssetBundleManagerConfig cfg;
    
    private AssetBundleManager abman;

    private void Start() {
        btnLoadBundle.interactable = false;
        abman = new AssetBundleManager(cfg, gameObject);
        StartCoroutine(Load());
    }

    private IEnumerator Load() {
        yield return new WaitForButtonClick(btnLoadManifest);
        yield return abman.LoadMasterManifest();
        btnLoadManifest.interactable = false;
        btnLoadBundle.interactable = true;
        yield return new WaitForButtonClick(btnLoadBundle);
        yield return abman.LoadBundle(bundleName);
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }
}
