using System;
using System.Collections.Generic;
using System.Text;

namespace core.logic.common_birokrat {
    public class BirokratNameOfFieldInFunctionality {

        public static string SifrantArtiklov(BirokratField field) {
            switch (field) {
                case BirokratField.SifraArtikla:
                    return "txtSifraArtikla";
                case BirokratField.Barkoda:
                    return "txtBarKoda";
                case BirokratField.Barkoda2:
                    return "Barkoda2";
                case BirokratField.Barkoda3:
                    return "Barkoda3";
                case BirokratField.Barkoda4:
                    return "Barkoda4";
                case BirokratField.Barkoda5:
                    return "Barkoda5";
                case BirokratField.PCsPD:
                    return "PCsPD";
                case BirokratField.Internet:
                    return "chkInternet";
                default:
                    throw new Exception("Birokrat field not recognized");
            }
        }

        public static BirokratField ReverseSifrantArtiklov(string fieldName)
        {
            Dictionary<string, BirokratField> reverseMappings = new Dictionary<string, BirokratField>()
            {
                { "txtSifraArtikla", BirokratField.SifraArtikla },
                { "txtBarKoda", BirokratField.Barkoda },
                { "Barkoda2", BirokratField.Barkoda2 },
                { "Barkoda3", BirokratField.Barkoda3 },
                { "Barkoda4", BirokratField.Barkoda4 },
                { "Barkoda5", BirokratField.Barkoda5 },
                { "PCsPD", BirokratField.PCsPD },
                { "chkInternet", BirokratField.Internet }
            };

            if (reverseMappings.TryGetValue(fieldName, out BirokratField field))
            {
                return field;
            }

            throw new Exception("Field name not recognized");
        }

        public static string KumulativaStanjeZaloga(BirokratField field) {
            switch (field) {
                case BirokratField.SifraArtikla:
                    return "Artikel";
                case BirokratField.Barkoda:
                    return "Barkoda";
                case BirokratField.Barkoda2:
                    return "Barkoda2";
                case BirokratField.Barkoda3:
                    return "Barkoda3";
                case BirokratField.Barkoda4:
                    return "Barkoda4";
                case BirokratField.Barkoda5:
                    return "Barkoda5";
                default:
                    throw new Exception("Birokrat field not recognized");
            }
        }

        public static string KumulativaPodrobniPregledArtiklov(BirokratField field) {
            switch (field) {
                case BirokratField.SifraArtikla:
                    return "Artikel";
                case BirokratField.Barkoda:
                    return "Barkoda";
                case BirokratField.Barkoda2:
                    return "Barkoda 2";
                case BirokratField.Barkoda3:
                    return "Barkoda 3";
                case BirokratField.Barkoda4:
                    return "Barkoda 4";
                case BirokratField.Barkoda5:
                    return "Barkoda 5";
                case BirokratField.PCsPD:
                    return "Cena z davkom"; 
                case BirokratField.Internet:
                    return "Prenesi v e-shop";
                default:
                    throw new Exception("Birokrat field not recognized");
            }
        }

        public static BirokratField ReverseKumulativaStanjeZaloga(string fieldName)
        {
            Dictionary<string, BirokratField> reverseMappings = new Dictionary<string, BirokratField>()
                {
                    { "Artikel", BirokratField.SifraArtikla },
                    { "Barkoda", BirokratField.Barkoda },
                    { "Barkoda2", BirokratField.Barkoda2 },
                    { "Barkoda3", BirokratField.Barkoda3 },
                    { "Barkoda4", BirokratField.Barkoda4 },
                    { "Barkoda5", BirokratField.Barkoda5 }
                };

            if (reverseMappings.TryGetValue(fieldName, out BirokratField field))
            {
                return field;
            }

            throw new Exception("Field name not recognized");
        }

        public static BirokratField ReverseKumulativaPodrobniPregledArtiklov(string fieldName)
        {
            Dictionary<string, BirokratField> reverseMappings = new Dictionary<string, BirokratField>()
                {
                    { "Artikel", BirokratField.SifraArtikla },
                    { "Barkoda", BirokratField.Barkoda },
                    { "Barkoda 2", BirokratField.Barkoda2 },
                    { "Barkoda 3", BirokratField.Barkoda3 },
                    { "Barkoda 4", BirokratField.Barkoda4 },
                    { "Barkoda 5", BirokratField.Barkoda5 },
                    { "Cena z davkom", BirokratField.PCsPD },
                    { "Prenesi v e-shop", BirokratField.Internet }
                };

            if (reverseMappings.TryGetValue(fieldName, out BirokratField field))
            {
                return field;
            }

            throw new Exception("Field name not recognized");
        }

    }
}
