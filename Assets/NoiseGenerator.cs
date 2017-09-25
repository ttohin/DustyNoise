using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NoiseGenerator : MonoBehaviour {

    [Range (0, 0.5f)]
    public float Speed = 0.02f;
    [Range (-2, 2)]
    public float RandDelta = -1.0f;
    [Range (1, 100)]
    public float Roughness = 20.0f;
    private int noizeSize = 512;
    private int textureSize = 480;
    private int textureHeight;
    private int textureWidth;
    private Texture2D texture;
    private DiamondSquareGenerator noise;
    public Color color1 = Color.red;
    public Color color2 = Color.blue;
    public Color darkColor = Color.black;

    [Range (1, 50)]
    public int Layers = 16;

    [Range (1, 50)]
    public int ColorLayers = 8;
    private void Start () {

        Color darkColor1 = new Color32 (27, 5, 0, 255);
        Color darkColor2 = new Color32 (27, 46, 42, 255);
        darkColor = Random.value > 0.5f ? darkColor1 : darkColor2;

        var colors = new Color[] {
            new Color32 (37, 105, 121, 255),
            new Color32 (157, 117, 68, 255),
            new Color32 (154, 192, 203, 255),
            new Color32 (255, 255, 255, 255),
        };

        int colorIndex1 = (int) Mathf.Floor (Random.value * colors.Length);
        int colorIndex2 = colorIndex1;
        while (colorIndex1 == colorIndex2)
            colorIndex2 = (int) Mathf.Floor (Random.value * colors.Length);

        color1 = colors[colorIndex1];
        color2 = colors[colorIndex2];

        if (Screen.width > Screen.height) {
            float ratio = (float) Screen.height / (float) Screen.width;
            textureWidth = textureSize;
            textureHeight = (int) ((float) textureSize * ratio);
        } else {
            float ratio = (float) Screen.width / (float) Screen.height;
            textureHeight = textureSize;
            textureWidth = (int) ((float) textureSize * ratio);
        }

        texture = new Texture2D (textureWidth, textureHeight);
        var spriteTexture = new Texture2D (textureWidth, textureHeight);
        spriteTexture.filterMode = FilterMode.Point;
        GetComponent<SpriteRenderer> ().sprite = Sprite.Create (spriteTexture,
            new Rect (0, 0, textureWidth, textureHeight),
            new Vector2 (0.0f, 0.0f));
        noise = new DiamondSquareGenerator (noizeSize, noizeSize, Roughness, RandDelta, true);
        noise.Generate ();

        transform.position = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0, 10.0f));
        var screenSize = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height, 0.0f));
        if (screenSize.x > screenSize.y) {
            float scale = 2 * screenSize.x / GetComponent<SpriteRenderer> ().sprite.bounds.size.x;
            transform.localScale = new Vector3 (scale, scale, 1);
        } else {
            float scale = 2 * screenSize.y / GetComponent<SpriteRenderer> ().sprite.bounds.size.y;
            transform.localScale = new Vector3 (scale, scale, 1);
        }

    }

    IEnumerator UpdateTexturePeriodically () {
        while (true) {
            UpdateTexture ();
            yield return new WaitForSeconds (0.01f);
        }
    }

    void UpdateTexture () {

        float valueTimeShift = Time.deltaTime * Speed;

        for (int i = 0; i < textureWidth; i++) {

            for (int j = 0; j < textureHeight; j++) {

                float value = noise.data.Get (i, j);
                float resultValue = value + valueTimeShift;
                if (resultValue < 0.0f)
                    resultValue = -resultValue;
                if (resultValue > 1.0f)
                    resultValue = resultValue - 1.0f;

                Color c = CreateColorForValue (value);

                noise.data.Set (resultValue, i, j);

                texture.SetPixel (i, j, c);
            }
        }

        texture.Apply ();
        Graphics.CopyTexture (texture, GetComponent<SpriteRenderer> ().sprite.texture);
    }

    static private Color mixColors (Color color1, Color color2, float ratio) {
        return color1 * ratio + color2 * (1.0f - ratio);
    }

    private Color CreateColorForValue (float value) {

        float levelOfDarkness = GetValueFromStep (value, Layers);
        float colorGradient = GetValueFromStep (value, ColorLayers);

        Color color = mixColors (color1, color2, colorGradient);
        return mixColors (color, darkColor, levelOfDarkness);
    }

    private float GetValueFromStep (float value, int steps) {
        float scaledValue = value * steps;
        return Mathf.Ceil (scaledValue) - scaledValue;
    }

    // Update is called once per frame
    private void Update () {
        if (Input.GetKeyDown (KeyCode.R)) {
            SceneManager.LoadScene ("MainScene");
        }
        if (Input.GetKeyDown (KeyCode.Q)) {
            Application.Quit ();
        }
        UpdateTexture ();
    }
}