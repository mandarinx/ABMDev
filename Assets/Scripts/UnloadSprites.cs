using System.Collections.Generic;
using UnityEngine;

public class UnloadSprites : MonoBehaviour {

    private List<SpriteRenderer> renderers;

    void Awake() {
        SpriteRenderer[] sr = FindObjectsOfType<SpriteRenderer>();
        renderers = new List<SpriteRenderer>();
        for (int i = 0; i < sr.Length; ++i) {
            if (sr[i].gameObject.scene != gameObject.scene) {
                continue;
            }
            renderers.Add(sr[i]);
        }
    }
    
    public void UnloadAll() {
        Debug.Log("Unload "+renderers.Count+" sprites from scene "+gameObject.scene.name);
        for (int i = 0; i < renderers.Count; ++i) {
            renderers[i].sprite = null;
        }
    }
}
