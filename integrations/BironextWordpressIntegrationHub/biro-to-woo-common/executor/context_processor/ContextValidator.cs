using biro_to_woo_common.error_handling.reports;
using biro_to_woo_common.executor.validation_stages;
using core.logic.common_birokrat;
using core.structs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.executor.context_processor
{
    public class ReportResult
    {
        public OperationOutcome Outcome { get; set; }
        public IOperationReport Report { get; set; }
        public Dictionary<string, object> InspectionObject { get; set; }
    }

    public class ContextValidator : IContextValidator
    {
        private Dictionary<string, ReportResult> itemResults = new Dictionary<string, ReportResult>();
        private List<IBiroToOutValidationStage> stages;

        public ContextValidator(List<IBiroToOutValidationStage> stages)
        {
            this.stages = stages;
        }

        public async Task Validate(BiroOutComparisonContext originalContext, CancellationToken token)
        {

            // preserve original context
            var context = new BiroOutComparisonContext()
            {
                biroItems = new List<Dictionary<string, object>>(originalContext.biroItems),
                outItems = originalContext.outItems // only one copy, won't be changed
            };

            foreach (var stage in stages)
            {
                if (token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                var stageContext = new BiroOutComparisonContext()
                {
                    biroItems = new List<Dictionary<string, object>>(context.biroItems), // deep copy for each stage
                    outItems = context.outItems // same across all stages
                };

                var reports = await stage.Execute(stageContext, token);
                var successBiroItems = new List<Dictionary<string, object>>();

                foreach (var report in reports)
                {
                    itemResults[report.Id] = new ReportResult
                    {
                        Outcome = report.OperationOutcome,
                        Report = report,
                        InspectionObject = (Dictionary<string, object>)report.ObjectUnderInspection
                    };

                    if (report.OperationOutcome == OperationOutcome.Success)
                    {
                        if (stageContext.biroItems.Contains(report.ObjectUnderInspection))
                            successBiroItems.Add((Dictionary<string, object>)report.ObjectUnderInspection);
                    }
                }

                context.biroItems = successBiroItems; // replace with only successful items
            }
        }

        public List<IOperationReport> GetFailedItems()
        {
            return itemResults.Values.Where(x => x.Outcome == OperationOutcome.Warning || x.Outcome == OperationOutcome.Error).Select(x => x.Report).ToList();
        }

        public List<string> GetSuccessfulItemSifras()
        {
            return itemResults.Where(x => x.Value.Outcome == OperationOutcome.Success).Select(x => x.Key).ToList();
        }

        public List<IOperationReport> GetSuccessfulReports()
        {
            return itemResults.Values.Where(x => x.Outcome == OperationOutcome.Success).Select(x => x.Report).ToList();
        }

        public List<string> GetNeutralItems()
        {
            string sifraFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla);
            return itemResults.Values.Where(x => x.Outcome == OperationOutcome.Ignore).Select(x => x.InspectionObject[sifraFieldName] as string).ToList();
        }
    }

}
