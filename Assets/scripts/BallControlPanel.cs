using Oculus.Interaction;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


// Attach this to an empty GameObject. When the scene runs, it builds a panel of
// Meta Interaction SDK sample poke buttons that change the ball's SineWaveMovement
// settings. The panel is built at this GameObject's position, with its buttons
// facing this GameObject's -Z direction (towards a player looking along +Z).
public class BallControlPanel : MonoBehaviour
{
    const string ButtonPrefabPath =
        "Packages/com.meta.xr.sdk.interaction/Runtime/Sample/Prefabs/OculusInteractionSamplePokeButton.prefab";


    [Tooltip("The ball to control. Left empty, the first SineWaveMovement in the scene is used.")]
    public SineWaveMovement ball;


    [Tooltip("Filled in automatically with the Interaction SDK sample poke button.")]
    public GameObject buttonPrefab;


    [Header("Change per button press")]
    public float speedStep = 0.25f;
    public float rangeStep = 0.5f;
    public float sideSwingStep = 0.25f;
    public float frequencyStep = 0.25f;


    [Header("Layout")]
    public float buttonScale = 0.6f;
    public float rowSpacing = 0.055f;
    public Color backplateColor = new Color(0.12f, 0.14f, 0.2f);


    TextMesh speedText, rangeText, sideSwingText, frequencyText, pauseText;
    float startSpeed, startRange, startSideSwing, startFrequency;
    Font font;


    void Start()
    {
        if (ball == null) ball = FindObjectOfType<SineWaveMovement>();
#if UNITY_EDITOR
        if (buttonPrefab == null) buttonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonPrefabPath);
#endif
        if (ball == null || buttonPrefab == null)
        {
            Debug.LogError("BallControlPanel: no SineWaveMovement ball or no button prefab found.");
            enabled = false;
            return;
        }


        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");


        startSpeed = ball.zSpeed;
        startRange = ball.zLimit;
        startSideSwing = ball.amplitude;
        startFrequency = ball.frequency;


