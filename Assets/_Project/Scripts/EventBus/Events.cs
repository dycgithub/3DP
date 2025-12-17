using System;

public interface IEvent
{
    
}

public struct TestEvent: IEvent
{
    public int a;
    public int b;
}