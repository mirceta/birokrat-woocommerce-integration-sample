using BironextWordpressIntegrationHub.structs;
using BiroWoocommerceHub.flows;
using BiroWooHub.logic.integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tests.tools;

namespace tests.tests.estrada
{
    public class VerifyWhetherWoocommerceRoundingIsOk {


        Func<double, double, double> fixtax;
        Func<double, double, double> fixvalue;
        Func<double, double> round = (x) => Math.Round(100.0 * x) / 100.0;
        public VerifyWhetherWoocommerceRoundingIsOk() {

            // presume total is correct and need to adjust tax
            Func<double, double, double> taxrate = (total, tax) => Math.Round(100.0 * tax / total) / 100.0;
            fixtax = (total, tax) => total * taxrate(total, tax);
            fixvalue = (total, tax) => total;


            // presume tax is correct and need to adjust total
            //fixtax = (total, tax) => tax;
            //fixvalue = (total, tax) => tax / taxrate(total, tax);



            // tax
            // taxrate = adjust(tax / total)
            // total = ?

            // total * (1 + taxrate) = subtotal
            // total + tax = subtotal

            // total * (1 + taxrate) = total + tax
            // total + total * taxrate = total + tax
            // total * taxrate = tax
            // total = tax / taxrate

        }

        public async Task<WoocommerceOrder> Act(IIntegration integration, WoocommerceOrder order) {

            
            



            Console.WriteLine();
            return null;
        }

        public string AreOrderValuesConsistent(WoocommerceOrder order) {
            double realtotal = round(Tools.ParseDoubleBigBrainTime(order.Data.Total));
            double realtax = round(Tools.ParseDoubleBigBrainTime(order.Data.TotalTax));

            double accumulation = 0;
            double accumulationtax = 0;
            foreach (var item in order.Items) {
                double total = round(Tools.ParseDoubleBigBrainTime(item.Total));
                double tax = round(Tools.ParseDoubleBigBrainTime(item.TotalTax));

                accumulation += total + tax;
                accumulationtax += tax;
            }

            double shippingtotal = round(Tools.ParseDoubleBigBrainTime(order.Data.ShippingTotal));
            double shippingtax = round(Tools.ParseDoubleBigBrainTime(order.Data.ShippingTax));

            accumulation += shippingtotal + shippingtax;
            accumulationtax += shippingtax;

            accumulation = round(accumulation);
            accumulationtax = round(accumulationtax);

            if (realtotal != accumulation) {
                return $"realtotal: {realtotal}, cumputedtotal: {accumulation}";
            }
            if (accumulationtax != realtax) {
                return $"realtax: {realtax}, cumputedtax: {accumulationtax}";
            }
            return null;
        }

        bool FixTotals(WoocommerceOrder order) {
            var items = InitializeItems(order);
            var total = InitializeTotal(order);

            Func<double, double> round = (x) => Math.Round(100.0 * x) / 100.0;
            double birogreater = round(total.currentTotal) - round(total.originalTotal) + round(total.currentTax) - round(total.originalTax);

            if (birogreater == 0) {
                return true;
            } else if (birogreater > 0) {
                AdjustUntilFits(items, total, -0.00000000000001);
            } else if (birogreater < 0) {
                AdjustUntilFits(items, total, 0.00000000000001);
            }

            return false;
        }

        void AdjustUntilFits(List<Item> items, Total total, double diff) {
            var limited = new HashSet<string>();

            while (true) {
                foreach (var item in items) {

                    //Console.WriteLine("---------------NEW LAP--------------");

                    if (limited.Contains(item.id)) continue;
                    if (!IsRound2TheSame(item.originalPrice, item.currentPrice + diff)) {
                        limited.Add(item.id);
                    }
                    if (limited.Count == items.Count) {
                        //Console.WriteLine("!!!!!!!!!!!!!!!HERE IS NO SOLUTION BECAUSE WE'VE TRIED INCREASING ALL OF THE POSTAVKAS!!!!!!!!!!!!!!!");
                        throw new Exception("THERE IS NO SOLUTION BECAUSE WE'VE TRIED INCREASING ALL OF THE POSTAVKAS");
                    }
                    item.currentPrice += diff;

                    UpdateTotals(items, total);
                    //Console.WriteLine(GetState(items, total));
                    if (total.currentTax >= 28.544999999999) {
                        Console.WriteLine();
                    }
                    if (IsRound2TheSame(total.currentTotal + total.currentTax, total.originalTotal) &&
                        IsRound2TheSame(total.currentTax, total.originalTax)) {
                        //Console.WriteLine("!!!!!!!!!!!!!!!FOUND SOLUTION!!!!!!!!!!!!!!!");
                        return; // FOUND SOLUTION!
                    }
                }
            }
        }

