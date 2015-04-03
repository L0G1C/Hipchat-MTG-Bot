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
        private const int MAXCARDNAME = 256;
        private const string URL = "http://api.deckbrew.com/mtg/cards";

        private const string urlParameters = "?name={0}";

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
        private static UInt16[,] d = new UInt16[MAXCARDNAME, MAXCARDNAME];

        public static Card BestMatch2(Dictionary<string, SetData> cardJson, string cardname)
        {
            int leastDistance = int.MaxValue;
            int curDistance = 0;
            Card match = cardJson.Values.ElementAt<SetData>(0).cards[0];
            foreach(SetData set in cardJson.Values) {
                foreach(Card c in set.cards) {
                    curDistance = LevenshteinDistance(c.name.ToUpper(), cardname.ToUpper());
                    if(curDistance < leastDistance) {
                        leastDistance = curDistance;
                        match = c;
                    }
                }
            }
            return match;
        }

        public static CardResult[] FuzzyMatch2(Dictionary<string, SetData> cardJson, string cardname, int numMatches)
        {
            int curDistance;
            List<CardResult> matches = new List<CardResult>();
            foreach (SetData set in cardJson.Values)
            {
                foreach (Card c in set.cards)
                {
                    if (!matches.Any(q => q.card.name == c.name)) { 
                        curDistance = LevenshteinDistance(c.name.ToUpper(), cardname.ToUpper());
                        if (matches.Count == numMatches) { 
                            if (curDistance < matches[matches.Count - 1].distance)
                            {
                                matches.Add(new CardResult(c, curDistance));
                                matches.Sort();
                                matches.RemoveAt(numMatches);
                            }
                        }
                        else
                        {
                            matches.Add(new CardResult(c, curDistance));
                            matches.Sort();
                        }
                    }
                }
            }
            return matches.ToArray();
        }

        public static int LevenshteinDistance(string s, string t)
        {
            UInt16 n, m;
            n = (UInt16)s.Length;
            m = (UInt16)t.Length;

            if(n==0)
                return m;
            if(m==0)
                return n;

            for (UInt16 i = 0; i <= n; d[i, 0] = i++) {}

	        for (UInt16 j = 0; j <= m; d[0, j] = j++) {}

            for (int i = 1; i <= n; i++) {
	            //Step 4
	            for (int j = 1; j <= m; j++)
	            {
		        // Step 5
		        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

		        // Step 6
		        d[i, j] = (UInt16)Math.Min(
		            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
		            d[i - 1, j - 1] + cost);
	            }
	        }

            return d[n,m];
        }
    }



}
