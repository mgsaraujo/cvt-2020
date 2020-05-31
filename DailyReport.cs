using System;

namespace cv_api
{
    //aguardando resultado (ou isolamento dom.)	Recuperados	internados em investigação	internados confirmados	obitos em invest	obitos confirmados
    public class DailyReport
    {
        public string Confirmed { get; set; }
        public string WaitingResults { get; set; }
        public string Recovered { get; set; }
        public string WaitingResultsAtHospital { get; set; }
        public string ConfirmedAtHospital { get; set; }
        public string Deaths { get; set; }
        public string Date { get; set; }
    }
}
