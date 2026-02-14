using EditorAttributes;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField, ReadOnly]
    WeaponData _weaponData;

    [SerializeField]
    WeaponAttachment _weaponBody;

    private void Start()
    {
        _weaponBody.AttachmentPoints.ForEach(
            (point) =>
            {
                var firstElem = point.AvailableAttachments[0];

                if (!firstElem)
                    return;

                Instantiate(
                    firstElem,
                    point.AttachmentPosition.position,
                    Quaternion.identity,
                    point.AttachmentPosition
                );
            }
        );
    }
}
