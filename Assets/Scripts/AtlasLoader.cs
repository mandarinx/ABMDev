using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteRenderer))]
public class AtlasLoader : MonoBehaviour {

    public SpriteAtlas atlas;

    [SerializeField]
    private string spriteAtlasName = "N/A";
    
    public void SetAtlasName(string atlasName) {
        spriteAtlasName = atlasName;
    }

    void OnEnable() {
        SpriteAtlasManager.atlasRequested += RequestAtlas;
    }

    void OnDisable() {
        SpriteAtlasManager.atlasRequested -= RequestAtlas;
    }

    void RequestAtlas(string tag, System.Action<SpriteAtlas> callback) {
        Debug.Log("RequestAtlas tag: "+tag);
//        callback(atlas);
    }
}
