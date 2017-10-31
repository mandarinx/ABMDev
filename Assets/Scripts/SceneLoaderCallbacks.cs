using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoaderCallbacks : MonoBehaviour {

    public UnityEvent onSceneLoaded;
    public UnityEvent onSceneUnloaded;
    public UnityEvent onSceneActivated;
    public UnityEvent onSceneDeactivated;

    public void SceneLoaded(Scene scene) {
        if (gameObject.scene != scene) {
            return;
        }
        onSceneLoaded.Invoke();
    }

    public void SceneUnloaded(Scene scene) {
        if (gameObject.scene != scene) {
            return;
        }
        onSceneUnloaded.Invoke();
    }

    public void SceneActivated(Scene scene) {
        if (gameObject.scene != scene) {
            return;
        }
        onSceneActivated.Invoke();
    }

    public void SceneDeactivated(Scene scene) {
        if (gameObject.scene != scene) {
            return;
        }
        onSceneDeactivated.Invoke();
    }
    
}
