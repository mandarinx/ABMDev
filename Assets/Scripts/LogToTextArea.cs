using UnityEngine;
using UnityEngine.UI;
using System.Text;

public interface ILog {
    void Log(string str);
}

public class LogToTextArea : MonoBehaviour, ILog {

    public Text logArea;
    private StringBuilder log;

    void Awake() {
        log = new StringBuilder();
    }
    
    public void Log(string str) {
        log.AppendLine(str);
        logArea.text = log.ToString();
    }
}
