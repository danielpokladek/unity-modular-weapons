#nullable enable

using UnityEditor;
using UnityEngine;

public class PrefabScreenshotWindow : EditorWindow
{
    private const string PREFAB_FOLDER_KEY = "PrefabScreenshots_PrefabFolder";
    private const string OUTPUT_FOLDER_KEY = "PrefabScreenshots_OutputFolder";
    private const string RESOLUTION_KEY = "PrefabScreenshots_Resolution";
    private const string PADDING_KEY = "PrefabScreenshots_Padding";
    private const string ROTATION_X_KEY = "PrefabScreenshots_RotationX";
    private const string ROTATION_Y_KEY = "PrefabScreenshots_RotationY";
    private const string ROTATION_Z_KEY = "PrefabScreenshots_RotationZ";
    private const string ORTHOGRAPHIC_KEY = "PrefabScreenshots_Orthographic";
    private const string AUTO_ASSIGN_KEY = "PrefabScreenshots_AutoAssign";
    private const string POST_PROCESSOR_KEY = "PrefabScreenshots_PostProcessor";

    private DefaultAsset? _prefabFolderAsset;
    private DefaultAsset? _outputFolderAsset;
    private UISpritePostProcessor? _postProcessor;

    private int _resolution = 512;

    private float _padding = 1.15f;

    private Vector3 _rotation = new(0, 90, 0);

    private bool _isOrthographic = true;
    private bool _autoAssign = true;

    [MenuItem("Tools/Prefab Screenshot Generator")]
    public static void ShowWindow()
    {
        GetWindow<PrefabScreenshotWindow>("Prefab Screenshots");
    }

    private void OnEnable()
    {
        string prefabPath = EditorPrefs.GetString(PREFAB_FOLDER_KEY, "");
        string outputPath = EditorPrefs.GetString(OUTPUT_FOLDER_KEY, "");
        string postProcessorPath = EditorPrefs.GetString(POST_PROCESSOR_KEY, "");

        _prefabFolderAsset = string.IsNullOrEmpty(prefabPath)
            ? null
            : AssetDatabase.LoadAssetAtPath<DefaultAsset>(prefabPath);

        _outputFolderAsset = string.IsNullOrEmpty(outputPath)
            ? null
            : AssetDatabase.LoadAssetAtPath<DefaultAsset>(outputPath);

        _postProcessor = string.IsNullOrEmpty(postProcessorPath)
            ? null
            : AssetDatabase.LoadAssetAtPath<UISpritePostProcessor>(postProcessorPath);

        _resolution = EditorPrefs.GetInt(RESOLUTION_KEY, 512);
        _padding = EditorPrefs.GetFloat(PADDING_KEY, 1.15f);

        _rotation = new Vector3(
            EditorPrefs.GetFloat(ROTATION_X_KEY, 0f),
            EditorPrefs.GetFloat(ROTATION_Y_KEY, 90f),
            EditorPrefs.GetFloat(ROTATION_Z_KEY, 0f)
        );

        _isOrthographic = EditorPrefs.GetBool(ORTHOGRAPHIC_KEY, true);
        _autoAssign = EditorPrefs.GetBool(AUTO_ASSIGN_KEY, true);
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Label("Folders", EditorStyles.boldLabel);

        _prefabFolderAsset = (DefaultAsset?)
            EditorGUILayout.ObjectField(
                "Prefab Folder",
                _prefabFolderAsset,
                typeof(DefaultAsset),
                false
            );

        _outputFolderAsset = (DefaultAsset?)
            EditorGUILayout.ObjectField(
                "Output Folder",
                _outputFolderAsset,
                typeof(DefaultAsset),
                false
            );

        GUILayout.Space(10);

        GUILayout.Label("Rendering Settings", EditorStyles.boldLabel);

        _resolution = EditorGUILayout.IntField("Resolution", _resolution);
        _padding = EditorGUILayout.FloatField("Padding", _padding);
        _rotation = EditorGUILayout.Vector3Field("Rotation", _rotation);
        _isOrthographic = EditorGUILayout.Toggle("Is Orthographic", _isOrthographic);
        _autoAssign = EditorGUILayout.Toggle("Auto Assign Sprites", _autoAssign);

        _postProcessor = (UISpritePostProcessor?)
            EditorGUILayout.ObjectField(
                "Post Processor",
                _postProcessor,
                typeof(UISpritePostProcessor),
                false
            );

        GUILayout.Space(20);

        if (GUILayout.Button("Generate Screenshots", GUILayout.Height(40)))
        {
            if (_prefabFolderAsset == null || _outputFolderAsset == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both folders.", "OK");
                return;
            }

            var settings = new PrefabScreenshotSettings
            {
                PrefabFolder = AssetDatabase.GetAssetPath(_prefabFolderAsset),
                OutputFolder = AssetDatabase.GetAssetPath(_outputFolderAsset),
                Resolution = _resolution,
                Padding = _padding,
                Rotation = _rotation,
                IsOrthographic = _isOrthographic,
                AutoAssignSprites = _autoAssign,
            };

            var results = PrefabScreenshotTool.Generate(settings);

            _postProcessor?.OnGenerationComplete(results);
        }

        if (EditorGUI.EndChangeCheck())
        {
            SaveSettings();
        }
    }

    private void SaveSettings()
    {
        EditorPrefs.SetString(
            PREFAB_FOLDER_KEY,
            _prefabFolderAsset != null ? AssetDatabase.GetAssetPath(_prefabFolderAsset) : ""
        );

        EditorPrefs.SetString(
            OUTPUT_FOLDER_KEY,
            _outputFolderAsset != null ? AssetDatabase.GetAssetPath(_outputFolderAsset) : ""
        );

        EditorPrefs.SetInt(RESOLUTION_KEY, _resolution);
        EditorPrefs.SetFloat(PADDING_KEY, _padding);

        EditorPrefs.SetFloat(ROTATION_X_KEY, _rotation.x);
        EditorPrefs.SetFloat(ROTATION_Y_KEY, _rotation.y);
        EditorPrefs.SetFloat(ROTATION_Z_KEY, _rotation.z);

        EditorPrefs.SetBool(ORTHOGRAPHIC_KEY, _isOrthographic);
        EditorPrefs.SetBool(AUTO_ASSIGN_KEY, _autoAssign);

        EditorPrefs.SetString(
            POST_PROCESSOR_KEY,
            _postProcessor != null ? AssetDatabase.GetAssetPath(_postProcessor) : ""
        );
    }
}
