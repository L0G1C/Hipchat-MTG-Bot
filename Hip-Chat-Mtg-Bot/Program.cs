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
        //var apiKey = "c7d2ef15f55d05ea84da26925f3bd8";
        static string regexPattern = @"{{(.+?)}}";
        static List<string> excludeList = new List<string>();
        private static bool callfunction = true;
        static Dictionary<string, SetData> cardJson = null;

        static void Main(string[] args)
        {
            //load jsonData
            LoadData();

            //View History API call shows last 75 messages. So, I need to call this function over and over. Decided on every 3 seconds
            Timer t = new Timer(ViewChatHistory, null, 0,4000);
            Console.ReadLine();
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
            if (callfunction)
            {
                callfunction = false;
                string apiKey = "hhJa1dKNwKv9COBhDlg2miyifJzZ63qufiycaB0n";
                HipchatClient client = new HipchatClient(apiKey);
                //client.SendNotification("TestRoom", "Test From MTG Bot", RoomColors.Purple);
                HipchatViewRoomHistoryResponse history = client.ViewRecentRoomHistory("TestRoom3");

                foreach (var item in history.Items.OrderByDescending(q => q.Date))
                {
                    //if (item.Date.AddSeconds(2) >= DateTime.Now && regEx.IsMatch(item.Message))
                    if (!excludeList.Contains(item.Id) && !item.From.Equals("MTG Bot"))
                    {

                        //TODO - This works for ONE card name. Need to update the regex to work for multiple cards in one line                        
                        foreach (Match match in Regex.Matches(item.Message, regexPattern))
                        {
                            if (!String.IsNullOrEmpty(match.Value))
                            {
                                var cardName = match.Groups[1].Value;
                                var cardData = GenerateCardData(cardName);

                                client.SendNotification("TestRoom3", cardData, RoomColors.Purple);
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
            var latestCardSet = cardJson.Values.LastOrDefault(q => q.cards.Any(p => p.name == cardName));

            if (latestCardSet != null)
            {
                Card card = latestCardSet.cards.Last(c => c.name == cardName);

                var html =
                    string.Format("<a href=\"http://gatherer.wizards.com/Pages/Card/Details.aspx?name={0}\">{1}</a>",
                        HttpUtility.UrlEncode(card.name), card.name);
                return html;
            }

            return "Card Not Recognized";


        }
    }
}
