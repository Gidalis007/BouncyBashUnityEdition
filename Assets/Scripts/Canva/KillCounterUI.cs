using TMPro;
using Unity.Netcode;
using UnityEngine;

public class KillCounterUI : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private TextMeshProUGUI _killText;

    #endregion Serialized Fields

    #region Attributes

    private CarController _myCar;
    private int _lastShown = -1;

    #endregion Attributes

    #region Mono

    private void Update()
    {
        if (_myCar == null)
        {
            ResolveOwnedCar();
        }

        if (_myCar == null || _killText == null)
        {
            return;
        }

        int kills = _myCar.KillCount;

        if (kills == _lastShown)
        {
            return;
        }

        _lastShown = kills;
        _killText.text = string.Format("KILLS: {0}/3", kills);
    }

    #endregion Mono

    #region Methods

    private void ResolveOwnedCar()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            return;
        }

        foreach (NetworkObjectReference playerReference in GameManager.Instance.PlayersController)
        {
            NetworkObject player;

            try
            {
                if (!playerReference.TryGet(out player))
                {
                    continue;
                }
            }
            catch
            {
                continue;
            }

            if (!player.IsOwner)
            {
                continue;
            }

            _myCar = player.GetComponent<CarController>();

            if (_myCar != null)
            {
                return;
            }
        }
    }

    #endregion Methods
}
