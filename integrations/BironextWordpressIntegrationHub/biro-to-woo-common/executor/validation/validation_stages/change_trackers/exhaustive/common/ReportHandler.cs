using biro_to_woo_common.error_handling.reports;
using core.logic.common_birokrat;
using System.Collections.Generic;

namespace biro_to_woo_common.executor.validation_stages.change_trackers.exhaustive.common
{
    public class ReportHandler
    {
        private readonly string sifraFieldName;

        public ReportHandler()
        {
            sifraFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla);
        }

        public List<IOperationReport> GenerateReports(HashSet<string> sifrasDiff, List<Dictionary<string, object>> biroItems)
        {
            List<IOperationReport> reports = new List<IOperationReport>();
            foreach (var item in biroItems)
            {
                string sifra = item[sifraFieldName].ToString();
                if (sifrasDiff.Contains(sifra))
                {
                    var report = new BiroToWooOperationReport(item, OperationOutcome.Success);
                    reports.Add(report);
                }
                else
                {
                    var report = new BiroToWooOperationReport(item, OperationOutcome.Ignore);
                    reports.Add(report);
                }
            }

            return reports;
        }
    }
}
