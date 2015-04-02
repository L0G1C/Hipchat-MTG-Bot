using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Hip_Chat_Mtg_Bot
{
    public class DataObject
    {
        public string name { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string store_url { get; set; }
    }

    class FuzzyMatch
    {
        private const string URL = "http://api.deckbrew.com/mtg/cards";

        private const string urlParameters = "?name={0}";

        //static void Main(string[] args)
        //{
        //    Console.WriteLine(BestMatch(args[0]));
        //}

        public static string[] FuzzyMatches(string cardname)
        {
            List<string> myCards = new List<string>();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync(String.Format(urlParameters, cardname)).Result;

            if (response.IsSuccessStatusCode)
            {
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<DataObject>>().Result;
                foreach (var d in dataObjects)
                {
                    myCards.Add(d.name);
                }
            }
            return myCards.ToArray();
        }

        public static string BestMatch(string cardname)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync(String.Format(urlParameters, cardname)).Result;

            if (response.IsSuccessStatusCode)
            {
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<DataObject>>().Result;
                if (dataObjects.Count<DataObject>() > 0)
                {
                    return dataObjects.ElementAt<DataObject>(0).name;
                }
            }
            return "";
        }
    }



}
