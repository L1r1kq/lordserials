namespace MyLordSerialsLibrary;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string Role { get; set; }
    public bool IsBlocked { get; set; }

    // Добавьте это свойство
    public string AuthToken { get; set; }
}