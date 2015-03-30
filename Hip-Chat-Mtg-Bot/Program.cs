using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HipchatApiV2;
using HipchatApiV2.Responses;
using HipchatApiV2.Enums;

namespace Hip_Chat_Mtg_Bot
{
    class Program
    {
        //var apiKey = "c7d2ef15f55d05ea84da26925f3bd8";
        static string regexPattern = @"^{{(.+?)}}$";        

        static void Main(string[] args)
        {
            
            //View History API call shows last 75 messages. So, I need to call this function over and over. Decided on every 3 seconds
            Timer t = new Timer(ViewChatHistory, null, 0,3000);
            Console.ReadLine();
        }

        /// <summary>
        /// Pulls in chat history for the "MTG" room, ordering messages in decending Date order
        /// If message was sent within 2 seconds ago, and it matches the {{card+name}} format, get the card info and send a notification with the data.
        /// </summary>
        /// <param name="o"></param>
        private static void ViewChatHistory(Object o) 
        {
            
            string apiKey = "hhJa1dKNwKv9COBhDlg2miyifJzZ63qufiycaB0n";
            HipchatClient client = new HipchatClient(apiKey);
            //client.SendNotification("TestRoom", "Test From MTG Bot", RoomColors.Purple);
            HipchatViewRoomHistoryResponse history = client.ViewRecentRoomHistory("TestRoom");
            Regex regEx = new Regex(regexPattern);
            var cardNames = new List<string>();

            foreach(var item in history.Items.OrderByDescending(q => q.Date)){
                if (item.Date.AddSeconds(2) >= DateTime.Now && regEx.IsMatch(item.Message))
                {

                    //TODO - This works for ONE card name. Need to update the regex to work for multiple cards in one line

                    var cardName = regEx.Match(item.Message).Groups[1].Value;
                    if (!String.IsNullOrEmpty(cardName))
                    {
                        client.SendNotification("TestRoom", cardName, RoomColors.Purple);
                    }

                    
                }
            }
            
            
            
        }
    }
}
