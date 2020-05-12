using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoCenter : IInfoCenter
{
    public event Action<string/*msg*/, object /*args*/> OnAnyEvent;
    public void InvokeEvent(string msg, object args)
    {
        OnAnyEvent.Invoke(msg, args);
    }
}
