using biro_to_woo.loop;
using BiroWooHub.logic.integration;
using Microsoft.Extensions.Logging;
using si.birokrat.next.common.logging;
using System;
using System.Text;

namespace biro_to_woo_common.executor.validation_stages.change_trackers.exhaustive
{
    public class ExhaustiveArtikelChangeTrackerFactory
    {

        IMyLogger logger;
        int parallelTaskCount;

        public ExhaustiveArtikelChangeTrackerFactory(IMyLogger logger, int parallelTaskCount)
        {
            this.logger = logger;
            this.parallelTaskCount = parallelTaskCount;
        }

        public IBiroToOutValidationStage Create(IIntegration integration)
        {

            bool addprods = !integration.Options.Birotowoo_changetracker_dontincludearticlesthatarenotonwebshop;
            bool include_articles_internetne = false;

            ArtikelChangeDetection_BTOStage some = new ArtikelChangeDetection_BTOStage(
                integration.BiroToWoo.SkuBirokratField,
                logger,
                addprods,
                verbose: true);

            // YOU MUST NOT DROP THE DRAFTS BECAUSE OF THE FOLLOWING REASON:
            // IF A PRODUCT HAS ONLY DRAFTS, THEN IF YOU DROP THE DRAFTS THEN THE INTEGRATION
            // WILL SEE IT AS IF THE PRODUCT IS NOT ONLINE AND WILL ATTEMPT TO UPLOAD IT WHICH
            // IS INCORRECT BECAUSE THE PRODUCT IS ALREADY ONLINE BUT IS A DRAFT!
            // return new DropDrafts(some); -> COMPLETELY AND UTTERLY INCORRECT - NEVER NEVER NEVER DO THIS!
            return some;
        }
    }
}
