using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HipchatApiV2;
using HipchatApiV2.Responses;
using HipchatApiV2.Enums;
using Newtonsoft.Json;
using System.Web;

namespace Hip_Chat_Mtg_Bot
{
    class Program
    {
        //var apiKey = "c7d2ef15f55d05ea84da26925f3bd8"; //This is what Matt Lingelbach gave me?
        static string regexPattern = @"{{(.+?)}}";
        static string room = "MTGTest";
        static List<string> excludeList = new List<string>();
        private static bool callfunction = true;
        static Dictionary<string, SetData> cardJson = null;
        

        static void Main(string[] args)
        {
            string apiKey = "hhJa1dKNwKv9COBhDlg2miyifJzZ63qufiycaB0n"; //I got this from MTG?Bot's user settings
            HipchatClient client = new HipchatClient(apiKey);

            //load jsonData and load list of cards currently mentioned without sending a billion notifications
            Init(client);            

            //View History API call shows last 75 messages. So, I need to call this function over and over. Decided on every 3 seconds
            Timer t = new Timer(ViewChatHistory, client, 0,4000);
            Console.ReadLine();
        }

        private static void Init(HipchatClient client)
        {
            client.SendNotification(room, "Initializing MTG Bot...", RoomColors.Green);

            //Get new json data on init (so I can just restart bot when new set comes out)
            using (WebClient WebClient = new WebClient())
            {
                if (File.Exists("AllSets.json"))
                    File.Delete("AllSets.json");
                WebClient.DownloadFile("http://mtgjson.com/json/AllSets.json", "AllSets.json");
            }

            LoadData();
            HipchatViewRoomHistoryResponse history = client.ViewRecentRoomHistory(room);
            foreach (var item in history.Items.OrderByDescending(q => q.Date))
            {
                if (!excludeList.Contains(item.Id) && !item.From.Equals("MTG Bot"))
                {
                    foreach (Match match in Regex.Matches(item.Message, regexPattern))
                    {
                        if (!String.IsNullOrEmpty(match.Value))
                        {                           
                            excludeList.Add(item.Id);
                        }
                    }
                }
            }

            client.SendNotification(room, "MTG Bot is Ready!", RoomColors.Green);
        }

        private static void LoadData()
        {
            using (var r = new StreamReader("AllSets.json"))
            {
                string json = r.ReadToEnd();
                cardJson = JsonConvert.DeserializeObject<Dictionary<string, SetData>>(json);
            }
        }

        /// <summary>
        /// Pulls in chat history for the "MTG" room, ordering messages in decending Date order
        /// If message was sent within 2 seconds ago, and it matches the {{card+name}} format, get the card info and send a notification with the data.
        /// </summary>
        /// <param name="o"></param>
        private static void ViewChatHistory(Object o)
        {
            var client = (HipchatClient)o;
            if (callfunction)
            {
                callfunction = false;

                
                HipchatViewRoomHistoryResponse history = client.ViewRecentRoomHistory(room);

                foreach (var item in history.Items.OrderByDescending(q => q.Date))
                {
                    //if (item.Date.AddSeconds(2) >= DateTime.Now && regEx.IsMatch(item.Message))
                    if (!excludeList.Contains(item.Id) && !item.From.Equals("MTG Bot"))
                    {
                                             
                        foreach (Match match in Regex.Matches(item.Message, regexPattern))
                        {
                            if (!String.IsNullOrEmpty(match.Value))
                            {
                                var cardName = match.Groups[1].Value;
                                var cardData = GenerateCardData(cardName);

                                client.SendNotification(room, cardData, RoomColors.Purple);
                                excludeList.Add(item.Id);
                            }   
                        }                        


                    }
                }
                callfunction = true;
            }



        }

        private static string GenerateCardData(string cardName)
        {
            var latestCardSet = cardJson.Values.LastOrDefault(q => q.cards.Any(p => p.name.ToUpper() == cardName.ToUpper()));

            if (latestCardSet != null)
            {
                Card card = latestCardSet.cards.Last(c => c.name.ToUpper() == cardName.ToUpper());
                var cardImg = "<img src=\"http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid=" + card.multiverseid + "&amp;type=card\" height=\"200\" width=\"150\">";

                var html =
                    string.Format(@"<a href=""http://gatherer.wizards.com/Pages/Card/Details.aspx?name={0}"">{1}</a>
                                    <br />
                                    <a href=""http://gatherer.wizards.com/Pages/Card/Details.aspx?name={0}"">{2}</a>",
                        HttpUtility.UrlEncode(card.name), card.name, cardImg);
                return html;
            }

            return "Card Not Recognized";


        }
    }
}
