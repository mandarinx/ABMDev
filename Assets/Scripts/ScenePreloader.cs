using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePreloader : MonoBehaviour {

    [SerializeField]
    private string activeScene;
    [SerializeField]
    private string[] sceneNames;
    private Scene[] scenes;
    private AsyncOperation[] operations;
    private float progress;
    private int current;
    
    void Start() {
        operations = new AsyncOperation[sceneNames.Length];
        scenes = new Scene[sceneNames.Length];
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        for (int i = 0; i < sceneNames.Length; ++i) {
            operations[i] = SceneManager.LoadSceneAsync(sceneNames[i], LoadSceneMode.Additive);
        }
    }

    private void Update() {
        float p = 0f;
        for (int i = 0; i < operations.Length; ++i) {
            p += operations[i].progress;
        }
        progress = p / operations.Length;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; ++i) {
            int si = Array.IndexOf(sceneNames, scene.name);
            scenes[si] = scene;

            if (scene.name == activeScene) {
                current = si;
            }

            SceneLoaderCallbacks cb = rootObjects[i].GetComponent<SceneLoaderCallbacks>();
            if (cb == null) {
                continue;
            }
            
            cb.SceneLoaded(scene);
            
            if (cb.gameObject.scene.name != activeScene) {
                cb.SceneDeactivated(scene);
            }
        }
    }

    private void OnGUI() {
        GUI.Label(new Rect(10, 10, 50, 20), progress.ToString("0.00"));
    }

    public void NextScene() {
        ++current;
        if (current >= sceneNames.Length) {
            current = sceneNames.Length - 1;
        } else {
            
        }
    }

    public void PreviousScene() {
    }
}
