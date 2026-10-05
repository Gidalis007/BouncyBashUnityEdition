using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameFont
{
    private const string FontResourcePath = "Fonts/TitanOne-Regular";
    private const float SweepInterval = 0.5f;

    private static TMP_FontAsset _fontAsset;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Font source = Resources.Load<Font>(FontResourcePath);

        if (source == null)
        {
            return;
        }

        _fontAsset = TMP_FontAsset.CreateFontAsset(source);

        if (_fontAsset == null)
        {
            return;
        }

        GameObject applierObject = new GameObject("GameFontApplier");
        Object.DontDestroyOnLoad(applierObject);
        applierObject.AddComponent<Applier>();

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyToAll();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToAll();
    }

    private static void ApplyToAll()
    {
        if (_fontAsset == null)
        {
            return;
        }

        foreach (TextMeshProUGUI text in Object.FindObjectsByType<TextMeshProUGUI>())
        {
            ApplyTo(text);
        }
    }

    private static void ApplyTo(TextMeshProUGUI text)
    {
        if (text == null || text.font == _fontAsset)
        {
            return;
        }

        text.font = _fontAsset;
        text.fontSharedMaterial = _fontAsset.material;
    }

    private sealed class Applier : MonoBehaviour
    {
        private float _timer;

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer < SweepInterval)
            {
                return;
            }

            _timer = 0f;
            ApplyToAll();
        }
    }
}
