using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Sample.LuisBot
{

    class BingWebPages
    {
        public string webSearchUrl { get; set; }
        public Boolean someResultsRemoved { get; set; }

        public dynamic totalEstimatedMatches { get; set; }

        public List<BingValues> value { get; set; }
    }
}