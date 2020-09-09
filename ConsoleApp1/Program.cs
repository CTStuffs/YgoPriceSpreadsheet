using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //await GetCardData("raigeki");

            Task.Run(async () =>
            {
                // Do any async anything you need here without worry
                Console.WriteLine(await GetCardData("raigeki"));
            }).GetAwaiter().GetResult();

            Console.ReadLine();
        }

        static async Task<string> GetCardData(string cardName)
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
