#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [SerializeField]
    Image _icon = null!;

    [SerializeField]
    TMP_Text _label = null!;

    [SerializeField]
    Button _button = null!;

    private Sprite _defaultIcon;

    public Button Button => _button;

    private void Start()
    {
        _defaultIcon = _icon.sprite;
    }

    public void Initialize(Sprite icon, string label, bool isRemove = false)
    {
        _icon.sprite = icon;
        _label.text = label;

        if (isRemove)
        {
            _icon.enabled = false;
        }
    }

    public void Reset()
    {
        transform.SetParent(null);

        _icon.sprite = _defaultIcon;
        _icon.enabled = true;

        _label.text = "NONE";

        _button.interactable = true;
        _button.onClick.RemoveAllListeners();
    }
}
