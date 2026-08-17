using UnityEngine;

public class ExitDoort : ButtonMenu
{
    protected override void OnClick()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}
