namespace MyLordSerialsServer.Attributes;

public sealed class GetAttribute : Attribute
{
    public string Route { get; }

    public GetAttribute(string route)
    {
        Route = route;
    }
}