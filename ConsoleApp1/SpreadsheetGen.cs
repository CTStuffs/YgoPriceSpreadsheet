using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace YugiohPriceSpreadsheet
{
    class SpreadsheetGen
    {

        public async Task RunAsync(string filein, string fileout)
        {
            List<Card> cardList = new List<Card>();
            if (!File.Exists(filein))
            {
                Console.WriteLine("File " + filein + " does not exist!");
                return;
            }
            else
            {
                using (var reader = new StreamReader(filein))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        while (csv.Read())
                        {
                            
                            var card = new Card
                            {
                                Name = csv.GetField("Name"),
                                Quantity = csv.GetField<int>("Quantity"),
                                Rarity = csv.GetField("Rarity"),
                                PackCode = csv.GetField("PackCode"),
                                Price = 0.0
                            };
                            card.Price = await GetPrice(card.Name, card.PackCode);
                            cardList.Add(card);
                            //Console.WriteLine(csv.ToString());
                        }

                    }



                }


                using (var writer = new StreamWriter(fileout))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(cardList);
                    }
                }
                    /*
                    using (TextFieldParser parser = new TextFieldParser(filepath))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");
                        while (!parser.EndOfData)
                        {
                            string[] fields = parser.ReadFields();
                            cardList.Append(new Card(fields[0], Int32.Parse(fields[1]), fields[2], fields[3], 0));

                            foreach (string field in fields)
                            {

                            }
                        }
                    }
                    */

                /*
                    foreach (Card c in cardList)
                {
                    //await GetCardData(c.Name);
                }*/
                // import .csv file from local dir
                // parse it into Card
                // for each card name, call the yugioh prices API and use the rarity and code to get the price
                // if the rarity and code can't get the price, default to the average price
                // set the price
                // repeat
                // re-export spreadsheet with price
            }

            return;

        }
        //public async Task<string> GetCardData(string cardName)
        public async Task<double> GetPrice(string cardName, string packcode)
        {
            var baseAddress = new Uri("http://yugiohprices.com/api/");
            string responseData = "";
            double price = 0.0;
            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                HttpResponseMessage response = await httpClient.GetAsync("get_card_prices/" + cardName);

                if (response.IsSuccessStatusCode)
                {
                    responseData = await response.Content.ReadAsStringAsync();
                    dynamic jsonData = JObject.Parse(responseData);

                    //var index = jsonData.data.Select();
                    JArray temp = jsonData.data;
                    dynamic token = JObject.Parse(jsonData.SelectToken("$..data[?(@.print_tag == '" + packcode + "')]").ToString());
                    //dynamic token = JObject.Parse((string) jsonData.SelectToken("$..data[?(@.print_tag == '" + packcode + "')]"));

                    // int index = temp.FindIndex(x => x.print_tag == packCode);
                    //dynamic raw = JObject.Parse(token.ToString());
                    //price = Double.Parse(raw.price_data.data.prices.average);
                    dynamic raw = token.price_data.data.prices.average;
                    price = raw.Value;
                }

                /*
                using (var response = await httpClient.GetAsync("get_card_prices/" + cardName))
                {

                    responseData = await response.Content.ReadAsStringAsync();
                }*/
            }

            return price;
        }
    }
}
