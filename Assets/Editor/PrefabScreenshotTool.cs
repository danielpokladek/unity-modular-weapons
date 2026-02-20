#nullable enable

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct PrefabScreenshotSettings
{
    public string PrefabFolder;
    public string OutputFolder;
    public int Resolution;
    public float Padding;
    public Vector3 Rotation;
    public bool IsOrthographic;
    public bool AutoAssignSprites;
}

public static class PrefabScreenshotTool
{
    private static string _debugPrefix = "Icon Generation Tool";
    private static int _warningCount = 0;

    public static void Generate(PrefabScreenshotSettings settings)
    {
        Selection.activeObject = null;
        ActiveEditorTracker.sharedTracker.isLocked = true;

        _warningCount = 0;

        if (!Directory.Exists(settings.PrefabFolder))
        {
            Debug.LogError($"{_debugPrefix}: Invalid prefab folder: {settings.PrefabFolder}!");
            return;
        }

        if (!Directory.Exists(settings.OutputFolder))
        {
            Directory.CreateDirectory(settings.OutputFolder);
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { settings.PrefabFolder });
        int totalFiles = guids.Length;

        var originalAmbientMode = RenderSettings.ambientMode;
        var originalAmbientLight = RenderSettings.ambientLight;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.white;

        Scene originalScene = SceneManager.GetActiveScene();

        Scene tempScene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Additive
        );
        SceneManager.SetActiveScene(tempScene);

        GameObject lightGO = new("TempLight");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50, 30, 0);
        light.shadows = LightShadows.None;
        light.intensity = 1.2f;

        GameObject cameraGO = new("TempCamera");
        Camera camera = cameraGO.AddComponent<Camera>();

        camera.allowHDR = false;
        camera.allowMSAA = false;
        camera.orthographic = settings.IsOrthographic;
        camera.clearFlags = CameraClearFlags.Color;
        camera.backgroundColor = Color.clear;

        camera.transform.rotation = Quaternion.identity;

        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 100f;

        try
        {
            for (int i = 0; i < totalFiles; i++)
            {
                EditorUtility.DisplayProgressBar(
                    _debugPrefix,
                    $"Generating Texture {i + 1}/{totalFiles}",
                    (float)i / totalFiles
                );

                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (string.IsNullOrEmpty(path))
                    continue;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                {
                    _warningCount++;
                    Debug.LogWarning($"{_debugPrefix}: Skipped invalid prefab at path: {path}");
                    continue;
                }

                GameObject go = Object.Instantiate(prefab);

                if (go == null)
                {
                    _warningCount++;
                    Debug.LogWarning($"{_debugPrefix}: Skipped invalid prefab at path: {path}");
                    continue;
                }

                go.transform.rotation = Quaternion.Euler(settings.Rotation);

                string relativeSpritePath = GetRelativeSpritePath(
                    path,
                    settings.PrefabFolder,
                    settings.OutputFolder
                );

                string? directory = Path.GetDirectoryName(relativeSpritePath);

                if (!string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                TakeScreenshot(
                    go,
                    camera,
                    relativeSpritePath,
                    settings.Resolution,
                    settings.Padding
                );

                Object.DestroyImmediate(go);
            }
        }
        finally
        {
            Object.DestroyImmediate(cameraGO);
            Object.DestroyImmediate(lightGO);

            SceneManager.SetActiveScene(originalScene);
            EditorSceneManager.CloseScene(tempScene, true);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.UnloadUnusedAssetsImmediate();

        AssetDatabase.StartAssetEditing();

        try
        {
            var files = Directory.GetFiles(
                settings.OutputFolder,
                "*.png",
                SearchOption.AllDirectories
            );

            for (int i = 0; i < files.Length; i++)
            {
                EditorUtility.DisplayProgressBar(
                    _debugPrefix,
                    "Setting Import Settings...",
                    (float)i / files.Length
                );

                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(files[i]);

                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = settings.Resolution;
                importer.alphaIsTransparency = true;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.UnloadUnusedAssetsImmediate();
        }

        AssetDatabase.Refresh();

        if (settings.AutoAssignSprites)
        {
            AssignDataToPrefab(guids, settings.PrefabFolder, settings.OutputFolder);
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();
        EditorUtility.UnloadUnusedAssetsImmediate();

        RenderSettings.ambientMode = originalAmbientMode;
        RenderSettings.ambientLight = originalAmbientLight;

        ActiveEditorTracker.sharedTracker.isLocked = false;

        Debug.Log($"{_debugPrefix}: Complete. Generated sprites with {_warningCount} warnings.");
    }

    private static void TakeScreenshot(
        GameObject prefabInstance,
        Camera camera,
        string fullSavePath,
        int resolution,
        float padding
    )
    {
        prefabInstance.transform.position = Vector3.zero;

        Bounds bounds = CalculateBounds(prefabInstance);

        float cameraSize = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        camera.orthographicSize = cameraSize * padding;

        camera.transform.position = bounds.center + Vector3.back * 10f;

        RenderTexture rt = new(resolution, resolution, 24);
        camera.targetTexture = rt;

        Texture2D screenshot = new(resolution, resolution, TextureFormat.ARGB32, false);
        camera.Render();

        RenderTexture.active = rt;

        screenshot.ReadPixels(new(0, 0, resolution, resolution), 0, 0);
        screenshot.Apply();

        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(fullSavePath, bytes);

        camera.targetTexture = null;
        RenderTexture.active = null;

        rt.Release();

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(screenshot);
    }

    private static Bounds CalculateBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            _warningCount++;
            Debug.LogWarning(
                $"[UI SCREENSHOT]: No renderers found for prefab: {go.name}! Defaulting to 1x1x1 bounds."
            );

            return new(go.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return bounds;
    }

    private static void AssignDataToPrefab(string[] guids, string prefabFolder, string outputFolder)
    {
        var length = guids.Length;

        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar(
                "UI Screenshot Tool",
                $"Assigning Sprites {i + 1}/{length}",
                (float)i / length
            );

            string path = AssetDatabase.GUIDToAssetPath(guids[i]);

            if (string.IsNullOrEmpty(path))
                continue;

            string relativeSpritePath = GetRelativeSpritePath(path, prefabFolder, outputFolder)
                .Replace("\\", "/");

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativeSpritePath);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);

            if (prefabRoot == null || sprite == null)
            {
                _warningCount++;
                Debug.LogWarning(
                    $"[UI SCREENSHOT]: Could not link {path}. Sprite found: {sprite != null}. Sprite Path: {relativeSpritePath}"
                );
                continue;
            }

            try
            {
                var attachment = prefabRoot.GetComponentInChildren<WeaponAttachment>();

                if (attachment != null)
                {
                    SerializedObject so = new(attachment);

                    var spriteProp = so.FindProperty("_uiSprite");
                    if (spriteProp != null)
                        spriteProp.objectReferenceValue = sprite;

                    var idProp = so.FindProperty("_id");
                    if (idProp != null)
                        idProp.intValue = i;
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }

    private static string GetRelativeSpritePath(
        string path,
        string prefabFolder,
        string outputFolder
    )
    {
        string relativePath = path.Substring(prefabFolder.Length).TrimStart('/', '\\');
        relativePath = Path.ChangeExtension(relativePath, ".png");

        return Path.Combine(outputFolder, relativePath);
    }
}
