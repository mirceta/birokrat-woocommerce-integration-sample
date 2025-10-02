using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiClient.utils
{
    public class BironextApiPathHelper
    {

        public static Dictionary<BirokratDocumentType, string> biroDoctTypeMap = new Dictionary<BirokratDocumentType, string>() {
                { BirokratDocumentType.RACUN, "racun"},
                { BirokratDocumentType.PREDRACUN, "predracun"},
                { BirokratDocumentType.DOBAVNICA, "dobavnica"},
                { BirokratDocumentType.NAROCILO, "narocilokupca"},
                { BirokratDocumentType.UNASSIGNED, "" }
            };


        public static BirokratDocumentType GetTypeByString(string type)
        {
            var dic = biroDoctTypeMap.ToList().ToDictionary(x => x.Value, x => x.Key);
            if (!dic.ContainsKey(type))
            {
                throw new IntegrationProcessingException("This birokrat document type does not exist");
            }
            return dic[type];
        }

        public static string GetStringByType(BirokratDocumentType type)
        {
            return biroDoctTypeMap[type];
        }

        public static string GetVnosByType(BirokratDocumentType documentType)
        {
            return GetVnosByType(biroDoctTypeMap[documentType]);
        }

        public static string GetCumulativeByDocumentType(BirokratDocumentType documentType)
        {
            return GetCumulativeByDocumentType(biroDoctTypeMap[documentType]);
        }

        public static string GetCumulativeFromDateParameter(BirokratDocumentType documentType)
        {
            return GetCumulativeFromDateParameter(biroDoctTypeMap[documentType]);
        }

        public static string GetCumulativeUntilDateParameter(BirokratDocumentType documentType)
        {
            return GetCumulativeUntilDateParameter(biroDoctTypeMap[documentType]);
        }

        public static string GetCumulativeAdditionalNumberParameter(BirokratDocumentType documentType)
        {
            return GetCumulativeAdditionalNumberParameter(biroDoctTypeMap[documentType]);
        }

        public static string GetCumulativeData_DocNumberField(BirokratDocumentType documentType)
        {
            return GetCumulativeData_DocNumberField(biroDoctTypeMap[documentType]);
        }

        public static string GetCumulativeData_DocAdditionalNumberField(BirokratDocumentType documentType)
        {
            return GetCumulativeData_DocAdditionalNumberField(biroDoctTypeMap[documentType]);
        }


        public static string GetVnosByType(string documentType)
        {
            if (documentType == "racun")
            {
                return @"poslovanje/racuni/izstavitevinpregled";
            }
            else if (documentType == "narocilokupca")
            {
                return "skladisce/narocilokupca/izstavitevinpregled";
            }
            else if (documentType == "predracun")
            {
                return "poslovanje/predracun/ponudba/izstavitevinpregled";
            }
            else if (documentType == "dobavnica")
            {
                return "skladisce/dobavnica/izstavitevinpregled";
            }
            throw new BirokratDocumentTypeNotSupported(documentType);
        }

        public static string GetCumulativeByDocumentType(string documentType)
        {
            if (documentType == "racun")
            {
                return @"poslovanje\racuni\kumulativnipregled";
            }
            else if (documentType == "predracun")
            {
                return "poslovanje/predracun/ponudba/kumulativnipregled";
            }
            else if (documentType == "narocilokupca")
            {
                return "skladisce/narocilokupca/kumulativnipregled";
            }
            else if (documentType == "dobavnica")
            {
                return "skladisce/dobavnica/kumulativnipregled";
            }
            throw new BirokratDocumentTypeNotSupported(documentType);
        }

        public static string GetCumulativeFromDateParameter(string documentType)
        {
            if (documentType == "racun")
            {
                return @"Oddatumaizstavitve";
            }
            else if (documentType == "predracun")
            {
                return "OdIzstavitve";
            }
            else if (documentType == "narocilokupca")
            {
                return "DateText1";
            }
            else if (documentType == "dobavnica")
            {
                return "DateText1";
            }
            throw new BirokratDocumentTypeNotSupported(documentType);
        }

        public static string GetCumulativeUntilDateParameter(string documentType)
        {
            if (documentType == "racun")
            {
                return @"Dodatumaizstavitve";
            }
            else if (documentType == "predracun")
            {
                return "DoIzstavitve";
            }
            else if (documentType == "narocilokupca")
            {
                return "DateText2";
            }
            else if (documentType == "dobavnica")
            {
                return "DateText2";
            }
            throw new BirokratDocumentTypeNotSupported(documentType);
        }

        public static string GetCumulativeAdditionalNumberParameter(string documentType)
        {
            if (documentType == "racun")
            {
                return @"Dodatekstevilke";
            }
            else if (documentType == "predracun")
            {
                return "Dodatekstevilke";
            }
            else if (documentType == "narocilokupca")
            {
                return "DodatekStevilke";
            }
            else if (documentType == "dobavnica")
            {
                return "DodatekStevilke";
            }
            throw new BirokratDocumentTypeNotSupported(documentType);
        }

        public static string GetCumulativeData_DocNumberField(string documentType)
        {
            if (documentType == "dobavnica")
            {
                return "St.";
            }
            else if (documentType == "predracun")
            {
                return "Številka predračuna";
            }
            else if (documentType == "narocilokupca")
            {
                return "St.";
            }
            else if (documentType == "racun")
            {
                return "Številka";
            }
            throw new BirokratDocumentTypeNotSupported(documentType);
        }

        public static string GetCumulativeData_DocAdditionalNumberField(string documentType)
        {
            if (documentType == "dobavnica")
            {
                return "Dodatna številka";
            }
            else if (documentType == "narocilokupca")
            {
                return "Dodatna številka";
            }
            throw new BirokratDocumentTypeNotSupported(documentType);

        }

    }

    public class BirokratDocumentTypeNotSupported : Exception
    {
        public BirokratDocumentTypeNotSupported(string message) : base(message) { }
    }

    public enum BirokratDocumentType
    {
        RACUN,
        PREDRACUN,
        NAROCILO,
        DOBAVNICA,
        UNASSIGNED
    }

}
