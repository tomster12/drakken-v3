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

    public void Init(Client client)
    {
        this.client = client;
    }

    protected Client client;
}
