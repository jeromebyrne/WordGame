using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainScene : MonoBehaviour
{
    public void OnPlayButtonPressed()
    {
        var evt = UIPlayButtonPressedEvent.Get();
        GameEventHandler.Instance.TriggerEvent(evt);
    }
}
