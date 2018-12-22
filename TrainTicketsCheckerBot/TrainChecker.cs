using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace TrainTicketsCheckerBot
{
    public class TrainChecker
    {
        private static readonly HttpClient HttpClient;
        private static readonly TimeSpan SleepTimeout;

        static TrainChecker()
        {
            string url =
                "https://rasp.rw.by/ru/route/?from=%D0%9C%D0%B8%D0%BD%D1%81%D0%BA&to=%D0%91%D0%B0%D1%80%D0%B0%D0%BD%D0%BE%D0%B2%D0%B8%D1%87%D0%B8&date=tomorrow&from_exp=2100000&from_esr=140210&to_exp=2100005&to_esr=138901";
            HttpClient = new HttpClient { BaseAddress = new Uri(url) };
            SleepTimeout = new TimeSpan(0, 1, 0);
        }

        public delegate void AvailableTrainFound();

        public event AvailableTrainFound OnAvailableTrainFound;

        public void Run()
        {
            Task.Run(CheckTrains);
        }

        private async Task CheckTrains()
        {
            string startTime = "12:40";
            while (true)
            {
                var response = await HttpClient.GetAsync("");
                string content = await response.Content.ReadAsStringAsync();
                var document = new HtmlDocument();
                document.LoadHtml(content);
                var trainsTable = document.DocumentNode.SelectSingleNode("//tbody");
                var trainRows = trainsTable.SelectNodes("./tr");
                foreach (HtmlNode trainRow in trainRows)
                {
                    HtmlNode startTimeElement = trainRow.SelectSingleNode("./td/b[@class='train_start-time']");
                    if (startTimeElement.InnerHtml == startTime)
                    {
                        var priceDetailsNode = trainRow.SelectSingleNode("./td[@class='train_item train_details non_regional_only']/ul");
                        if (priceDetailsNode != null)
                        {
                            OnAvailableTrainFound?.Invoke();
                        }

                        break;
                    }
                }
                Thread.Sleep(SleepTimeout);
            }
        }
    }
}
