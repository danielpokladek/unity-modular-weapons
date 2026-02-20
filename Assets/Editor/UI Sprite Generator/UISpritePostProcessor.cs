using System.Collections.Generic;
using UnityEngine;

public abstract class UISpritePostProcessor : ScriptableObject
{
    public abstract void OnGenerationComplete(List<GeneratedSpriteInfo> results);
}
