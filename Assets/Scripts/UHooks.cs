using UnityEngine;
using System.Collections.Generic;

public interface IOnGUI {
    void UOnGUI();
}

public static class UHooks {

    private static GameObject hooksOwner;
    
    private static List<IOnGUI> onGUIs = new List<IOnGUI>();
    private static UOnGUI onGUI;

    public static int numOnGUIs {
        get { return onGUIs.Count; }
    }

    public static IOnGUI GetOnGUI(int n) {
        return onGUIs[n];
    }
    
    public static void AddOnGUI(IOnGUI hook) {
        if (onGUI == null) {
            if (hooksOwner == null) {
                hooksOwner = new GameObject("UHooks");
            }
            onGUI = hooksOwner.AddComponent<UOnGUI>();
        }

        if (onGUIs.Contains(hook)) {
            return;
        }

        onGUIs.Add(hook);
    }
}

public class UOnGUI : MonoBehaviour {
    
    private void OnGUI() {
        for (int i = 0; i < UHooks.numOnGUIs; ++i) {
            UHooks.GetOnGUI(i).UOnGUI();
        }
    }
}
