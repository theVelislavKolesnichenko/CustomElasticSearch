using Lastic.Core.Models.Enums;
using System.Configuration;

namespace Lastic.Core.Configuration
{
    public class LasticIndex : ConfigurationElement
    {
        [ConfigurationProperty("key", IsKey = true, IsRequired = true)]
        public IndexTypes Key
        {
            get
            {
                return (IndexTypes)this["key"];
            }
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return ((string)this["name"]).ToLower(); }
        }
    }
}
