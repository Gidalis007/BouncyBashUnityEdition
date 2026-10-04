using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : NetworkBehaviour
{
    #region Serialized Fields

    [Tooltip("Durée de la partie en secondes.")]
    [SerializeField] private float _gameDuration = 180f;
    [SerializeField] private string _winSceneName = "WinScene";
    [SerializeField] private GameObject _timerCanvasPrefab;

    #endregion Serialized Fields

    #region Attributes

    private NetworkVariable<float> _remainingTime = new NetworkVariable<float>(180f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private GameObject _canvas;
    private TextMeshProUGUI _timerText;

    // Même objet en scène : arbitre du gagnant à la fin du chrono.
    private ScoreManager _scoreManager;
    private float _elapsed;
    private int _lastBroadcastSecond = -1;
    private int _lastShownSecond = -1;
    private bool _timeUp;

    #endregion Attributes

    #region Mono

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _remainingTime.Value = _gameDuration;
        }

        _elapsed = 0f;
        _lastBroadcastSecond = -1;
        _lastShownSecond = -1;
        _timeUp = false;

        _scoreManager = GetComponent<ScoreManager>();

        ShowTimer();
    }

    public override void OnNetworkDespawn()
    {
        HideTimer();

        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (IsServer)
        {
            TickServer();
        }

        UpdateDisplay();
    }

    #endregion Mono

    #region Methods

    // Seul le serveur fait avancer le chrono ; l'affichage est répliqué via _remainingTime.
    private void TickServer()
    {
        if (_timeUp)
        {
            return;
        }

        _elapsed += Time.deltaTime;

        float remaining = Mathf.Max(_gameDuration - _elapsed, 0f);
        int remainingSeconds = Mathf.CeilToInt(remaining);

        if (remainingSeconds != _lastBroadcastSecond)
        {
            _lastBroadcastSecond = remainingSeconds;
            _remainingTime.Value = remaining;
        }

        if (remaining <= 0f)
        {
            _timeUp = true;

            // Fin du chrono : l'équipe avec le plus de terrain gagne.
            if (_scoreManager != null)
            {
                _scoreManager.DeclareWinner(_scoreManager.GetLeadingTeam());
            }
            else
            {
                LoadWinScene();
            }
        }
    }

    private void LoadWinScene()
    {
        if (NetworkManager == null || !IsServer)
        {
            return;
        }

        if (string.IsNullOrEmpty(_winSceneName))
        {
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == _winSceneName)
        {
            return;
        }

        NetworkManager.SceneManager.LoadScene(
            _winSceneName,
            LoadSceneMode.Single
        );
    }

    private void ShowTimer()
    {
        if (_timerCanvasPrefab == null || _canvas != null)
        {
            return;
        }

        _canvas = Instantiate(_timerCanvasPrefab);

        foreach (TextMeshProUGUI text in _canvas.GetComponentsInChildren<TextMeshProUGUI>())
        {
            _timerText = text;
            break;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_timerText == null)
        {
            return;
        }

        int remainingSeconds = Mathf.CeilToInt(Mathf.Max(_remainingTime.Value, 0f));

        if (remainingSeconds == _lastShownSecond)
        {
            return;
        }

        _lastShownSecond = remainingSeconds;
        _timerText.text = string.Format("{0}:{1:00}", remainingSeconds / 60, remainingSeconds % 60);
    }

    private void HideTimer()
    {
        _timerText = null;

        if (_canvas != null)
        {
            Destroy(_canvas);
        }
    }

    #endregion Methods
}
