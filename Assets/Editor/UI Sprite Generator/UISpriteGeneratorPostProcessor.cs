using System.Collections.Generic;

public interface IUISpriteGeneratorPostProcessor
{
    void OnGenerationComplete(List<GeneratedSpriteInfo> results);
}
