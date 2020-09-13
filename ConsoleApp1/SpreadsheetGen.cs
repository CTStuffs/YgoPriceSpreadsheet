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

                            // load the card details from the input csv
                            var card = new Card
                            {
                                Name = csv.GetField("Name"),
                                Quantity = csv.GetField<int>("Quantity"),
                                Rarity = csv.GetField("Rarity"),
                                PackCode = csv.GetField("PackCode"),
                                Edition = csv.GetField("Edition"),
                                PriceIndividual = 0.0,
                                PriceTotal = 0.0
                            };

                            string cardJson = await GetRaw(card.Name);
                            string raritytemp = card.Rarity;
                            string packCodetemp = card.PackCode;
                            double priceTemp = card.PriceIndividual;

                            ParseRaw(card.Name, cardJson, ref  raritytemp, ref packCodetemp, ref priceTemp);

                            card.Rarity = raritytemp;
                            card.PackCode = packCodetemp;
                            card.PriceIndividual = priceTemp;
                            card.PriceTotal = card.PriceIndividual * card.PriceTotal;
                            // if (cardJson is bad), skip the next step



                           // card.Price = await GetPrice(card.Name, card.PackCode);
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
               
                // re-export spreadsheet with price
            }

            return;

        }

        public async Task<string> GetRaw(string cardName)
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

        public void ParseRaw(string cardName, string rawdata, ref string rarity, ref string packcode, ref double price)//, ref string edition)
        {
            /*
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

            */
            dynamic rawParsed = JObject.Parse(rawdata);

            if (rawParsed.status == "fail")
            {
                Console.WriteLine("Could not find " + cardName + " using the API! Skipping over this one...");
                return;
            }

            JArray arrayParsed = rawParsed.data;
            string pcode = packcode;

            if (String.IsNullOrEmpty(packcode))
            {
                Console.WriteLine("Could not find the pack code for " + cardName + ".Skipping parsing the rarity, pack code and edition data for " + cardName + "!");
                return;
            }
            else
            {
                //dynamic token = JObject.Parse(arrayParsed.SelectToken("$..data[?(@.print_tag contains '" + packcode + "')]").ToString());
                // dynamic token = arrayParsed["data"].Values<JProperty>().Where(m => m.price_tag == packcode);
                //dynamic token = arrayParsed["data"].Values<JProperty>().FirstOrDefault(x => x.Name == "price_tag").Value;
                JToken matchingToken = null;
                foreach (JToken item in arrayParsed.Children())
                {
                    var itemProperties = item.Children<JProperty>();
                    //you could do a foreach or a linq here depending on what you need to do exactly with the value
                   // var myElement = itemProperties.FirstOrDefault(x => x.Name == "url");
                   // var myElementValue = myElement.Value; ////This is a JValue type
                   if (item["print_tag"].Value<string>().Contains(packcode))
                   {
                        Console.WriteLine("Found the data for " + cardName + " with pack code " + packcode + "!");
                        matchingToken = item;
                        break;
                   }
                }
                dynamic matchingObject = null;
                if (matchingToken != null)
                {

                    matchingObject = JObject.Parse(matchingToken.ToString());
                    // dynamic raw =matchingToken["price_data"]["data"]["prices"]["average"].Value;
                    //price = raw.Value;
                    packcode = matchingObject.print_tag.Value;
                    price = matchingObject.price_data.data.prices.average.Value;

                    if (!String.IsNullOrEmpty(packcode))
                    {
                        rarity = matchingObject.rarity.Value;
                    }

                    if (price > 0)
                    {

                    }
                }
                else
                {
                    Console.WriteLine("Could not find an appropriate pack code for " + cardName + " via querying the API. Please check the spreadsheet and try again later.");
                }

            
                //JObject match;
                //var temp = arrayParsed["users"]["doe"];
                // if (arrayParsed["password"].ToString() == "john")
                //{
                //    match = temp.ToObject<JObject>();
                //}
                // JObject.Parse(arrayParsed.SelectToken(arrayParsed["price_data"].Select(s => s.ToString().Contains(pcode));
            }

            

            // if the rarity has been parsed, find it
            /*
            if (String.IsNullOrEmpty(rarity))
            {

            }*/
            /*
            if (String.IsNullOrEmpty(edition))
            {

            }*/

            /*
            if (String.IsNullOrEmpty(packcode))
            {
                Console.WriteLine("Pack code for " + cardName + " was empty! Skipping  parsing packcode data for this one...");
            }*/
        }

        //public async Task<string> GetCardData(string cardName)

            /*
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

            
        }

            return price;
        }*/
    }
}
