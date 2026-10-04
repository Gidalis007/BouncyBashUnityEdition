using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Fondu de transition entre scènes. Singleton persistant (créé à la demande,
// survit aux changements de scène). L'overlay est construit en code car il ne
// peut appartenir à aucune scène. Appelé à chaque changement de scène.
public class SceneTransition : MonoBehaviour
{
    #region Serialized Fields

    [Tooltip("Durée du fondu (aller comme retour), en secondes.")]
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private Color _fadeColor = Color.black;

    #endregion Serialized Fields

    #region Attributes

    private static SceneTransition _instance;
    private Canvas _canvas;
    private Image _fadeImage;
    private Coroutine _fadeRoutine;

    #endregion Attributes

    #region Properties

    public static SceneTransition Instance => _instance;
    public float FadeDuration => _fadeDuration;

    #endregion Properties

    #region Mono

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (_instance == this)
        {
            _instance = null;
        }
    }

    #endregion Mono

    #region Methods

    // Idempotent : crée le singleton s'il n'existe pas encore.
    public static void EnsureExists()
    {
        if (_instance != null)
        {
            return;
        }

        new GameObject("SceneTransition").AddComponent<SceneTransition>();
    }

    public void FadeOut(Action onComplete = null)
    {
        RestartFade(1f, onComplete);
    }

    public void FadeIn(Action onComplete = null)
    {
        RestartFade(0f, onComplete);
    }

    private void RestartFade(float targetAlpha, Action onComplete)
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, Action onComplete)
    {
        _canvas.enabled = true;

        float startAlpha = _fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _fadeDuration);
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }

        SetAlpha(targetAlpha);

        if (targetAlpha <= 0f)
        {
            _canvas.enabled = false;
        }

        _fadeRoutine = null;
        onComplete?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        Color color = _fadeColor;
        color.a = Mathf.Clamp01(alpha);
        _fadeImage.color = color;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Retour de fondu uniquement si on arrive masqué (évite un flash noir au boot).
        if (_fadeImage != null && _fadeImage.color.a > 0.01f)
        {
            FadeIn();
        }
        else if (_canvas != null)
        {
            _canvas.enabled = false;
        }
    }

    private void BuildOverlay()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 999;
        _canvas.enabled = false;

        gameObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(transform, false);

        RectTransform rect = imageObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;

        _fadeImage = imageObject.AddComponent<Image>();
        _fadeImage.raycastTarget = true;
        SetAlpha(0f);
    }

    #endregion Methods
}
