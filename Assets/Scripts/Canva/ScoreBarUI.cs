using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBarUI : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Image _redFill;
    [SerializeField] private Image _blueFill;
    [SerializeField] private TextMeshProUGUI _scoreText;

    #endregion Serialized Fields

    #region Methods

    // Moitiés fixes rouge/bleu : chaque moitié se remplit depuis l'extérieur,
    // pilotée en largeur (pas de fillAmount : ignoré sans sprite).
    public void SetScores(float red, float blue)
    {
        red = Mathf.Clamp01(red);
        blue = Mathf.Clamp01(blue);

        if (_redFill != null)
        {
            RectTransform rect = _redFill.rectTransform;
            rect.anchorMax = new Vector2(red * 0.5f, 1f);
        }

        if (_blueFill != null)
        {
            RectTransform rect = _blueFill.rectTransform;
            rect.anchorMin = new Vector2(1f - blue * 0.5f, 0f);
        }

        if (_scoreText != null)
        {
            _scoreText.text = string.Format("{0}% - {1}%", Mathf.RoundToInt(red * 100f), Mathf.RoundToInt(blue * 100f));
        }
    }

    #endregion Methods
}
