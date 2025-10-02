using biro_to_woo_common.error_handling.reports;
using biro_to_woo_common.executor.validation_stages.change_trackers.exhaustive.common;
using BirokratNext;
using BiroWoocommerceHubTests;
using BiroWooHub.logic.integration;
using core.logic.common_birokrat;
using core.structs;
using core.tools.birokratops;
using core.tools.zalogaretriever;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.executor.validation_stages.change_trackers.exhaustive
{

    public class ArtikelChangeDetection_BTOStage : IBiroToOutValidationStage
    {
        
        bool verbose;
        bool include_articles_internetne;

        IMyLogger logger;

        public ArtikelChangeDetection_BTOStage(
            BirokratField skuToBirokrat,
            IMyLogger logger,
            bool addproducts_notonwebshop,
            bool verbose = true)
        {
            this.verbose = verbose;

            this.logger = logger;
            string strsifra = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla);
            string strskuToBirokrat = BirokratNameOfFieldInFunctionality.SifrantArtiklov(skuToBirokrat);
            this.changeDetector = new ChangeDetector(logger, strsifra, strskuToBirokrat, verbose, addproducts_notonwebshop);
        }
    
        ChangeDetector changeDetector;

        public async Task<List<IOperationReport>> Execute(BiroOutComparisonContext ctx, CancellationToken token)
        {

            HashSet<string> sifrasDiff = changeDetector.DetectChanges(ctx.outItems, ctx.biroItems, token);

            string sifraFieldName = BirokratNameOfFieldInFunctionality.SifrantArtiklov(BirokratField.SifraArtikla);

            return new ReportHandler().GenerateReports(sifrasDiff, ctx.biroItems);

        }

        public void Clear()
        {
            // do nothing
        }

        private void ConsolePrintout(string content)
        {
            if (verbose)
            {
                Console.WriteLine(content);
            }
        }
    }
}
