using Lastic.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Lastic.Core.Configuration
{
    public class ConfigManager
    {
        readonly static Dictionary<LasticSectionParameter, string> exceptionMessage = new Dictionary<LasticSectionParameter, string>()
        {
            { LasticSectionParameter.MainIndex, "Unable to read connection name parameters."},
            { LasticSectionParameter.Index, "Unable to read connection name parameters."},
        };

        private static readonly LasticSection lasticSection = (LasticSection)ConfigurationManager
            .GetSection(nameof(LasticSection));

        public static LasticIndexSection Indices
        {
            get { return lasticSection.Indices; }
        }

        public static string GetIndexNameByKey(IndexTypes key)
        {
            string name = null;

            try
            {

                LasticIndex elasticSearchIndex = Indices.FirstOrDefault(i => i.Key == key);

                if (elasticSearchIndex != null)
                {
                    name = elasticSearchIndex.Name.ToLower();
                }

                if (string.IsNullOrEmpty(name))
                {
                    throw new Exceptions.ConfigurationException(exceptionMessage[LasticSectionParameter.Index]);
                }
            }
            catch
            {
                throw new Exceptions.ConfigurationException(exceptionMessage[LasticSectionParameter.Index]);
            }

            return name;
        }

        public static string MainIndexName
        {
            get
            {
                return GetIndexNameByKey(IndexTypes.MainIndex);
            }
        }

        public static Uri ConnectionUri
        {
            get
            {
                return new Uri(string.Format("{0}:{1}", lasticSection.Host, lasticSection.Port));
            }
        }
    }
}
