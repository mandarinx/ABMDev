using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AtlasLoader))]
public class AtlasLoaderEditor : Editor {
    
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Update Sprite Atlas Name")) {
            SpriteRenderer sr = (target as AtlasLoader).GetComponent<SpriteRenderer>();
            (target as AtlasLoader).SetAtlasName(sr.sprite.texture.name);
        }
    }
}
