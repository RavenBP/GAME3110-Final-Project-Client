using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolveButton : MonoBehaviour
{
    // Roundabout way, but directly calling SetPhase from the button does not work
    public void Click()
    {
        GameManager.Instance.gamePhaseManager.SetPhase(GamePhase.SOLVE);
    }
}
