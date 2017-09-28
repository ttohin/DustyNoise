using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NoiseGenerator : MonoBehaviour {

    [Range (-1, 1)]
    public float Speed = 0.005f;
    [Range (-2, 2)]
    public float RandDelta = -1.0f;
    [Range (1, 100)]
    public float Roughness = 20.0f;
    private int noizeSize = 1024;
    private int textureSize = 1024;
    private int textureHeight;
    private int textureWidth;
    private Texture2D texture;

    [Range (1, 50)]
    public int Layers = 12;

    [Range (1, 50)]
    public int ColorLayers = 3;
    private float offset = 0.0f;
    private Vector2? mousePosition;
    private float mouseValue = 0.0f;
    private bool mouseIsDown = false;
    private void Start () {

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

        var color1 = colors[colorIndex1];
        var color2 = colors[colorIndex2];
        var darkColor = color1 * (0.2f + Random.value * 0.3f);

        GetComponent<Renderer> ().material.SetColor (name: "_Color1", value : color1);
        GetComponent<Renderer> ().material.SetColor (name: "_Color2", value : color2);
        GetComponent<Renderer> ().material.SetColor (name: "_DarkColor", value : darkColor);

        ColorLayers = 1 + (int) Mathf.Floor (Random.value * 2);
        Layers = ColorLayers * (2 + (int) Mathf.Floor (Random.value * 5));
        GetComponent<Renderer> ().material.SetFloat (name: "_DarkSteps", value : Layers);
        GetComponent<Renderer> ().material.SetFloat (name: "_ColorSteps", value : ColorLayers);

        int verticalOffset = 0;
        int horizontalOffset = 0;
        if (Screen.width > Screen.height) {
            float ratio = (float) Screen.height / (float) Screen.width;
            textureWidth = textureSize;
            textureHeight = (int) ((float) textureSize * ratio);
            verticalOffset = (textureSize - textureHeight) / 2;
        } else {
            float ratio = (float) Screen.width / (float) Screen.height;
            textureHeight = textureSize;
            textureWidth = (int) ((float) textureSize * ratio);
            horizontalOffset = (textureSize - textureWidth) / 2;
        }

        var noise = new DiamondSquareGenerator (noizeSize, noizeSize, Roughness, RandDelta, true);
        noise.Generate ();
        //noise.data.Fill(0.0f);

        noise.data.ForEach ((value, x, y) => {
            var scaledValue = value * 6;
            var normilizedValue = Mathf.Ceil (scaledValue) - scaledValue;
            noise.data.Set (normilizedValue, x, y);
        });

        texture = new Texture2D (textureWidth, textureHeight);
        texture.filterMode = FilterMode.Point;
        for (int i = 0; i < textureWidth; i++) {

            for (int j = 0; j < textureHeight; j++) {

                float value = noise.data.Get (i + horizontalOffset, j + verticalOffset);
                Color c = new Color (value, value, value);
                texture.SetPixel (i, j, c);
            }
        }

        texture.Apply (true, false);

        GetComponent<Renderer> ().material.mainTexture = texture;

        var quadHeight = Camera.main.orthographicSize * 2.0;
        var quadWidth = quadHeight * Screen.width / Screen.height;
        transform.localScale = new Vector3 ((float) quadWidth, (float) quadHeight, 1.0f);
    }

    IEnumerator UpdateTexturePeriodically () {
        while (true) {
            UpdateTexture ();
            yield return new WaitForSeconds (0.0005f);
        }
    }

    void UpdateTexture () {
        if (!mouseIsDown)
            return;

        Vector2 mousePosInTexture = new Vector2 (
            mousePosition.Value.x * textureWidth,
            mousePosition.Value.y * textureHeight
        );

        for (int i = 0; i < textureWidth; i++) {
            for (int j = 0; j < textureHeight; j++) {

                Vector2 pos = new Vector2(i, j);
                Vector2 distance = pos - mousePosInTexture;

                float maxDistance = 8000.0f;
                if (distance.sqrMagnitude > maxDistance)
                    continue;

                float ratio = (maxDistance - distance.sqrMagnitude) / maxDistance;
                ratio = ratio * ratio;
                
                var color = texture.GetPixel(i, j);

                float value = ratio * mouseValue;
                value = color.r + value;

                texture.SetPixel(i, j, new Color(value, value, value));
            }
        }

        texture.Apply();

    }

    void UpdateShaderParams () {
        offset = offset + Time.deltaTime * Speed;
        if (offset > 1)
            offset = 0;
        if (offset < 0)
            offset = 1;
        GetComponent<Renderer> ().material.SetFloat (name: "_Offset", value : offset);
    }

    private void Update () {
        if (Input.GetKeyDown (KeyCode.R)) {
            SceneManager.LoadScene ("MainScene");
        }
        if (Input.GetKeyDown (KeyCode.Q)) {
            Application.Quit ();
        }
        if (Input.GetMouseButtonDown (0)) {
            mouseValue = 0.1f;
            mouseIsDown = true;
        }
        if (Input.GetMouseButtonDown (1)) {
            mouseValue = -0.1f;
            mouseIsDown = true;
        }
        mousePosition = new Vector2 (
                Input.mousePosition.x / Screen.width,
                Input.mousePosition.y / Screen.height
            );
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            mouseIsDown = false;
        }
        UpdateTexture ();
        UpdateShaderParams ();
    }
}