using BiroWooHub.logic.integration;
using System;
using System.Collections.Generic;
using System.Text;

namespace core.structs {
    public class TestingConfiguration {
        TestingConfigurationBiroToWoo biroToWoo;
        TestingConfigurationWooToBiro wooToBiro;

        public TestingConfiguration(TestingConfigurationBiroToWoo biroToWoo, TestingConfigurationWooToBiro wooToBiro) {
            this.BiroToWoo = biroToWoo;
            this.WooToBiro = wooToBiro;
        }

        public TestingConfigurationBiroToWoo BiroToWoo { get => biroToWoo; set => biroToWoo = value; }
        public TestingConfigurationWooToBiro WooToBiro { get => wooToBiro; set => wooToBiro = value; }
    }

    public class TestingConfigGenHelper
    {
        public static TestingConfiguration GetTestingConfiguration(IIntegration integ, string ProductTestsetRetrieverImpl, List<string> orderStatuses)
        {
            string companyName = integ.Name;
            return new TestingConfiguration(
                new TestingConfigurationBiroToWoo(ProductTestsetRetrieverImpl),
                new TestingConfigurationWooToBiro(orderStatuses, companyName, $"{companyName}/narocila"));
        }
    }

    public class TestingConfigurationBiroToWoo {

        string concreteProductTestsetRetrieverImplementation = "";
        public bool allowMultipleProductsWithSameSkuOnWebshop;

        public TestingConfigurationBiroToWoo(string concreteProductTestsetRetrieverImplementation, bool allowMultipleProductsWithSameSkuOnWebshop = false) {
            this.ConcreteProductTestsetRetrieverImplementation = concreteProductTestsetRetrieverImplementation;
            if (string.IsNullOrEmpty(ConcreteProductTestsetRetrieverImplementation)) {
                this.concreteProductTestsetRetrieverImplementation = "DEFAULT";
            }
            this.allowMultipleProductsWithSameSkuOnWebshop = allowMultipleProductsWithSameSkuOnWebshop;
        }

        public string ConcreteProductTestsetRetrieverImplementation { get => concreteProductTestsetRetrieverImplementation; set => concreteProductTestsetRetrieverImplementation = value; }

    
    }

    public class TestingConfigurationWooToBiro {
        List<string> testedOrderStatusSequence;
        string testOrdersDataPath;
        string companyName;
        DateTime startDate;
        DateTime endDate;

        public TestingConfigurationWooToBiro(List<string> testedOrderStatusSequence, 
            string companyName, 
            string testOrdersDataPath) {
            this.TestedOrderStatusSequence = testedOrderStatusSequence;
            this.TestResultsPath = testOrdersDataPath;
            this.companyName = companyName;

            StartDate = DateTime.MinValue;
            EndDate = DateTime.MaxValue;
        }

        public List<string> TestedOrderStatusSequence { get => testedOrderStatusSequence; set => testedOrderStatusSequence = value; }
        public string TestResultsPath { get => testOrdersDataPath; set => testOrdersDataPath = value; }
        public string CompanyName { get => companyName; set => companyName = value; }
        public DateTime StartDate { get => startDate; set => startDate = value; }
        public DateTime EndDate { get => endDate; set => endDate = value; }
    }
}
