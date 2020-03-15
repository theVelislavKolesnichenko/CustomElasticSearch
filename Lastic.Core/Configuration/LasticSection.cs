using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Lastic.Core.Configuration
{
    public class LasticSection : ConfigurationSection
    {
        [ConfigurationProperty("host", DefaultValue = "http://localhost", IsRequired = true)]
        public String Host
        {
            get
            {
                return (string)this["host"];
            }
            set
            {
                this["host"] = value;
            }
        }

        [ConfigurationProperty("port", DefaultValue = "9200", IsRequired = false)]
        [IntegerValidator(ExcludeRange = false, MaxValue = 65356, MinValue = 0)]
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(LasticIndexSection), AddItemName = "index")]
        public LasticIndexSection Indices
        {
            get { return (LasticIndexSection)this[""]; }
        }
    }
}
