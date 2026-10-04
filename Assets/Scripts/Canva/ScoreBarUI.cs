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

    // Moitiés fixes rouge/bleu : chaque moitié se remplit de son côté (rouge depuis
    // la gauche, bleu depuis la droite) + % absolus en texte.
    public void SetScores(float red, float blue)
    {
        if (_redFill != null)
        {
            _redFill.fillAmount = Mathf.Clamp01(red);
        }

        if (_blueFill != null)
        {
            _blueFill.fillAmount = Mathf.Clamp01(blue);
        }

        if (_scoreText != null)
        {
            _scoreText.text = string.Format("{0}% - {1}%", Mathf.RoundToInt(red * 100f), Mathf.RoundToInt(blue * 100f));
        }
    }

    #endregion Methods
}
