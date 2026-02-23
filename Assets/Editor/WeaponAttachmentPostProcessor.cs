using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(
    fileName = "WeaponAttachmentPostProcessor",
    menuName = "Icon Forge/Weapon Attachment Post Processor"
)]
public class WeaponAttachmentPostProcessor : IconForgePostProcessor
{
    public override void OnGenerationComplete(List<GeneratedSpriteData> results)
    {
        Debug.Log("Running post processor script..");

        var length = results.Count;
        var spriteCounter = 0;

        foreach (var result in results)
        {
            EditorUtility.DisplayProgressBar(
                "Icon Forge Post Process",
                $"Assigning Sprites {spriteCounter + 1}/{length}",
                (float)spriteCounter / length
            );

            string path = AssetDatabase.GUIDToAssetPath(result.GUID);

            if (string.IsNullOrEmpty(path))
                continue;

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(result.SpritePath);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);

            if (prefabRoot == null || sprite == null)
            {
                Debug.LogWarning(
                    $"Could not link {path}. Sprite found: {sprite != null}. Sprite Path: {result.SpritePath}"
                );
                continue;
            }

            try
            {
                if (prefabRoot.TryGetComponent<WeaponAttachment>(out var attachment))
                {
                    SerializedObject so = new(attachment);

                    var spriteProp = so.FindProperty("_uiSprite");
                    if (spriteProp != null)
                        spriteProp.objectReferenceValue = sprite;

                    var idProp = so.FindProperty("_id");
                    if (idProp != null)
                        idProp.intValue = spriteCounter++;

                    so.ApplyModifiedProperties();
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, path, out bool successful);

                if (!successful)
                {
                    Debug.LogError("Failed to save prefab!");

                    AssetDatabase.SaveAssets();
                    EditorUtility.ClearProgressBar();
                    EditorUtility.UnloadUnusedAssetsImmediate();

                    return;
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
        EditorUtility.UnloadUnusedAssetsImmediate();
    }
}
