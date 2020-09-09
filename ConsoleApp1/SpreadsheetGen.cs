using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Linq;

namespace YugiohPriceSpreadsheet
{
    class SpreadsheetGen
    {

        public void Run(string filepath)
        {
            List<Card> cardList = new List<Card>();
            if (!File.Exists(filepath))
            {
                Console.WriteLine("File " + filepath + " does not exist!");
                return;
            }
            else
            {
                using (TextFieldParser parser = new TextFieldParser(@filepath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        cardList.Append(new Card(fields[0], Int32.Parse(fields[1]), fields[2], fields[3], 0));
                        /*
                        foreach (string field in fields)
                        {
                           
                        }*/
                    }
                }
            // import .csv file from local dir
            // parse it into Card
            // for each card name, call the yugioh prices API and use the rarity and code to get the price
                // if the rarity and code can't get the price, default to the average price
                // set the price
                // repeat
            // re-export spreadsheet with price
        }

        public async Task<string> GetCardData(string cardName)
        {
            var baseAddress = new Uri("http://yugiohprices.com/api/");
            string responseData = "";
            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {

                using (var response = await httpClient.GetAsync("get_card_prices/" + cardName))
                {

                    responseData = await response.Content.ReadAsStringAsync();
                }
            }
            return responseData;
        }
    }
}
