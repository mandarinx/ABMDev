using UnityEngine;
using System.Collections;
using HyperGames.AssetBundles;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class DemoPreloader : MonoBehaviour {

    public Button btnLoadManifest;
    public Slider progressBar;
    public Text doneLabel;
    public AssetBundleManagerConfig cfg;

    private AssetBundleManager abman;
    private VariantsResolver variants;
    private ILog log;

    private void Start() {
        Assert.IsNotNull(cfg, "AssetBundleConfig is null");

        log = GetComponent<LogToTextArea>() as ILog;

        doneLabel.enabled = false;
        
        variants = new VariantsResolver(cfg);
        variants.RegisterResolutionVariants();
        abman = new AssetBundleManager(cfg, gameObject, log);
        abman.AddVariantsResolver(variants);
        StartCoroutine(Load());
    }

    private IEnumerator Load() {
        yield return new WaitForButtonClick(btnLoadManifest);
        yield return abman.LoadMasterManifest();
        
        MultiBundleLoad multiLoad = new MultiBundleLoad(abman, variants, abman.GetManifestBundles());
        while (multiLoad.keepWaiting) {
            progressBar.value = multiLoad.progress;
            yield return null;
        }
        
        progressBar.value = multiLoad.progress;

        doneLabel.enabled = true;
    }
}
