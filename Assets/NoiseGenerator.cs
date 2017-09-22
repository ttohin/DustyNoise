using System.Collections;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour {

    private int iteration = 0;
    private int textureSize = 256;
    private Texture2D texture;
    private void Start () {
        this.texture = new Texture2D (textureSize, textureSize);
        var spriteTexture = new Texture2D (textureSize, textureSize);
        spriteTexture.filterMode = FilterMode.Point;
        GetComponent<SpriteRenderer> ().sprite = Sprite.Create (spriteTexture, new Rect (0, 0, textureSize, textureSize), new Vector2 (0.1f, 0.1f));
        StartCoroutine (UpdateTexturePeriodically ());
    }

    IEnumerator UpdateTexturePeriodically () {
        while (true) {
            UpdateTexture ();
            yield return new WaitForSeconds (0.01f);
        }
    }

    void UpdateTexture () {
        iteration = (iteration + 1) % textureSize;

        for (var y = 0; y < texture.height; y++)
            for (var x = 0; x < texture.width; x++) {
                var color = (x & y) != iteration ? Color.white : Color.gray;
                texture.SetPixel (x, y, color);
            }
        texture.Apply ();
        Graphics.CopyTexture (texture, GetComponent<SpriteRenderer> ().sprite.texture);
    }

    // Update is called once per frame
    private void Update () { }
}