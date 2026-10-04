using UnityEngine;

public class WinSceneSetup : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private string _winCarName = "WinCar";
    [SerializeField] private string _loseCarName = "LoseCar";

    [Tooltip("Temps max d'attente du gagnant avant d'abandonner (couleurs par défaut).")]
    [SerializeField] private float _waitTimeout = 5f;

    #endregion Serialized Fields

    #region Attributes

    private bool _applied;
    private float _waitTimer;

    #endregion Attributes

    #region Mono

    private void Start()
    {
        TryApply();
    }

    private void Update()
    {
        if (_applied)
        {
            return;
        }

        _waitTimer += Time.deltaTime;

        if (_waitTimer >= _waitTimeout)
        {
            _applied = true;
            return;
        }

        TryApply();
    }

    #endregion Mono

    #region Methods

    private void TryApply()
    {
        ETeam winner = ScoreManager.MatchWinner;

        if (winner != ETeam.RED && winner != ETeam.BLUE)
        {
            return;
        }

        ApplyColor(_winCarName, winner == ETeam.RED ? Color.red : Color.blue);
        ApplyColor(_loseCarName, winner == ETeam.RED ? Color.blue : Color.red);

        _applied = true;
    }

    private void ApplyColor(string carName, Color color)
    {
        GameObject car = GameObject.Find(carName);

        if (car == null)
        {
            return;
        }

        foreach (Renderer renderer in car.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }
    }

    #endregion Methods
}
