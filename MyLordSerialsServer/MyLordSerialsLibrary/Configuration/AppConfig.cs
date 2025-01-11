namespace MyLordSerialsLibrary.Configuration;

public sealed class AppConfig
{
    public static string Domain { get; set; } = "localhost";
    public static uint Port { get; set; } = 6529;
    public static string StaticDirectoryPath { get; set; } = @"Public/";
}