using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace cv_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private IMemoryCache _cache { get; set; }
        private const string SpreadSheetDataUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vTLHS5ON6wix2P6pplcynlXp0UUeMgoL8aiIa1z945xa-_3YJkbyepD0a9-ezHdqe1YrRB6OnZ3U2OP/pub?output=csv";

        private static ILogger<ReportController> _logger;

        public ReportController(ILogger<ReportController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IEnumerable<DailyReport>> Get()
        {
            var cacheEntry = _cache.GetOrCreateAsync<IEnumerable<DailyReport>>("cacheReg", async entry =>
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(3600));
                entry.SetPriority(CacheItemPriority.High);
                return await GetCSVData();
            });

            return await cacheEntry;
        }

        private static async Task<IEnumerable<DailyReport>> GetCSVData()
        {
            List<DailyReport> DailyReport = new List<DailyReport>();
            try
            {
                using (var client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(SpreadSheetDataUrl))
                using (HttpContent content = response.Content)
                using (var stream = (MemoryStream)await content.ReadAsStreamAsync())
                using (var streamReader = new StreamReader(stream))
                {
                    ParseCSVData(DailyReport, streamReader);
                }

                return DailyReport;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return DailyReport;
        }

        private static void ParseCSVData(List<DailyReport> DailyReport, StreamReader streamReader)
        {
            var skipFirstLine = true;
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                var parsedData = line.Split(",");
                if (skipFirstLine)
                {
                    skipFirstLine = false;
                    continue;
                }
                DailyReport.Add(new cv_api.DailyReport
                {
                    Date = parsedData[0],
                    Confirmed = parsedData[1],
                    WaitingResults = parsedData[2],
                    Recovered = parsedData[3],
                    WaitingResultsAtHospital = parsedData[4],
                    ConfirmedAtHospital = parsedData[5],
                    Deaths = parsedData[7]
                });
            }
        }
    }
}
