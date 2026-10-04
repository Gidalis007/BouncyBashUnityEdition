using UnityEngine;
using UnityEngine.UI;

public class DashIndicator : MonoBehaviour
{
#region Serialized Fields

    [SerializeField] private Image _squareImage;
    [SerializeField] private Color _availableColor = Color.green;
    [SerializeField] private Color _cooldownColor = Color.gray;

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

        _squareImage.color = available ? _availableColor : _cooldownColor;
    }

#endregion Methods
}
