using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tools
{
    public static T EnsureComponent<T>(this MonoBehaviour m) where T : Component
    {
        var c = m.GetComponentInChildren<T>();

        if (c == null)
            throw new NullReferenceException();

        return c;
    }
}
