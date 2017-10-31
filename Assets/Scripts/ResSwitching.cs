using UnityEngine;
using HyperGames.AssetBundles;

public class ResSwitching : MonoBehaviour {

    public AssetBundleManagerConfig config;
    public int hdRes;
    
    public void SetHD() {
        config.baseDPI = hdRes;
    }

    public void SetSD() {
        config.baseDPI = (int)Screen.dpi + 1;
    }
}
