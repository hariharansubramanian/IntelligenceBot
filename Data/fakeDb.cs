using LuisBot.Models;
using System.Collections.Generic;

namespace LuisBot.Data
{
    public class FakeDb
    {
        public static HashSet<Incident> incidents = new HashSet<Incident>();
        public FakeDb()
        {
            if (incidents == null)
            {
                incidents = new HashSet<Incident>();
            }

        }
    }

}
