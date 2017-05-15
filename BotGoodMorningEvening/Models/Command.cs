using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotGoodMorningEvening.Models
{
    public class Command
    {
        public string command { get; set; } = "cardset";
        public int top { get; set; }
        public string text { get; set; }
        public string culture { get; set; } = "en-EN";
        public string recipientGender { get; set; } = "M";
    }
}