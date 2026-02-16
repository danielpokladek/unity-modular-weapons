using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [SerializeField]
    private Image _itemImage;

    [SerializeField]
    Button _button;

    public Image ItemImage => _itemImage;

    public Button Button => _button;

    public void Reset()
    {
        _itemImage.sprite = null;
        _button.interactable = true;
        transform.SetParent(null);

        _button.onClick.RemoveAllListeners();
    }
}
