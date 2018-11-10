public class ActorFrameworkSystem
{
    public ActorRef GetActorRef(object type, object id) => null;
}

public class ActorRef
{
    public void Send(Message message) { }
}

public class Message
{
    public ActorRef Sender;
}

public class Actor
{
    protected ActorFrameworkSystem System;

    protected void Terminate() { }
}

public class PersistedValue
{
    public static PersistedValue<T> Create<T>(T value)
        => new PersistedValue<T>(value);
}

public class PersistedValue<T> : PersistedValue
{
    public PersistedValue(T value) => Value = value;
    public T Value;
}

public interface IReceive<T>
{
    void Receive(T message);
}
