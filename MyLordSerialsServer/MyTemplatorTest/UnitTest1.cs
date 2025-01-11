using MyLordSerialsServer.Core;

namespace MyTemplatorTest;

public class Tests
{
    [TestFixture]
    public class SimpleTemplateEngineTests
    {
        private SimpleTemplateEngine _engine;

        [SetUp]
        public void SetUp()
        {
            _engine = new SimpleTemplateEngine();
        }

        // Тест на обработку простых переменных
        [Test]
        public void Render_ShouldReplaceVariablesCorrectly()
        {
            var template = "Hello, {{Name}}! Today is {{Date}}.";
            var data = new { Name = "Alice", Date = DateTime.Now.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("ru-RU")) };

            var result = _engine.Render(template, data);

            Assert.IsTrue(result.Contains("Hello, Alice!"));
            Assert.IsTrue(result.Contains(DateTime.Now.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"))));
        }

        // Тест на обработку циклов (foreach)
        [Test]
        public void Render_ShouldProcessForEachLoopCorrectly()
        {
            var template = "Items: {{#foreach Items}}<li>{{Name}}</li>{{/foreach}}";
            var data = new
            {
                Items = new List<object>
                {
                    new { Name = "Item 1" },
                    new { Name = "Item 2" }
                }
            };

            var result = _engine.Render(template, data);

            Assert.IsTrue(result.Contains("<li>Item 1</li>"));
            Assert.IsTrue(result.Contains("<li>Item 2</li>"));
        }

        // Тест на обработку пустой коллекции в цикле (foreach)
        [Test]
        public void Render_ShouldReturnEmptyStringForEmptyCollectionInForEach()
        {
            var template = "Items: {{#foreach Items}}<li>{{Name}}</li>{{/foreach}}";
            var data = new { Items = new List<object>() };

            var result = _engine.Render(template, data);

            Assert.IsFalse(result.Contains("<li>"));
        }

        // Тест на обработку условий (if)
        [Test]
        public void Render_ShouldProcessIfConditionCorrectly()
        {
            var template = "Condition: {{#if IsValid}}Valid{{/if}}";
            var data = new { IsValid = true };

            var result = _engine.Render(template, data);

            Assert.IsTrue(result.Contains("Valid"));
        }

        // Тест на обработку даты
        [Test]
        public void Render_ShouldFormatDateCorrectly()
        {
            var template = "Date: {{Date}}";
            var data = new { Date = new DateTime(2023, 10, 1) };

            var result = _engine.Render(template, data);

            Assert.IsTrue(result.Contains("1 октября 2023"));
        }

        // Тест на обработку пустых значений переменных
        [Test]
        public void Render_ShouldHandleEmptyValuesCorrectly()
        {
            var template = "Hello, {{Name}}!";
            var data = new { Name = (string)null };

            var result = _engine.Render(template, data);

            Assert.IsTrue(result.Contains("Hello, !"));
        }
    }
}