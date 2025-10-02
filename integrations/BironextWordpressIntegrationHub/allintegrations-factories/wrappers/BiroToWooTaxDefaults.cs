using core.tools.attributemapper;

namespace allintegrations_factories.wrappers
{
    public class BiroToWooTaxDefaults
    {
        public BiroTaxToWooTax taxdefaults()
        {
            // TOREJ OD DAVKOV ZAENKRAT PODPIRAMO SAMO TO!!! - VI SI JIH USTVARITE SAMI, SAMO POVEJTE MI KAKO SE BIROKRAT ATRIBUTI PRESLIKAJO!
            BiroTaxToWooTax tax = new BiroTaxToWooTax("SifraDavka", "tax_class");
            tax.AddMapping("1    22 DDV osnovna stopnja", "standard");
            tax.AddMapping("2   9.5 DDV znižana stopnja", "reduced-rate");
            tax.AddMapping("3     0 DDV oproščen promet", "zero-rate");
            tax.AddMapping("4    22 DDV osnovna stopnja            Storitev", "standard");
            tax.AddMapping("5   9.5 DDV znižana stopnja            Storitev", "reduced-rate");
            tax.AddMapping("6     0 DDV oproščen promet            Storitev", "zero-rate");
            tax.AddMapping("7   9.5 Prejemniki plačniki DDV        Storitev", "reduced-rate");
            tax.AddMapping("8     8 Pavšalno nadomestilo           Storitev", ""); // TALE JE KAJ SPLOH?
            tax.AddMapping("9    22 Prejemniki plačniki DDV        Storitev", "standard");
            tax.AddMapping("A     0 DDV drug neobdavč. promet      Storitev", "zero-rate");
            tax.AddMapping("B     0 DDV neobdavčljiv promet        Storitev", "zero-rate");
            tax.AddMapping("C     5 DDV znižana stopnja", ""); // TALE JE KAJ SPLOH?;
            return tax;
        }
    }
}
