using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class PlatformController : NetworkBehaviour
{
    [System.Serializable]
    public class TeamEvent : UnityEvent<ETeam>
    {
    }

    #region Serialized Fields

    [Header("Résolution de la texture de peinture")]
    [Tooltip("Plus la résolution est haute, plus la peinture est précise (mais plus c'est lourd).")]
    [SerializeField]
    private int _textureResolution = 256;

    [Header("Couleurs des teams")] [SerializeField]
    private Color _colorNone = Color.white;

    [SerializeField] private Color _colorBlue = Color.blue;
    [SerializeField] private Color _colorRed = Color.red;

    [Header("Capture")]
    [Range(0f, 1f)]
    [Tooltip("Pourcentage de la plateforme nécessaire pour capturer (0.6 = 60%).")]
    [SerializeField]
    private float _captureThreshold = 0.6f;

    [Tooltip("Si vrai, la plateforme est remplie à 100% par la team dès que le seuil est atteint.")] [SerializeField]
    private bool _floodFillOnCapture = true;

    [Header("Events")] [SerializeField]
    private TeamEvent _onPlatformCaptured;

    #endregion Serialized Fields

    #region Attributes

    private Texture2D _paintTexture;
    private int _totalPixels;
    private Renderer _renderer;

    private ETeam[] _pixelTeams;
    private Dictionary<ETeam, int> _teamPixelCount;

    private NetworkVariable<ETeam> _currentOwner = new NetworkVariable<ETeam>(ETeam.NONE,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private bool _isCaptured = false;

    #endregion Attributes

    #region Properties

    public ETeam CurrentOwner => _currentOwner.Value;
    public bool IsCaptured => _isCaptured;
    public int TotalPixels => _totalPixels;

    #endregion Properties

    #region Mono

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        InitTexture();
    }

    #endregion Mono

    #region Methods

    #region private methods

    private void InitTexture()
    {
        _paintTexture = new Texture2D(_textureResolution, _textureResolution, TextureFormat.RGBA32, false);

        _totalPixels = _textureResolution * _textureResolution;
        _pixelTeams = new ETeam[_totalPixels];

        _teamPixelCount = new Dictionary<ETeam, int>
        {
            { ETeam.NONE, _totalPixels },
            { ETeam.BLUE, 0 },
            { ETeam.RED, 0 }
        };

        Color32[] initColors = new Color32[_totalPixels];
        for (int i = 0; i < _totalPixels; i++)
        {
            _pixelTeams[i] = ETeam.NONE;
            initColors[i] = _colorNone;
        }

        _paintTexture.SetPixels32(initColors);
        _paintTexture.Apply();

        _renderer.material.mainTexture = _paintTexture;
    }



    private void CheckCapture()
    {
        CheckTeamCapture(ETeam.BLUE);
        CheckTeamCapture(ETeam.RED);
    }

    private void CheckTeamCapture(ETeam team)
    {
        float ratio = (float)_teamPixelCount[team] / _totalPixels;
        if (ratio >= _captureThreshold && _currentOwner.Value != team)
        {
            CapturePlatformRpc(team);
        }
    }

    private Vector2 WorldToUV(Vector3 worldPosition)
    {
        Bounds bounds = _renderer.bounds;
        float u = (worldPosition.x - bounds.min.x) / bounds.size.x;
        float v = (worldPosition.z - bounds.min.z) / bounds.size.z;
        return new Vector2(u, v);
    }

    private float GetPlatformWorldSize()
    {
        Bounds bounds = _renderer.bounds;
        return Mathf.Max(bounds.size.x, bounds.size.z);
    }

    #endregion private methods

    #region public methods

    public void ResetPlatform()
    {
        _currentOwner.Value = ETeam.NONE;
        _isCaptured = false;
        InitTexture();
    }

    public float GetTeamPercentage(ETeam team)
    {
        if (_teamPixelCount == null) return 0f;
        return (float)_teamPixelCount[team] / _totalPixels;
    }

    public int GetTeamPixelCount(ETeam team)
    {
        if (_teamPixelCount == null) return 0;
        return _teamPixelCount.TryGetValue(team, out int count) ? count : 0;
    }

    public void InitHalfSplit()
    {
        if (_pixelTeams == null || _paintTexture == null) return;

        Color32 redColor = GetColorForTeam(ETeam.RED);
        Color32 blueColor = GetColorForTeam(ETeam.BLUE);
        Color32[] colors = new Color32[_totalPixels];

        int half = _textureResolution / 2;
        int redCount = 0;

        for (int i = 0; i < _totalPixels; i++)
        {
            bool isRed = (i % _textureResolution) < half;
            _pixelTeams[i] = isRed ? ETeam.RED : ETeam.BLUE;
            colors[i] = isRed ? redColor : blueColor;

            if (isRed) redCount++;
        }

        _teamPixelCount[ETeam.NONE] = 0;
        _teamPixelCount[ETeam.RED] = redCount;
        _teamPixelCount[ETeam.BLUE] = _totalPixels - redCount;

        _paintTexture.SetPixels32(colors);
        _paintTexture.Apply(false);

        _isCaptured = false;
    }

    public Color GetColorForTeam(ETeam team)
    {
        switch (team)
        {
            case ETeam.BLUE: return _colorBlue;
            case ETeam.RED: return _colorRed;
            default: return _colorNone;
        }
    }


    public bool Paint(Vector3 worldPosition, ETeam team, float brushRadiusWorld)
    {
        Vector2 uv = WorldToUV(worldPosition);
        if (uv.x < 0f || uv.x > 1f || uv.y < 0f || uv.y > 1f) return false;

        int centerX = Mathf.RoundToInt(uv.x * (_textureResolution - 1));
        int centerY = Mathf.RoundToInt(uv.y * (_textureResolution - 1));

        float platformSize = GetPlatformWorldSize();
        int pixelRadius = Mathf.Max(1, Mathf.RoundToInt((brushRadiusWorld / platformSize) * _textureResolution));

        bool changed = false;

        for (int y = -pixelRadius; y <= pixelRadius; y++)
        {
            for (int x = -pixelRadius; x <= pixelRadius; x++)
            {
                if (x * x + y * y > pixelRadius * pixelRadius) continue;

                int px = centerX + x;
                int py = centerY + y;
                if (px < 0 || px >= _textureResolution || py < 0 || py >= _textureResolution) continue;

                int index = py * _textureResolution + px;
                ETeam previous = _pixelTeams[index];
                if (previous == team) continue;

                _teamPixelCount[previous]--;
                _teamPixelCount[team]++;
                _pixelTeams[index] = team;

                _paintTexture.SetPixel(px, py, GetColorForTeam(team));
                changed = true;
            }
        }

        if (changed)
        {
            _paintTexture.Apply(false);
            CheckCapture();
        }

        return changed;
    }

    #endregion public methods

    #region Rpc

    [Rpc(SendTo.Authority, InvokePermission = RpcInvokePermission.Everyone)]
    private void CapturePlatformRpc(ETeam team)
    {
        _currentOwner.Value = team;
        _isCaptured = true;

        if (_floodFillOnCapture)
        {
            FillAllWithRpc(team);
        }

        _onPlatformCaptured?.Invoke(team);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void FillAllWithRpc(ETeam team)
    {
        Color color = GetColorForTeam(team);
        Color32[] colors = new Color32[_totalPixels];
        for (int i = 0; i < _totalPixels; i++)
        {
            _pixelTeams[i] = team;
            colors[i] = color;
        }

        _paintTexture.SetPixels32(colors);
        _paintTexture.Apply(false);

        _teamPixelCount[ETeam.NONE] = 0;
        _teamPixelCount[ETeam.BLUE] = team == ETeam.BLUE ? _totalPixels : 0;
        _teamPixelCount[ETeam.RED] = team == ETeam.RED ? _totalPixels : 0;
    }

    #endregion Rpc

    #endregion methods

}
