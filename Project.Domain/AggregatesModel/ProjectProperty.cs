using Project.Domain.SeedWork;
using System.Collections.Generic;

namespace Project.Domain.AggregatesModel
{
    public class ProjectProperty : ValueObject
    {
        public string Key { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }

        public int Project { get; set; }

        public ProjectProperty(string key, string text, string value)
        {
            this.Key = key;
            this.Text = text;
            this.Value = value;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Key;
            yield return Text;
            yield return Value;
        }
    }
}