        BuildPanel();
        RefreshLabels();
    }


    void BuildPanel()
    {
        float y = 0f;
        MakeLabel("Ball Controls", new Vector3(0f, y, 0f), 1.7f);
        y -= rowSpacing;


        speedText = MakeRow(y, () => ball.zSpeed = Mathf.Max(0f, ball.zSpeed - speedStep),
                               () => ball.zSpeed += speedStep);
        y -= rowSpacing;
        rangeText = MakeRow(y, () => ball.zLimit = Mathf.Max(rangeStep, ball.zLimit - rangeStep),
                               () => ball.zLimit += rangeStep);
        y -= rowSpacing;
        sideSwingText = MakeRow(y, () => ball.amplitude = Mathf.Max(0f, ball.amplitude - sideSwingStep),
                                   () => ball.amplitude += sideSwingStep);
        y -= rowSpacing;
        frequencyText = MakeRow(y, () => ball.frequency = Mathf.Max(0f, ball.frequency - frequencyStep),
                                   () => ball.frequency += frequencyStep);
        y -= rowSpacing * 1.2f;


        pauseText = MakeButton("Pause", new Vector3(-0.06f, y, 0f), () => ball.enabled = !ball.enabled);
        MakeButton("Reset", new Vector3(0.06f, y, 0f), () =>
        {
            ball.zSpeed = startSpeed;
            ball.zLimit = startRange;
            ball.amplitude = startSideSwing;
            ball.frequency = startFrequency;
            ball.enabled = true;
        });


        MakeBackplate(y);
    }


    // One row: [ - ]  Name: value  [ + ]. Returns the middle label.
    TextMesh MakeRow(float y, System.Action decrease, System.Action increase)
    {
        MakeButton("-", new Vector3(-0.13f, y, 0f), decrease);
        MakeButton("+", new Vector3(0.13f, y, 0f), increase);
        return MakeLabel("", new Vector3(0f, y, 0f), 1.3f);
    }


    TextMesh MakeButton(string text, Vector3 localPos, System.Action onPress)
    {
        GameObject button = Instantiate(buttonPrefab, transform);
        button.name = "Button " + text;
        button.transform.localPosition = localPos;
        button.transform.localRotation = Quaternion.identity;
        button.transform.localScale *= buttonScale;


        // Put our own label where the sample's TextMeshPro label sits, so it moves
        // with the button when pressed. (TextMeshPro needs its Essential Resources
        // imported to show text, so the built-in TextMesh is used instead.)
        TMP_Text sampleLabel = button.GetComponentInChildren<TMP_Text>(true);
        Transform labelParent = sampleLabel != null ? sampleLabel.transform : button.transform;
        if (sampleLabel != null) sampleLabel.enabled = false;
        float textSize = text.Length == 1 ? 2.5f : 1.6f; // make "+" and "-" bigger
        TextMesh label = CreateText(text, labelParent, Vector3.zero, textSize, Color.black);
        label.transform.position -= labelParent.forward * 0.004f; // just in front of the button face


        // The sample button fires WhenRelease when a poke press is completed.
        PointableUnityEventWrapper events = button.GetComponent<PointableUnityEventWrapper>();
        events.WhenRelease.AddListener(_ =>
        {
            onPress();
            RefreshLabels();
        });
        return label;
    }


    TextMesh MakeLabel(string text, Vector3 localPos, float size)
    {
        return CreateText(text, transform, transform.TransformPoint(localPos + new Vector3(0f, 0f, -0.005f)), size, Color.white, true);
    }


    // Creates a TextMesh. 'size' 1 gives text that reads well at arm's length.
    TextMesh CreateText(string text, Transform parent, Vector3 position, float size, Color color, bool worldPosition = false)
    {
        GameObject go = new GameObject("Text " + text);
        go.transform.SetParent(parent, false);
        if (worldPosition) go.transform.position = position;
        else go.transform.localPosition = position;
        go.transform.localRotation = Quaternion.identity;
        float worldScale = 0.015f * size;
        go.transform.localScale = Vector3.one * (worldScale / parent.lossyScale.x);


        TextMesh tm = go.AddComponent<TextMesh>();
        tm.font = font;
        tm.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        tm.fontSize = 48;
        tm.fontStyle = FontStyle.Bold;
        tm.characterSize = 0.1f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;
        tm.text = text;
        return tm;
    }


    // A dark board behind the buttons so the white labels are easy to read.
    void MakeBackplate(float bottomY)
    {
        GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.name = "Backplate";
        Destroy(plate.GetComponent<Collider>());
        plate.transform.SetParent(transform, false);
        float top = rowSpacing * 0.6f;
        float bottom = bottomY - rowSpacing * 0.6f;
        plate.transform.localPosition = new Vector3(0f, (top + bottom) * 0.5f, 0.01f);
        plate.transform.localScale = new Vector3(0.4f, top - bottom, 0.01f);
        plate.GetComponent<Renderer>().material.color = backplateColor;
    }


    void RefreshLabels()
    {
        speedText.text = $"Speed: {ball.zSpeed:0.00}";
        rangeText.text = $"Range: {ball.zLimit:0.0}";
        sideSwingText.text = $"Side swing: {ball.amplitude:0.00}";
        frequencyText.text = $"Frequency: {ball.frequency:0.00}";
        if (pauseText != null) pauseText.text = ball.enabled ? "Pause" : "Play";
    }


#if UNITY_EDITOR
    // Runs when the script is added in the editor: links the sample button prefab
    // (so it is included in the build) and places the panel in front of the player.
    void Reset()
    {
        buttonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonPrefabPath);
        ball = FindObjectOfType<SineWaveMovement>();
        transform.position = new Vector3(0f, 1.2f, 0.45f);
        transform.rotation = Quaternion.identity;
    }
#endif
}