using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Lastic.Core.Configuration
{
    public class LasticIndexSection : ConfigurationElementCollection, IEnumerable<LasticIndex>
    {
        private readonly List<LasticIndex> elements;

        public LasticIndexSection()
        {
            this.elements = new List<LasticIndex>();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            var element = new LasticIndex();
            this.elements.Add(element);
            return element;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LasticIndex)element).Name;
        }

        public new IEnumerator<LasticIndex> GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }
    }
}
