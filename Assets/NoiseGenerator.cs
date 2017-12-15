using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
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

class Palette {
    public int ColorIndex1;
    public int ColorIndex2;
    public int ColorIndex3;
    public Palette (int colorIndex1) {
        ColorIndex1 = colorIndex1;
        ColorIndex2 = colorIndex1 + 1;
        ColorIndex3 = colorIndex1;
    }
    public Palette (int colorIndex1, int colorIndex2, int colorIndex3) {
        ColorIndex1 = colorIndex1;
        ColorIndex2 = colorIndex2;
        ColorIndex3 = colorIndex3;
    }
    public Palette (Palette palette) {
        ColorIndex1 = palette.ColorIndex1;
        ColorIndex2 = palette.ColorIndex2;
        ColorIndex3 = palette.ColorIndex3;
    }
}

public class NoiseGenerator : MonoBehaviour {

    private Material currentMaterial;
    public RawImage cameraImage;
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
    public MouseMode mouseMode = MouseMode.Simple;
    public GameObject speedSlider;
    public GameObject colorsSlider;
    public GameObject layersSlider;
    public GameObject brushSelector;
    public GameObject paletteSlider;
    public GameObject Color1Slider;
    public GameObject Color2Slider;
    public GameObject Color3Slider;
    private Palette currentPalette;
    private int paletteIndex;
    private Palette[] palettes;
    private Color[] colors;
    public ToggleButton mainMenuButton;
    private System.DateTime? restartDate;
    private bool timerIsEnagled = false;
    private bool useCamera = false;
    private WebCamTexture cameraTexture;

    public Dropdown CameraSelector;
    private int cameraIndex;

    static private Palette CreatePaletteWithDiffrentColors (Color[] colors) {
        var palette = new Palette (0, 0, 0);
        palette.ColorIndex1 = Random.Range (0, colors.Length);

        palette.ColorIndex2 = palette.ColorIndex1;
        while (palette.ColorIndex1 == palette.ColorIndex2)
            palette.ColorIndex2 = Random.Range (0, colors.Length);

        palette.ColorIndex3 = palette.ColorIndex1;
        while (palette.ColorIndex3 == palette.ColorIndex1 || palette.ColorIndex3 == palette.ColorIndex1)
            palette.ColorIndex3 = Random.Range (0, colors.Length);

        return palette;
    }
    static private Palette CreateRandomPalette (Color[] colors) {
        var palette = new Palette (0, 0, 0);
        palette.ColorIndex1 = Random.Range (0, colors.Length);
        if (Random.value < 0.5f) {
            palette.ColorIndex2 = palette.ColorIndex1;
            while (palette.ColorIndex1 == palette.ColorIndex2)
                palette.ColorIndex2 = Random.Range (0, colors.Length);
            palette.ColorIndex3 = palette.ColorIndex1;
        } else if (Random.value < 0.8f) {
            return CreatePaletteWithDiffrentColors (colors);
        } else {
            palette.ColorIndex2 = palette.ColorIndex1;
            palette.ColorIndex3 = palette.ColorIndex1;
        }

        return palette;
    }

    private IEnumerator RestartTimer () {
        while (true) {
            if (restartDate == null || restartDate.Value > System.DateTime.Now) {
                yield return new WaitForSeconds (5);
            } else {
                ScheduleRestart ();
                GenerateRandomValues ();
            }

        }
    }

    public void ScheduleRestart () {
        restartDate = System.DateTime.Now + System.TimeSpan.FromMinutes (5);
    }

    public void EnableRestartTimer (bool enable) {
        if (enable)
            ScheduleRestart ();
        else
            restartDate = null;
    }

    private void InitializeCamera () {

        if (cameraIndex == 0)
        {
            Debug.Log("disabling camera");
            if (cameraTexture != null)
            {
                cameraTexture.Stop();
                cameraTexture = null;
            }

            cameraIndex = 0;
            currentMaterial.SetTexture ("_AdditionalTex", null);
            return;
        }

        if (WebCamTexture.devices.Length <= (cameraIndex - 1))
        {
            cameraIndex = 0;
            UpdateCameras();
            return;
        }

        if (WebCamTexture.devices.Length == 0) {
            Debug.Log("No devices cameras found");
            return;
        }

        Debug.Log("starting camera");
        var cameraDevice = WebCamTexture.devices[cameraIndex - 1];
        cameraTexture = new WebCamTexture (cameraDevice.name);
        cameraTexture.Play ();
        currentMaterial.SetTexture ("_AdditionalTex", cameraTexture);
    }

