#nullable enable

using System.IO;
using UnityEditor;
using UnityEngine;

public struct PrefabScreenshotSettings
{
    public string PrefabFolder;
    public string OutputFolder;
    public int Resolution;
    public float Padding;
    public Vector3 Rotation;
    public bool IsOrthographic;
}

public static class PrefabScreenshotTool
{
    public static void Generate(PrefabScreenshotSettings settings)
    {
        Debug.Log("[UI SCREENSHOT]: Starting UI icons generation");

        if (!Directory.Exists(settings.PrefabFolder))
        {
            Debug.LogError($"[UI SCREENSHOT]: Invalid prefab folder: {settings.PrefabFolder}!");
            return;
        }

        if (!Directory.Exists(settings.OutputFolder))
        {
            Directory.CreateDirectory(settings.OutputFolder);
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { settings.PrefabFolder });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path))
                continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogWarning($"[UI SCREENSHOT]: Skipped invalid prefab at path: {path}");
                continue;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab);
            var go = instance as GameObject;

            if (go == null)
            {
                Debug.LogWarning($"[UI SCREENSHOT]: Skipped invalid prefab at path: {path}");
                continue;
            }

            go.transform.rotation = Quaternion.Euler(settings.Rotation);

            string relativePath = path[settings.PrefabFolder.Length..];

            // Remove leading slashes
            relativePath = relativePath.TrimStart('/', '\\');

            // Change extension to PNG
            relativePath = Path.ChangeExtension(relativePath, ".png");

            string fullSavePath = Path.Combine(settings.OutputFolder, relativePath);

            // Ensure the final directory actually exists.
            string? directory = Path.GetDirectoryName(fullSavePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            TakeScreenshot(
                go,
                fullSavePath,
                settings.Resolution,
                settings.Padding,
                settings.IsOrthographic
            );

            Object.DestroyImmediate(instance);
        }

        AssetDatabase.Refresh();
        Debug.Log("[UI SCREENSHOT]: Finished generating prefab screenshots");

        string[] files = Directory.GetFiles(
            settings.OutputFolder,
            "*.png",
            SearchOption.AllDirectories
        );

        foreach (string file in files)
        {
            string assetPath = file.Replace(Application.dataPath, "Assets");

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);

            if (importer == null)
            {
                Debug.LogWarning($"[UI SCREENSHOT]: Invalid path for importer! {assetPath}");
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }

    private static void TakeScreenshot(
        GameObject prefabInstance,
        string fullSavePath,
        int resolution,
        float padding,
        bool isOrthographic
    )
    {
        string directory = Path.GetDirectoryName(fullSavePath);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        GameObject lightGO = new("TempLight");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50, 30, 0);

        prefabInstance.transform.position = Vector3.zero;

        Bounds bounds = CalculateBounds(prefabInstance);

        GameObject cameraGO = new("TempCamera");
        Camera camera = cameraGO.AddComponent<Camera>();

        camera.orthographic = isOrthographic;
        camera.clearFlags = CameraClearFlags.Color;
        camera.backgroundColor = Color.clear;

        float cameraSize = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        camera.orthographicSize = cameraSize * padding;

        camera.transform.position = bounds.center + Vector3.back * 10f;
        camera.transform.rotation = Quaternion.identity;

        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 100f;

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
        Object.DestroyImmediate(cameraGO);
        Object.DestroyImmediate(lightGO);
        Object.DestroyImmediate(screenshot);
    }

    private static Bounds CalculateBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
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
}
