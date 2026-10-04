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

    // Affiche la part de chaque équipe (rouge à gauche, bleu à droite) + les % absolus.
    public void SetScores(float red, float blue)
    {
        float painted = red + blue;
        float redShare = painted > 0.0001f ? red / painted : 0.5f;

        if (_redFill != null)
        {
            _redFill.rectTransform.anchorMax = new Vector2(redShare, 1f);
        }

        if (_blueFill != null)
        {
            _blueFill.rectTransform.anchorMin = new Vector2(redShare, 0f);
        }

        if (_scoreText != null)
        {
            _scoreText.text = string.Format("{0}% - {1}%", Mathf.RoundToInt(red * 100f), Mathf.RoundToInt(blue * 100f));
        }
    }

    #endregion Methods
}
