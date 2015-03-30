using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hip_Chat_Mtg_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            //var apiKey = "c7d2ef15f55d05ea84da26925f3bd8";
            var apiKey = "hhJa1dKNwKv9COBhDlg2miyifJzZ63qufiycaB0n";
            var client = new HipchatApiV2.HipchatClient(apiKey);

            client.SendNotification("MTG", "Test From MTG Bot");
        }
    }
}
