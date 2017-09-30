﻿using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum MouseMode {
    Simple,
    Overload,
    QuantumBubles,
    BrokenLens,
}

public class NoiseGenerator : MonoBehaviour {

    [Range (-1, 1)]
    public float Speed = 0.005f;
    [Range (-2, 2)]
    public float RandDelta = -1.0f;
    [Range (1, 100)]
    public float Roughness = 20.0f;
    private int noizeSize = 1024;
    private int drawingTextureSize = 256;
    private int drawingTextureHeight;
    private int drawingTextureWidth;
    private Texture2D drawingTexture;
    private int textureWidth;
    private int textureHeight;
    private int verticalOffset = 0;
    private int horizontalOffset = 0;

    [Range (1, 50)]
    public int Layers = 12;

    [Range (1, 50)]
    public int ColorLayers = 3;
    private float offset = 0.0f;
    private Vector2? mousePosition;
    private bool isLeftMouseButton = true;
    private bool mouseIsDown = false;
    public MouseMode mouseMode = MouseMode.QuantumBubles;
    public GameObject speedSlider;
    public GameObject colorsSlider;
    public GameObject layersSlider;
    public GameObject brushSelector;
    private void Start () {

        float horizontalScreenRatio = 1;
        float verticalScreenRatio = 1;
        if (Screen.width > Screen.height) {
            horizontalScreenRatio = (float) Screen.height / (float) Screen.width;
            textureWidth = noizeSize;
            textureHeight = (int) ((float) noizeSize * horizontalScreenRatio);
            verticalOffset = (noizeSize - textureHeight) / 2;
        } else {
            verticalScreenRatio = (float) Screen.width / (float) Screen.height;
            textureHeight = noizeSize;
            textureWidth = (int) ((float) noizeSize * verticalScreenRatio);
            horizontalOffset = (noizeSize - textureWidth) / 2;
        }

        drawingTextureHeight = (int) (drawingTextureSize * horizontalScreenRatio);
        drawingTextureWidth = (int) (drawingTextureSize * verticalScreenRatio);
        drawingTexture = new Texture2D (drawingTextureWidth, drawingTextureHeight);
        drawingTexture.filterMode = FilterMode.Trilinear;
        for (int i = 0; i < drawingTextureWidth; i++) {
            for (int j = 0; j < drawingTextureHeight; j++) {
                drawingTexture.SetPixel (i, j, new Color (0.5f, 0.5f, 0.5f));
            }
        }
        drawingTexture.Apply ();
        GetComponent<Renderer> ().material.SetTexture (name: "_DrawingTex", value : drawingTexture);

        var quadHeight = Camera.main.orthographicSize * 2.0;
        var quadWidth = quadHeight * Screen.width / Screen.height;
        transform.localScale = new Vector3 ((float) quadWidth, (float) quadHeight, 1.0f);

        GenerateRandomValues ();
    }

    void GenerateRandomValues () {
        mouseMode = (MouseMode) (Random.value * System.Enum.GetNames (typeof (MouseMode)).Length);

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
        var texture = new Texture2D (textureWidth, textureHeight);
        texture.filterMode = FilterMode.Point;

        var noise = new DiamondSquareGenerator (noizeSize, noizeSize, Roughness, RandDelta, true);
        noise.Generate ();
        noise.data.ForEach ((value, x, y) => {
            noise.data.Set (GetValueFromStep (value, 6), x, y);
        });

        for (int i = 0; i < textureWidth; i++) {

            for (int j = 0; j < textureHeight; j++) {

                float value = noise.data.Get (i + horizontalOffset, j + verticalOffset);
                Color c = new Color (value, value, value);
                texture.SetPixel (i, j, c);
            }
        }

        texture.Apply (false, true);
        GetComponent<Renderer> ().material.mainTexture = texture;
    }

    void updateUIControls () {
        Speed = 0.1f;

        speedSlider.GetComponent<Slider> ().value = Speed;
        layersSlider.GetComponent<Slider> ().value = Layers;
        colorsSlider.GetComponent<Slider> ().value = ColorLayers;
    }

    IEnumerator UpdateTexturePeriodically () {
        while (true) {
            UpdateTexture ();
            yield return new WaitForSeconds (0.0005f);
        }
    }
    private float GetValueFromStep (float value, float steps) {
        float scaledValue = value * steps;
        return Mathf.Ceil (scaledValue) - scaledValue;
    }

