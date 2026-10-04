using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : NetworkBehaviour
{
    #region Serialized Fields

    [SerializeField] private GameObject _redBlueBarPrefab;
    [SerializeField] private string _winSceneName = "WinScene";

    [Tooltip("Part du terrain (0-1) à atteindre pour gagner immédiatement.")]
    [Range(0f, 1f)]
    [SerializeField] private float _winThreshold = 1f;

    [Tooltip("Intervalle de recalcul des scores côté serveur.")]
    [SerializeField] private float _refreshInterval = 0.5f;

    #endregion Serialized Fields

    #region Attributes

    private NetworkVariable<float> _redPercent = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> _bluePercent = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private RedBlueBar _redBlueBar;
    private GameObject _canvas;
    private float _refreshTimer;
    private bool _gameOver;

    #endregion Attributes

    #region Properties

    public static ETeam MatchWinner { get; private set; } = ETeam.NONE;

    public float RedPercent => _redPercent.Value;
    public float BluePercent => _bluePercent.Value;

    #endregion Properties

    #region Mono

    public override void OnNetworkSpawn()
    {
        MatchWinner = ETeam.NONE;
        _refreshTimer = 0f;
        _gameOver = false;

        if (IsServer)
        {
            InitTerrainRpc();
        }

        ShowBar();
    }

    public override void OnNetworkDespawn()
    {
        HideBar();

        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (IsServer && !_gameOver)
        {
            _refreshTimer += Time.deltaTime;

            if (_refreshTimer >= _refreshInterval)
            {
                _refreshTimer = 0f;
                ComputeAndBroadcast();
            }
        }

        UpdateBar();
    }

    #endregion Mono

    #region Methods

    public ETeam GetLeadingTeam()
    {
        return _redPercent.Value >= _bluePercent.Value ? ETeam.RED : ETeam.BLUE;
    }

    public void DeclareWinner(ETeam winner)
    {
        if (NetworkManager == null || !IsServer || _gameOver)
        {
            return;
        }

        _gameOver = true;
        ReceiveWinnerRpc(winner);

        if (string.IsNullOrEmpty(_winSceneName))
        {
            return;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == _winSceneName)
        {
            return;
        }

        SceneTransition.EnsureExists();
        NotifyTransitionRpc();
        StartCoroutine(LoadSceneAfterFade());
    }

    private void ComputeAndBroadcast()
    {
        PlatformController[] platforms = FindObjectsByType<PlatformController>();

        long redPixels = 0;
        long bluePixels = 0;
        long totalPixels = 0;

        foreach (PlatformController platform in platforms)
        {
            if (platform == null)
            {
                continue;
            }

            redPixels += platform.GetTeamPixelCount(ETeam.RED);
            bluePixels += platform.GetTeamPixelCount(ETeam.BLUE);
            totalPixels += platform.TotalPixels;
        }

        float red = totalPixels > 0 ? (float)redPixels / totalPixels : 0f;
        float blue = totalPixels > 0 ? (float)bluePixels / totalPixels : 0f;

        _redPercent.Value = red;
        _bluePercent.Value = blue;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TeamsPoints[ETeam.RED] = Mathf.RoundToInt(red * 100f);
            GameManager.Instance.TeamsPoints[ETeam.BLUE] = Mathf.RoundToInt(blue * 100f);
        }

        if (red >= _winThreshold)
        {
            DeclareWinner(ETeam.RED);
        }
        else if (blue >= _winThreshold)
        {
            DeclareWinner(ETeam.BLUE);
        }
    }

    private void ShowBar()
    {
        if (_redBlueBarPrefab == null || _canvas != null)
        {
            return;
        }

        _canvas = Instantiate(_redBlueBarPrefab);
        _redBlueBar = _canvas.GetComponent<RedBlueBar>();

        UpdateBar();
    }

    private void UpdateBar()
    {
        if (_redBlueBar == null)
        {
            return;
        }

        _redBlueBar.SetPercentage(_redPercent.Value * 100f);
    }

    private void HideBar()
    {
        _redBlueBar = null;

        if (_canvas != null)
        {
            Destroy(_canvas);
        }
    }

    private IEnumerator LoadSceneAfterFade()
    {
        yield return new WaitForSeconds(SceneTransition.Instance.FadeDuration);

        if (NetworkManager == null || !IsServer)
        {
            yield break;
        }

        NetworkManager.SceneManager.LoadScene(
            _winSceneName,
            LoadSceneMode.Single
        );
    }

    #endregion Methods

    #region Network RPCs

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReceiveWinnerRpc(ETeam winner)
    {
        MatchWinner = winner;

        if (IsServer && GameManager.Instance != null)
        {
            GameManager.Instance.WinnerTeam = winner;
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void NotifyTransitionRpc()
    {
        SceneTransition.EnsureExists();
        SceneTransition.Instance.FadeOut();
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void InitTerrainRpc()
    {
        foreach (PlatformController platform in FindObjectsByType<PlatformController>())
        {
            if (platform != null)
            {
                platform.InitHalfSplit();
            }
        }
    }

    #endregion Network RPCs
}
