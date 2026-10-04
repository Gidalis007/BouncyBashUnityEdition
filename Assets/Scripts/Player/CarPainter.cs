using UnityEngine;

// Récompense de kills : la voiture peint au contact pendant une durée (voir CarController).
// Hérite toute la logique de peinture de PainterController (même rythme, même rayon).
public class CarPainter : PainterController
{
    #region Attributes

    private bool _painting;
    private ETeam _paintTeam = ETeam.NONE;

    #endregion Attributes

    #region Methods

    public void SetPainting(bool painting, ETeam team)
    {
        _painting = painting;
        _paintTeam = team;
        enabled = painting;
    }

    protected override ETeam PaintTeam => _paintTeam;
    protected override bool CanPaint => _painting;

    // Pas de transfert d'ownership pour la voiture (comportement réservé à la bille).
    protected override void OnPlayerTouched(Collision collision)
    {
    }

    #endregion Methods
}
