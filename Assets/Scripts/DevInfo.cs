using UnityEngine;

public class DevInfo : MonoBehaviour, IOnGUI {

    private void Start() {
        ILog log = GetComponent<LogToTextArea>();
        if (log != null) {
            log.Log(GetInfo());
            return;
        }
        UHooks.AddOnGUI(this);
    }

    public void UOnGUI() {
        GUI.Label(new Rect(10, 10, Screen.width - 20, Screen.height - 20), GetInfo());
    }

    private string GetInfo() {
        return "[Dev Info]\n"+
            " * DPI: " + Screen.dpi;
    }
}
