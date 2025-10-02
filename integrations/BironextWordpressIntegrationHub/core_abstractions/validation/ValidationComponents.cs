using BiroWoocommerceHub;
using BiroWoocommerceHub.structs_wc_to_biro;
using core.logic.common_woo;
using core.tools.zalogaretriever;
using System;
using System.Collections.Generic;

namespace tests.tools
{
    public class ValidationComponents
    {
        // Components to be used during validation!

        ICountryMapper countryMapper;
        IVatIdParser vatIdParser;
        List<TestEqualAddition> testAdditions;
        IZalogaRetriever zaloga;

        public ValidationComponents(ICountryMapper countryMapper,
            IVatIdParser vatIdParser,
            List<TestEqualAddition> testAdditions,
            IZalogaRetriever zaloga) {
            if (countryMapper == null)
                throw new ArgumentNullException("countryMapper");
            this.countryMapper = countryMapper;
            if (countryMapper == null)
                throw new ArgumentNullException("vatIdParser");
            //if (zaloga == null)
            //    throw new ArgumentNullException("zaloga");
            this.vatIdParser = vatIdParser;
            this.testAdditions = testAdditions;
            this.zaloga = zaloga;
        }

        public static ValidationComponents NullObject()
        {
            return new ValidationComponents(
                new HardcodedCountryMapper(),
                new NopVatParser(),
                new List<TestEqualAddition>(),
                new NopZalogaRetriever());
        }

        public ICountryMapper CountryMapper { get => countryMapper; }
        public IVatIdParser VatIdParser { get => vatIdParser; }
        public List<TestEqualAddition> TestEqualAdditions { get => testAdditions; }
        public IZalogaRetriever Zaloga { get => zaloga; }
    }
}
