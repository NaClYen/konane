using System;

public class InfoCenter : IInfoCenter
{
    public event Action<string/*msg*/, object /*args*/> OnAnyEvent;
    public void InvokeEvent(string msg, object args = null)
    {
        OnAnyEvent?.Invoke(msg, args);
    }
}
