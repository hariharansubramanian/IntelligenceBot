namespace Microsoft.Bot.Sample.LuisBot
{

    class BingResponse
    {
        public string _type { get; set; }
        public dynamic queryContext { get; set; }

        public dynamic rankingResponse { get; set; }

        public BingWebPages webPages { get; set; }
    }
}