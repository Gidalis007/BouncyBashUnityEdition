using UnityEngine;
using UnityEngine.UI;

public class DashIndicator : MonoBehaviour
{
#region Serialized Fields

    [SerializeField] private Image _squareImage;
    [SerializeField] private Sprite _readySprite;
    [SerializeField] private Sprite _cooldownSprite;
    [SerializeField] private Color _availableColor = Color.white;
    [SerializeField] private Color _cooldownColor = Color.white;

#endregion Serialized Fields

#region Properties

    public bool IsAvailable { get; private set; } = true;

#endregion Properties

#region Methods

    public void SetAvailable(bool available)
    {
        IsAvailable = available;

        if (_squareImage == null)
            return;

        if (_readySprite != null && _cooldownSprite != null)
            _squareImage.sprite = available ? _readySprite : _cooldownSprite;

        _squareImage.color = available ? _availableColor : _cooldownColor;
    }

#endregion Methods
}
