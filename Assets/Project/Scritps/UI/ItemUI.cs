using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [SerializeField]
    private Image _itemImage;

    public Image ItemImage => _itemImage;
}
