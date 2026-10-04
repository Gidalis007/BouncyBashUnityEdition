using UnityEngine;

public class RedBlueBar : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private RectTransform _redFill;
    [SerializeField] private RectTransform _blueFill;

    #endregion Serialized Fields

    #region Attributes

    private float _redPercentage;

    #endregion Attributes

    #region Properties

    public float RedPercentage => _redPercentage;

    #endregion Properties

    #region Methods

    public void SetPercentage(float percentage)
    {
        _redPercentage = Mathf.Clamp(percentage, 0f, 100f);

        float red = _redPercentage / 100f;
        float blue = 1f - red;

        if (_redFill != null)
        {
            _redFill.anchorMin = new Vector2(0f, 0f);
            _redFill.anchorMax = new Vector2(red, 1f);
        }

        if (_blueFill != null)
        {
            _blueFill.anchorMin = new Vector2(red, 0f);
            _blueFill.anchorMax = new Vector2(1f, 1f);
        }
    }

    #endregion Methods
}
