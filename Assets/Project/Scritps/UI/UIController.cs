using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private Dictionary<WeaponAttachmentPoint, Transform> _attachmentDictionary = new();

    private void Update()
    {
        foreach (var point in _attachmentDictionary)
        {
            var screenPos = Camera.main.WorldToScreenPoint(point.Key.Transform.position);
            point.Value.transform.position = screenPos;
        }
    }

    public void RegisterAttachmentToUI(WeaponAttachmentPoint point, Transform uiPoint)
    {
        _attachmentDictionary.Add(point, uiPoint);
    }
}