    private void UpdateCameras()
    {
        string[] cameraNames = WebCamTexture.devices.Select((camDevice) => camDevice.name).ToArray();
        var options = cameraNames.Select((name) => new Dropdown.OptionData(name)).ToList();
        options.Insert(0, new Dropdown.OptionData("none"));
        CameraSelector.options = options;
        CameraSelector.value = 0;
    }

    private void Start () {

        currentMaterial = cameraImage.material;

        UpdateCameras();

        colors = new Color[] {
            hexToColor ("#AB584F"),
            hexToColor ("#88A14B"),
            hexToColor ("#843D6B"),
            hexToColor ("#3C814B"),
            hexToColor ("#AB824F"),
            hexToColor ("#36596D"),
            hexToColor ("#454077"), // 6
            hexToColor ("#ABAA4F"),
            hexToColor ("#5C3972"), // 8 
            hexToColor ("#7B9D49"),
            hexToColor ("#904366"),
            hexToColor ("#377659"),
            hexToColor ("#AB684F"),
            hexToColor ("#627082"),
            hexToColor ("#648978"),
            hexToColor ("#1B2634"),
            hexToColor ("#4F3025"), // 16
            hexToColor ("#CCCCCC"), // 17
        };

        Color1Slider.GetComponent<Slider> ().maxValue = colors.Length;
        Color2Slider.GetComponent<Slider> ().maxValue = colors.Length;
        Color3Slider.GetComponent<Slider> ().maxValue = colors.Length;

        palettes = new Palette[] {
            new Palette (0, 2, 15),
            new Palette (1, 3, 7),
            new Palette (17, 5, 17),
            new Palette (4, 5, 0),
            new Palette (6, 10, 6),
            new Palette (6, 7, 3),
            new Palette (10, 9, 10),
            new Palette (11, 9, 11),
            new Palette (0, 2, 16),
            new Palette (1, 3, 15),
            new Palette (4, 5, 14),
            new Palette (4, 5, 13),
        };

        paletteSlider.GetComponent<Slider> ().maxValue = palettes.Length;

        if (Application.platform == RuntimePlatform.OSXPlayer && Screen.width > 1440)
            GameObject.Find ("Canvas").GetComponent<CanvasScaler> ().scaleFactor = Screen.width / 1440.0f;

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
        resetDrawingTexture ();

        var quadHeight = Camera.main.orthographicSize * 2.0;
        var quadWidth = quadHeight * Screen.width / Screen.height;
        transform.localScale = new Vector3 ((float) quadWidth, (float) quadHeight, 1.0f);

        GenerateRandomValues ();
        currentMaterial.SetTexture (name: "_DrawingTex", value: drawingTexture);
        StartCoroutine (RestartTimer ());
    }

    private void resetDrawingTexture () {
        for (int i = 0; i < drawingTextureWidth; i++) {
            for (int j = 0; j < drawingTextureHeight; j++) {
                drawingTexture.SetPixel (i, j, new Color (0.5f, 0.5f, 0.5f));
            }
        }
        drawingTexture.Apply ();
    }