    void UpdateTexture () {
        if (!mouseIsDown)
            return;

        Vector2 mousePosInTexture = new Vector2 (
            mousePosition.Value.x * drawingTextureWidth,
            mousePosition.Value.y * drawingTextureHeight
        );

        for (int i = 0; i < drawingTextureWidth; i++) {
            for (int j = 0; j < drawingTextureHeight; j++) {
                if (mouseMode == MouseMode.QuantumBubles)
                    QuantumBublesMouseMode (mousePosInTexture, drawingTexture, i, j, isLeftMouseButton);
                else if (mouseMode == MouseMode.Overload)
                    OverloadMouseMode (mousePosInTexture, drawingTexture, i, j, isLeftMouseButton);
                else if (mouseMode == MouseMode.Simple)
                    SimpleMouseMode (mousePosInTexture, drawingTexture, i, j, isLeftMouseButton);
                else if (mouseMode == MouseMode.BrokenLens)
                    BrokenLensMouseMode (mousePosInTexture, drawingTexture, i, j, isLeftMouseButton);
            }
        }

        drawingTexture.Apply ();

    }
    static void BrokenLensMouseMode (Vector2 mousePosInTexture, Texture2D texture, int i, int j, bool isLeftMouseButton) {
        Vector2 pos = new Vector2 (i, j);
        Vector2 distance = pos - mousePosInTexture;

        float maxDistance = 800.0f;
        if (distance.sqrMagnitude > maxDistance)
            return;

        float ratio = (maxDistance - distance.sqrMagnitude) / maxDistance;
        ratio = ratio * ratio * ratio;

        var colorsAround = new Color[] {
            texture.GetPixel (i + 0, j + 0),
            texture.GetPixel (i + 1, j + 1),
            texture.GetPixel (i + 1, j + 0),
            texture.GetPixel (i + 1, j - 1),
            texture.GetPixel (i - 1, j + 1),
            texture.GetPixel (i - 1, j + 0),
            texture.GetPixel (i - 1, j - 1),
            texture.GetPixel (i + 0, j - 1),
            texture.GetPixel (i + 0, j + 1),
        };

        var colorsSum = 0.0f;
        foreach (var item in colorsAround) {
            colorsSum += item.r;
        }

        float lensValue = 0;
        if (isLeftMouseButton)
            lensValue = colorsAround[0].r * 0.9f + ratio * colorsSum / colorsAround.Length;
        else
            lensValue = colorsAround[0].r * (1.0f - ratio) + ratio * (-0.5f + Random.value * 1.5f);

        float value = lensValue;

        texture.SetPixel (i, j, new Color (value, value, value));
    }

    static void QuantumBublesMouseMode (Vector2 mousePosInTexture, Texture2D texture, int i, int j, bool isLeftMouseButton) {
        Vector2 pos = new Vector2 (i, j);
        Vector2 distance = pos - mousePosInTexture;

        float maxDistance = 800.0f;
        if (distance.sqrMagnitude > maxDistance)
            return;

        float ratio = (maxDistance - distance.sqrMagnitude) / maxDistance;
        ratio = 3 * ratio * ratio;

        var color = texture.GetPixel (i, j);
        var colorsAround = new Color[] {
            texture.GetPixel (i + 1, j + 1),
            texture.GetPixel (i + 1, j + 0),
            texture.GetPixel (i + 1, j - 1),
            texture.GetPixel (i - 1, j + 1),
            texture.GetPixel (i - 1, j + 0),
            texture.GetPixel (i - 1, j - 1),
            texture.GetPixel (i + 0, j - 1),
            texture.GetPixel (i + 0, j + 1),
        };

        var colorsSum = 0.0f;
        foreach (var item in colorsAround) {
            colorsSum += item.r;
        }

        float value = color.r + ratio * (isLeftMouseButton ? 0.005f : -0.005f);
        value = value * 0.8f + (colorsSum / colorsAround.Length) * 0.2f;

        if (value > 1) value = 0;
        if (value < 0) value = 1;

        texture.SetPixel (i, j, new Color (value, value, value));
    }

    static void SimpleMouseMode (Vector2 mousePosInTexture, Texture2D texture, int i, int j, bool isLeftMouseButton) {
        Vector2 pos = new Vector2 (i, j);
        Vector2 distance = pos - mousePosInTexture;

        float maxDistance = 800.0f;
        if (distance.sqrMagnitude > maxDistance)
            return;

        float ratio = (maxDistance - distance.sqrMagnitude) / maxDistance;
        ratio = 3 * ratio * ratio;

        var color = texture.GetPixel (i, j);
        float value = color.r + ratio * (isLeftMouseButton ? 0.005f : -0.005f);

        if (value > 1) value = 1;
        if (value < 0) value = 0;

        texture.SetPixel (i, j, new Color (value, value, value));
    }
    static void OverloadMouseMode (Vector2 mousePosInTexture, Texture2D texture, int i, int j, bool isLeftMouseButton) {
        Vector2 pos = new Vector2 (i, j);
        Vector2 distance = pos - mousePosInTexture;

        float maxDistance = 800.0f;
        if (distance.sqrMagnitude > maxDistance)
            return;

        float ratio = (maxDistance - distance.sqrMagnitude) / maxDistance;
        ratio = 3 * ratio * ratio;

        var color = texture.GetPixel (i, j);
        float value = color.r + ratio * (isLeftMouseButton ? 0.005f : -0.005f);

        if (value > 1) value = value - 1;
        if (value < 0) value = 1 - value;

        texture.SetPixel (i, j, new Color (value, value, value));
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
            GenerateRandomValues ();
        }
        if (Input.GetKeyDown (KeyCode.Q)) {
            Application.Quit ();
        }
        if (!EventSystem.current.IsPointerOverGameObject ()) {
            if (Input.GetMouseButtonDown (0)) {
                isLeftMouseButton = true;
                mouseIsDown = true;
            }
            if (Input.GetMouseButtonDown (1)) {
                isLeftMouseButton = false;
                mouseIsDown = true;
            }
        }
        mousePosition = new Vector2 (
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );
        if (Input.GetMouseButtonUp (0) || Input.GetMouseButtonUp (1)) {
            mouseIsDown = false;
        }
        UpdateTexture ();
        UpdateShaderParams ();
    }

    public void OnMouseModeChanges (int mode) {
        mouseMode = (MouseMode) mode;
    }

    public void OnSpeedChanged (float value) {
        Speed = value;
    }
    public void OnColorsChanged (float value) {
        ColorLayers = (int) value;
        GetComponent<Renderer> ().material.SetFloat (name: "_ColorSteps", value : ColorLayers);
    }
    public void OnLayersChanged (float value) {
        Layers = (int) value;
        GetComponent<Renderer> ().material.SetFloat (name: "_DarkSteps", value : Layers);
    }
}