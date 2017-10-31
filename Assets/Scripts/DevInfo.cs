using UnityEngine;

public class DevInfo : MonoBehaviour {

    private void OnGUI() {
        GUI.Label(new Rect(10, 10, Screen.width, Screen.height), 
            "DPI: "+Screen.dpi
            );
    }
}
