using System.Linq.Expressions;
using System.Reflection;
using System.Data.SqlClient;
using MyLordSerialsLibrary;

namespace MyORMLibrary;

public class ORMContext<T> where T : class, new()
{
    private readonly string _connectionString;
    private readonly string _tableName;

    public ORMContext(string connectionString, string tableName)
    {
        _connectionString = connectionString;
        _tableName = tableName;
    }

    public void Create(T entity)
    {
        // Получаем все свойства типа T, кроме "Id"
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id") // Исключаем "Id"
            .ToArray();

        // Формируем строку столбцов
        var columns = string.Join(", ", properties.Select(p => p.Name));

        // Формируем строку значений с параметрами
        var values = string.Join(", ", properties.Select((p, i) => $"@p{i}"));

        // Формируем текст команды INSERT
        var commandText = $"INSERT INTO {_tableName} ({columns}) VALUES ({values})";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand(commandText, connection))
            {
                // Добавляем параметры без "Id"
                for (int i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(entity);
                    command.Parameters.AddWithValue($"@p{i}", value ?? DBNull.Value);
                }

                command.ExecuteNonQuery();
            }
        }
    }


    public T ReadById(int id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"SELECT * FROM {_tableName} WHERE Id = @id";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Map(reader);
                    }
                }
            }
        }

        return null;
    }


    public List<T> ReadByCondition(Expression<Func<T, bool>> predicate)
    {
        // Пример простой реализации с использованием ADO.NET
        // Для реального использования лучше использовать более продвинутые подходы

        var result = new List<T>();

        // Преобразование выражения в SQL WHERE-клаузулу (упрощённо)
        // В реальном мире это требует парсинга выражения или использования ORM-фреймворка

        string whereClause = "1=1"; // Заглушка, нужно реализовать парсер выражений

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string query = $"SELECT * FROM {typeof(T).Name} WHERE {whereClause}";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                // Добавление параметров из выражения (не реализовано)
                // Это требует парсинга выражения и извлечения параметров

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T entity = new T();
                       
                        result.Add(entity);
                    }
                }
            }
        }

        return result;
    }


    public IEnumerable<Films> ReadByAll()
    {
        var result = new List<Films>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"SELECT * FROM {_tableName}";

            using (var command = new SqlCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(MapFilms(reader)); // Используем правильный маппер для Films
                }
            }
        }

        return result;
    }
    
    public IEnumerable<User> ReadByAllUser()
    {
        var result = new List<User>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"SELECT * FROM {_tableName}";

            using (var command = new SqlCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(MapUser(reader)); // Используем правильный маппер для Films
                }
            }
        }

        return result;
    }

    
    
    private Films MapFilms(SqlDataReader reader)
    {
        var film = new Films();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            var property = typeof(Films).GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property != null && !reader.IsDBNull(i))
            {
                property.SetValue(film, reader.GetValue(i));
            }
        }
        return film;
    }
    
    private User MapUser(SqlDataReader reader)
    {
        var user = new User();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            var property = typeof(User).GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property != null && !reader.IsDBNull(i))
            {
                property.SetValue(user, reader.GetValue(i));
            }
        }
        return user;
    }




    public void Update(T entity)
    {
        // Получаем все свойства, исключая поле "Id"
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id") // Исключаем Id, так как оно генерируется автоматически
            .ToArray();

        // Формируем SET-часть SQL-запроса
        var setClause = string.Join(", ", properties.Select((p, i) => $"{p.Name} = @p{i}"));
        var commandText = $"UPDATE {_tableName} SET {setClause} WHERE Id = @Id";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand(commandText, connection))
            {
                // Добавляем параметры для всех свойств
                for (int i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(entity);
                    command.Parameters.AddWithValue($"@p{i}", value ?? DBNull.Value);
                }

                // Добавляем параметр для Id
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty == null)
                {
                    throw new Exception("Entity does not have an Id property.");
                }

                var idValue = idProperty.GetValue(entity);
                command.Parameters.AddWithValue("@Id", idValue); // Добавляем параметр Id для WHERE

                // Выполняем запрос
                command.ExecuteNonQuery();
            }
        }
    }



    public void Delete(Films film)
    {
        if (film == null)
        {
            throw new ArgumentNullException(nameof(film), "Film cannot be null");
        }

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"DELETE FROM {_tableName} WHERE Id = @id"; // Используем параметризованный запрос

            using (var command = new SqlCommand(sql, connection))
            {
                // Передаем id фильма для удаления
                command.Parameters.AddWithValue("@id", film.Id);
                command.ExecuteNonQuery(); // Выполняем команду удаления
            }
        }
    }
    
    public void DeleteUser(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User cannot be null");
        }

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string sql = $"DELETE FROM {_tableName} WHERE Id = @id"; // Используем параметризованный запрос

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", user.Id);
                command.ExecuteNonQuery(); // Выполняем команду удаления
            }
        }
    }




    public T FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
        var query = BuildQuery(predicate);

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand(query.Item1, connection))
            {
                foreach (var parameter in query.Item2)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Map(reader);
                    }
                }
            }
        }

        return null;
    }


    private T Map(SqlDataReader reader)
    {
        var entity = new T();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            Console.WriteLine($"Column in DB: {columnName}");
        
            var property = typeof(T).GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property != null && !reader.IsDBNull(i))
            {
                Console.WriteLine($"Mapping {columnName} to property {property.Name}");
                property.SetValue(entity, reader.GetValue(i));
            }
        }

        return entity;
    }

    private Tuple<string, Dictionary<string, object>> BuildQuery(Expression<Func<T, bool>> predicate)
        {
            var tableName = _tableName;
            var parameters = new Dictionary<string, object>();
            var whereClause = BuildWhereClause(predicate.Body, parameters);

            var query = $"SELECT * FROM {tableName} WHERE {whereClause}";
            return new Tuple<string, Dictionary<string, object>>(query, parameters);
        }

    private string BuildWhereClause(Expression expression, Dictionary<string, object> parameters)
    {
        if (expression is BinaryExpression binaryExpression)
        {
            var left = BuildWhereClause(binaryExpression.Left, parameters);
            var right = BuildWhereClause(binaryExpression.Right, parameters);
            var operatorString = binaryExpression.NodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                _ => throw new NotSupportedException($"Operator {binaryExpression.NodeType} is not supported")
            };

            return $"{left} {operatorString} {right}";
        }
        else if (expression is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        else if (expression is ConstantExpression constantExpression)
        {
            var parameterName = $"@p{parameters.Count}";
            parameters.Add(parameterName, constantExpression.Value);
            return parameterName;
        }
        else
        {
            throw new NotSupportedException($"Expression type {expression.GetType().Name} is not supported");
        }
    }


    
        public User FindUserByEmail(string email)
        {
            if (typeof(T) != typeof(User))
                throw new InvalidOperationException("FindUserByEmail can only be used with User type.");

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = $"SELECT * FROM {_tableName} WHERE Email = @Email";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Map(reader) as User;
                        }
                    }
                }
            }

            return null;
        }
    
        public User FindUserByAuthToken(string authToken)
        {
            if (typeof(T) != typeof(User))
                throw new InvalidOperationException("FindUserByAuthToken can only be used with User type.");

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = $"SELECT * FROM {_tableName} WHERE AuthToken = @AuthToken";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@AuthToken", authToken);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Map(reader) as User;
                        }
                    }
                }
            }

            return null;
        }
}

