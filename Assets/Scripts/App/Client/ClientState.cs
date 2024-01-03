using System;

[Serializable]
public abstract class ClientState
{
    public virtual void Set()
    { }

    public virtual void Unset()
    { }

    public virtual void Update()
    { }

    protected Client app;

    protected ClientState(Client app)
    {
        this.app = app;
    }
}