        void UpdateTotals(List<Item> items, Total total) {
            double accum = 0.0;
            double accumtax = 0.0;
            foreach (var item in items) {
                accum += item.currentPrice;
                accumtax += item.currentPrice * item.taxrate;
            }
            total.currentTotal = accum;
            total.currentTax = accumtax;
        }
        bool IsRound2TheSame(double value1, double value2) {
            return round(value1) == round(value2);
        }

        List<Item> InitializeItems(WoocommerceOrder order) {
            var items = new List<Item>();

            // items
            foreach (var item in order.Items) {

                double price = Tools.ParseDoubleBigBrainTime(item.Subtotal);
                double tax = Tools.ParseDoubleBigBrainTime(item.SubtotalTax);
                double taxrate = round(tax / price);
                double quantity = item.Quantity;

                var myitem = new Item() {
                    id = item.Id + "",
                    originalPrice = price,
                    currentPrice = price,
                    taxrate = taxrate,
                    quantity = quantity
                };


                items.Add(myitem);
            }

            // shipping
            double shippingtotal = Tools.ParseDoubleBigBrainTime(order.Data.ShippingTotal);
            double shippingtax = Tools.ParseDoubleBigBrainTime(order.Data.ShippingTax);
            double taxrate1 = round(shippingtax / shippingtotal);
            items.Add(new Item() {
                id = "SHIPPING",
                originalPrice = shippingtotal,
                currentPrice = shippingtotal,
                taxrate = taxrate1
            });

            return items;
        }

        Total InitializeTotal(WoocommerceOrder order) {
            double some = Tools.ParseDoubleBigBrainTime(order.Data.Total);
            double chome = Tools.ParseDoubleBigBrainTime(order.Data.TotalTax);

            double accumulation = 0;
            double accumulationtax = 0;
            foreach (var item in order.Items) {
                double total = Tools.ParseDoubleBigBrainTime(item.Subtotal);
                double tax = Tools.ParseDoubleBigBrainTime(item.SubtotalTax);
                total = fixvalue(total, tax);
                tax = fixtax(total, tax);

                accumulation += total + tax;
                accumulationtax += tax;
            }

            double shippingtotal = Tools.ParseDoubleBigBrainTime(order.Data.ShippingTotal);
            double shippingtax = Tools.ParseDoubleBigBrainTime(order.Data.ShippingTax);
            shippingtotal = fixvalue(shippingtotal, shippingtax);
            shippingtax = fixtax(shippingtotal, shippingtax);

            accumulation += shippingtotal + shippingtax;
            accumulationtax += shippingtax;

            return new Total() {
                originalTotal = Tools.ParseDoubleBigBrainTime(order.Data.Total),
                originalTax = Tools.ParseDoubleBigBrainTime(order.Data.TotalTax),
                currentTotal = accumulation,
                currentTax = accumulationtax
            };
        }

        string GetState(List<Item> items, Total total) {

            string tmp = "";
            tmp += string.Join("\n", items.Select(x => $"{x.id} currPrice: {x.currentPrice} origiPrice: {x.originalPrice}")) + "\n";
            tmp += $"Total: {total.currentTotal + total.currentTax}, originalTotal: {total.originalTotal}\n";
            tmp += $"TotalTax: {total.currentTax}, originalTotal: {total.originalTax}\n";
            return tmp;

        }

    }

    class Item {
        public string id;
        public double originalPrice;
        public double currentPrice;
        public double taxrate;
        public double quantity;
    }

    class Total {
        public double originalTotal;
        public double currentTotal;
        public double originalTax;
        public double currentTax;
    }
}
