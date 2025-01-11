namespace MyLordSerialsServer.Attributes;

public sealed class PostAttribute : Attribute
{
    public string Route { get; }

    public PostAttribute(string route)
    {
        Route = route;
    }
}