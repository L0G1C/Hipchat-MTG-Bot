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
using HipchatApiV2.Requests;
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

        /// <summary>
        /// Change this to name of Test room for teting
        /// </summary>
        static string room = "MTG";


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
                {
                    if (File.GetCreationTime("AllSets.json") < DateTime.Now.AddDays(-1.0)) { 
                        File.Delete("AllSets.json");
                        WebClient.DownloadFile("http://mtgjson.com/json/AllSets.json", "AllSets.json");
                    }
                }
                else
                {
                    WebClient.DownloadFile("http://mtgjson.com/json/AllSets.json", "AllSets.json");
                }
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
                            if(excludeList.Count > 75)
                                excludeList.RemoveAt(0);
                        }
                    }
                }
            }

            client.SendNotification(room, "MTG Bot is Ready!<br/>To request a card: {{cardname}}<br/>To search for a card: {{cardname:numresults:numcolumns}}", RoomColors.Green);
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

        private static string GenerateCardData(string cardData)
        {
            string cardName = "";
            int numResults = 3;
            int numColumns = 3;
            int test = 0;
            int column = 0;
            Boolean longForm = false;

            string[] cd = cardData.Split(new char[] {':'});

            if (cd.Length > 0)
                cardName = cd[0];

            if (cd.Length > 1)
            {
                if (int.TryParse(cd[1], out test)) {
                    if (test > 0)
                    {
                        longForm = true;
                        numResults = test;
                    }
                }
            }

            if (cd.Length > 2)
            {
                if (int.TryParse(cd[2], out test))
                {
                    if (test > 0)
                    {
                        longForm = true;
                        numColumns = test;
                    }
                }
            }

            var latestCardSet = cardJson.Values.LastOrDefault(q => q.cards.Any(p => p.name.ToUpper() == cardName.ToUpper()));
            Card card = null;

            string html;

            if (latestCardSet != null) {
                card = latestCardSet.cards.Last(c => c.name.ToUpper() == cardName.ToUpper());

                var cardImg = "<img src=\"http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid=" + card.multiverseid + "&amp;type=card\" height=\"200\" width=\"150\">";

                html =
                    string.Format(@"<a href=""http://gatherer.wizards.com/Pages/Card/Details.aspx?name={0}"">{1}</a>
                                    <br />
                                    <a href=""http://gatherer.wizards.com/Pages/Card/Details.aspx?name={0}"">{2}</a>",
                        HttpUtility.UrlEncode(card.name), card.name, cardImg);
            }
            else
            {
                html = "Exact match not found.  Best Matching card:<br />";
                card = FuzzyMatch.BestMatch2(cardJson, cardName);
                var cardImg = String.Format("<img src=\"http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid={0}&amp;type=card\" height=\"200\" width=\"150\">", card.multiverseid);

                html +=
                    string.Format(@"<a href=""http://gatherer.wizards.com/Pages/Card/Details.aspx?name={0}"">{1}</a>
                                    <br />
                                    <a href=""http://gatherer.wizards.com/Pages/Card/Details.aspx?name={0}"">{2}</a>
                                    <br />",
                        HttpUtility.UrlEncode(card.name), card.name, cardImg);
                longForm = true;
            }

            if(longForm) {
                CardResult[] cards = FuzzyMatch.FuzzyMatch2(cardJson, cardName, numResults);
                html += string.Format("Best {0} matches (smaller is better):<br/><table border=0><tr>", numResults);
                column = 0;
                while (column < numColumns)
                {
                    html += "<td>Name</td><td>Dist</td>";
                    column += 1;
                }
                html += "</tr>";
                column %= numColumns;
                html += "<tr>";
                foreach (CardResult c in cards)
                {
                    if (column == 0)
                        html += "<tr>";
                    html += string.Format(@"
                                                <td><a href=""http://gatherer.wizards.com/Pages/Card/Details.aspx?name={0}"">{1}</a></td>
                                                <td>{2}</td>
                                            ", HttpUtility.UrlEncode(c.card.name), c.card.name, c.distance);
                    column += 1;
                    column %= numColumns;
                    if (column == 0)
                        html += "</tr>";
                }
                if(column != 0)
                    html += "</tr>";
                html += "</table>";
            }

            if(card != null) {
                return html;
            }


            return "Card Not Recognized. Did you mean?..." + FuzzyMatch.BestMatch(cardName);


        }
    }
}
