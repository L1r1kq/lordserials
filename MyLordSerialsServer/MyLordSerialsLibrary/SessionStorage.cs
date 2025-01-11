namespace MyLordSerialsLibrary;

public class SessionStorage
{
    private static readonly Dictionary<string, string> _sessions = new Dictionary<string, string>();

    // Сохранение токена и его соответствующего ID пользователя
    public static void SaveSession(string token, string userId)
    {
        _sessions[token] = userId;
    }

    // Проверка токена
    public static bool ValidateToken(string token)
    {
        return _sessions.ContainsKey(token);
    }

    // Получение ID пользователя по токену
    public static string GetUserId(string token)
    {
        return _sessions.TryGetValue(token, out var userId) ? userId : null;
    }

    // Удаление сессии
    public static void RemoveSession(string token)
    {
        _sessions.Remove(token);
    }
}