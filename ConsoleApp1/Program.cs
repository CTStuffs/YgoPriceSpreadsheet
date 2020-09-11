using System;
using System.Net.Http;
using System.Threading.Tasks;
using YugiohPriceSpreadsheet;
using CommandLine;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        public class Options
        {
            [Option('f', "filein", Required = true, HelpText = "Set output to verbose messages.")]

            public string FileIn { get; set; }

            [Option('i', "fileout", Required = true, HelpText = "Set output to verbose messages.")]
            public string FileOut { get; set; }
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            var result = -2;
            Console.WriteLine("errors {0}", errs.Count());
            if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError))
                result = -1;
            Console.WriteLine("Exit code {0}", result);
            return;
        }
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(async o =>
            {
                //await GetCardData("raigeki");
                SpreadsheetGen gen = new SpreadsheetGen();
                gen.RunAsync(o.FileIn, o.FileOut).Wait();
                /*
                Task.Run(async () =>
                {
                   
                }).GetAwaiter().GetResult();*/

                Console.ReadLine();
            }).WithNotParsed(HandleParseError);

            
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
