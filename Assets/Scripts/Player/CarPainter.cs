using UnityEngine;

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

    protected override bool ServerAuthoritative => true;

    protected override void OnPlayerTouched(Collision collision)
    {
    }

    #endregion Methods
}
