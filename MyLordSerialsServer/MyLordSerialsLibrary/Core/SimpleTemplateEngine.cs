using System.Text.RegularExpressions;

namespace MyLordSerialsServer.Core;

public class SimpleTemplateEngine
{
    public string Render(string template, object data)
    {
        // Сначала обрабатываем циклы, если они есть
        template = ProcessLoops(template, data);

        // Теперь обрабатываем переменные
        var properties = data.GetType().GetProperties();
        var result = template;

        foreach (var property in properties)
        {
            var placeholder = $"{{{{{property.Name}}}}}";
            var value = property.GetValue(data);

            // Если это дата, форматируем её
            if (value is DateTime dateValue)
            {
                value = dateValue.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));
            }

            result = result.Replace(placeholder, value?.ToString() ?? string.Empty);
        }

        return result;
    }

    private string ProcessLoops(string template, object data)
    {
        // Простейшая обработка циклов для списка объектов
        var regex = new Regex(@"\{\{#foreach (.*?)\}\}(.*?)\{\{\/foreach\}\}", RegexOptions.Singleline);
        return regex.Replace(template, match =>
        {
            var propertyName = match.Groups[1].Value;
            var loopContent = match.Groups[2].Value;

            var property = data.GetType().GetProperty(propertyName);
            if (property != null)
            {
                var collection = property.GetValue(data) as IEnumerable<object>;
                if (collection != null && collection.Any())
                {
                    var loopResult = string.Empty;
                    foreach (var item in collection)
                    {
                        loopResult += new SimpleTemplateEngine().Render(loopContent, item);
                    }
                    return loopResult;
                }
            }
            // Если коллекция пуста или не найдена, возвращаем пустое место
            return string.Empty;
        });
    }

    private string ProcessConditions(string template, object data)
    {
        // Простейшая обработка условий if
        var regex = new Regex(@"\{\{#if (.*?)\}\}(.*?)\{\{\/if\}\}", RegexOptions.Singleline);
        return regex.Replace(template, match =>
        {
            var conditionProperty = match.Groups[1].Value;
            var conditionContent = match.Groups[2].Value;

            var property = data.GetType().GetProperty(conditionProperty);
            if (property != null)
            {
                var value = property.GetValue(data);
                if (value != null && (value is bool b && b || !(value is bool)))
                {
                    return new SimpleTemplateEngine().Render(conditionContent, data);
                }
            }
            return string.Empty;
        });
    }
}