    public static Color hexToColor (string hex) {
        hex = hex.Replace ("0x", ""); //in case the string is formatted 0xFFFFFF
        hex = hex.Replace ("#", ""); //in case the string is formatted #FFFFFF
        byte a = 255; //assume fully visible unless specified in hex
        byte r = byte.Parse (hex.Substring (0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse (hex.Substring (2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse (hex.Substring (4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8) {
            a = byte.Parse (hex.Substring (6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32 (r, g, b, a);
    }

    public void RandomizeColors () {
        currentPalette = CreatePaletteWithDiffrentColors (colors);
        updateColorSliders ();
    }

    public void GenerateRandomValues () {

        mouseMode = (MouseMode) (Random.value * System.Enum.GetNames (typeof (MouseMode)).Length);

        paletteIndex = (int) Mathf.Floor (Random.value * (palettes.Length - 1));
        currentPalette = new Palette (palettes[paletteIndex]);
        RandomizeColors ();
        setColorsToShader (currentPalette);

        ColorLayers = 1 + (int) Mathf.Floor (Random.value * 2);
        Layers = ColorLayers * (2 + (int) Mathf.Floor (Random.value * 5));
        currentMaterial.SetFloat (name: "_DarkSteps", value: Layers);
        currentMaterial.SetFloat (name: "_ColorSteps", value: ColorLayers);

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
        currentMaterial.mainTexture = texture;

        updateUIControls ();
        resetDrawingTexture ();
    }

    private void setColorsToShader (Palette palette) {
        var color1 = colors[palette.ColorIndex1];
        var color2 = colors[palette.ColorIndex2];
        var color3 = colors[palette.ColorIndex3];
        var darkColor = 0.3f + Random.value * 0.1f;
        if (Layers == 0)
            darkColor = 1.0f;

        currentMaterial.SetColor (name: "_Color1", value: color1);
        currentMaterial.SetColor (name: "_Color2", value: color2);
        currentMaterial.SetColor (name: "_Color3", value: color3);
        currentMaterial.SetFloat (name: "_DarkColor", value: darkColor);
    }

    void updateUIControls () {
        Speed = 0.1f;

        brushSelector.GetComponent<Dropdown> ().value = (int) mouseMode;
        speedSlider.GetComponent<Slider> ().value = Speed;
        layersSlider.GetComponent<Slider> ().value = Layers;
        colorsSlider.GetComponent<Slider> ().value = ColorLayers;
        paletteSlider.GetComponent<Slider> ().value = paletteIndex + 1;

        updateColorSliders ();
    }

    void updateColorSliders () {
        Debug.Log ("updateColorSliders");
        Color1Slider.GetComponent<Slider> ().value = currentPalette.ColorIndex1 + 1;
        Color2Slider.GetComponent<Slider> ().value = currentPalette.ColorIndex2 + 1;
        Color3Slider.GetComponent<Slider> ().value = currentPalette.ColorIndex3 + 1;
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
        currentMaterial.SetFloat (name: "_Offset", value: offset);
    }

    private void Update () {
        if (Input.GetKeyDown (KeyCode.R)) {
            GenerateRandomValues ();
        }
        if (Input.GetKeyDown (KeyCode.Q)) {
            Application.Quit ();
        }
        if (Input.GetKeyDown (KeyCode.Escape)) {
            mainMenuButton.Toggle ();
        }
        if (EventSystem.current.IsPointerOverGameObject () && EventSystem.current.gameObject.name == "Image") {
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

    public void Quit () {
        Application.Quit ();
    }

    public void OnMouseModeChanges (int mode) {
        mouseMode = (MouseMode) mode;
    }

    public void OnSpeedChanged (float value) {
        Speed = value;
    }
    public void OnColorsChanged (float value) {
        ColorLayers = (int) value;
        currentMaterial.SetFloat (name: "_ColorSteps", value: ColorLayers);
    }
    public void OnLayersChanged (float value) {
        Layers = (int) value;
        int resultLayers = (Layers == 0) ? 1 : Layers;
        currentMaterial.SetFloat (name: "_DarkSteps", value: resultLayers);
        setColorsToShader (currentPalette);
    }
    public void OnPaletteChanged (float value) {
        paletteIndex = (int) value - 1;
        currentPalette = new Palette (palettes[paletteIndex]);
        setColorsToShader (currentPalette);
        updateColorSliders ();
    }
    public void OnColor1Changed (float value) {
        Debug.Log ("OnColor1Changed " + value);
        currentPalette.ColorIndex1 = (int) value - 1;
        setColorsToShader (currentPalette);
    }
    public void OnColor2Changed (float value) {
        Debug.Log ("OnColor2Changed " + value);
        currentPalette.ColorIndex2 = (int) value - 1;
        setColorsToShader (currentPalette);
    }
    public void OnColor3Changed (float value) {
        Debug.Log ("OnColor3Changed " + value);
        currentPalette.ColorIndex3 = (int) value - 1;
        setColorsToShader (currentPalette);
    }

    public void OnCameraSelected(int cameraIndex)
    {
        this.cameraIndex = cameraIndex;
        InitializeCamera();
    }
}