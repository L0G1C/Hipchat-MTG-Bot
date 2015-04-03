# Hip-Chat-MTG-Bot
This is an "MTG" Bot for Hip Chat. It uses KyleGobel's Hipchat-CS wrapper https://github.com/KyleGobel/Hipchat-CS to get room 
information with Hipchat's API v2 and helps identify MTG card data referenced by [[card name]]


## Use:
1. Make a user called "MTG Bot". 
2. Update the string apiKey in Program.cs. This the private API key for the MTG Bot user. Limited to 100 hits per 5 minutes.
3. Update the  string room variable to the name of the room the Bot should parse for input.
4. Run the Project. Bot will give an initiliazation, then welcome message.


## Acceptable Card Searching
Currently MTG bot looks for a card name enclosed with {{ }}. For example "{{Sphinx's Revelation}}.

It then matches against a json data file of all mtg cards from http://mtgjson.com. If there is an exact match, it returns the card. If not, it finds the best possible match and returns it, along with a number of other possible "close" matches.

Additionally, you can pass something like {{Sphinx:30:3}} this is in the pattern of "cardname:number of results: number of columns". It will return 30 of the nearest matching card names, laid out into 3 clolumns.

##Future 
Check the issue spage. We're adding searching for nicknames, type/subtype, cmc, text, and color.
