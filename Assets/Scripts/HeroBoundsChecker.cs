using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;

public class HeroBoundsChecker : MonoBehaviour
{
    [SerializeField]
    float leftLimit = 19.3427f;

    [SerializeField]
    float rightLimit = 40.6573f;

    [SerializeField]
    float lowerLimit = 11.62613f;

    [SerializeField]
    float leftBoundsCheck = 18.5f;

    [SerializeField]
    float rightBoundsCheck = 42f;

    [SerializeField]
    float bottomBoundsCheck = 11f;

    private void LateUpdate()
    {
        if (Player.Player1.transform.GetPositionX() < leftBoundsCheck)
        {
            Player.Player1.transform.SetPositionX(leftLimit);
        }

        if (Player.Player1.transform.GetPositionX() > rightBoundsCheck)
        {
            Player.Player1.transform.SetPositionX(rightLimit);
        }

        if (Player.Player1.transform.GetPositionY() < bottomBoundsCheck)
        {
            Player.Player1.transform.SetPositionY(lowerLimit);
        }
    }
}
