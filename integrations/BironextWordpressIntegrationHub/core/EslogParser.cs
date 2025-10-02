using BironextWordpressIntegrationHub.structs;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace tests.tools
{
    public class EslogParser {
        public EslogParser() {
        }

        public ComparisonAttributes ParseComparisonAttributes(WoocommerceOrder order, string xml) {

            List<HtmlNode> lst = new List<HtmlNode>();

            HtmlDocument htmlDoc = new HtmlDocument();

            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.LoadHtml(xml);

            if (htmlDoc.DocumentNode != null) {
                HtmlNode racun = htmlDoc.DocumentNode.ChildNodes.ToList().Where(x => x.Name == "izdaniracunenostavni").Single();
                racun = racun.ChildNodes.ToList().Where(x => x.Name == "racun").Single();


                ComparisonAttributes attrs = new ComparisonAttributes();

                var nodes = GetNodes(racun, "podatkipodjetja");
                var partnerNodes = nodes.Select(x => {
                    var nazivpartnerja = GetNode(x, "nazivnaslovpodjetja:nazivpartnerja:nazivpartnerja1").InnerText + "\n";
                    nazivpartnerja += GetNode(x, "nazivnaslovpodjetja:nazivpartnerja:nazivpartnerja2").InnerText;

                    var vrstapartnerja = GetNode(x, "nazivnaslovpodjetja:vrstapartnerja").InnerText;

                    var ulicapartnerja = GetNode(x, "nazivnaslovpodjetja:ulica:ulica1").InnerText + "\n";
                    ulicapartnerja += GetNode(x, "nazivnaslovpodjetja:ulica:ulica2").InnerText;

                    string krajpartnerja = GetNode(x, "nazivnaslovpodjetja:kraj").InnerText;
                    string postapartnerja = GetNode(x, "nazivnaslovpodjetja:postnastevilka").InnerText;

                    string davcna = "";
                    try {
                        var ksoad = GetNodes(x, "referencnipodatkipodjetja").Where(x =>
                            !string.IsNullOrEmpty(GetNode(x, "podatekpodjetja").InnerText))
                            .Select(x => GetNode(x, "podatekpodjetja").InnerText).ToList();

                        if (ksoad.Count != 0)
                            davcna = ksoad.First();
                    } catch (Exception ex) { // do nothing just unable to parse davcna... 
                    }
                    var kodadrzave = GetNode(x, "nazivnaslovpodjetja:kodadrzave").InnerText;

                    return new Partner() {
                        davcna = davcna,
                        kodadrzave = kodadrzave,
                        naziv = nazivpartnerja,
                        mesto = krajpartnerja,
                        postnastevilka = postapartnerja,
                        ulica = ulicapartnerja,
                        vrsta = vrstapartnerja
                    };
                }).ToList();

                // buyer
                attrs.Davcna = partnerNodes.Where(x => x.vrsta == "BY").Select(x => x.davcna).First();
                attrs.Nazivbuyer = partnerNodes.Where(x => x.vrsta == "BY").First().naziv;
                attrs.Drzavabuyer = partnerNodes.Where(x => x.vrsta == "BY").Select(x => x.kodadrzave).First();
                attrs.Postabuyer = partnerNodes.Where(x => x.vrsta == "BY").Select(x => x.mesto).First();
                attrs.Postnastevilkabuyer = partnerNodes.Where(x => x.vrsta == "BY").Select(x => x.postnastevilka).First();
                attrs.Naslovbuyer = partnerNodes.Where(x => x.vrsta == "BY").Select(x => x.ulica).First();


                // shipping
                if (partnerNodes.Where(x => x.vrsta == "DP").Count() == 1) { 
                    attrs.Nazivshipping = partnerNodes.Where(x => x.vrsta == "DP").First().naziv;
                    attrs.Drzavashipping = partnerNodes.Where(x => x.vrsta == "DP").First().kodadrzave;
                    attrs.Postashipping = partnerNodes.Where(x => x.vrsta == "DP").First().mesto;
                    attrs.Postnastevilkashipping = partnerNodes.Where(x => x.vrsta == "DP").First().postnastevilka;
                    attrs.Naslovshipping = partnerNodes.Where(x => x.vrsta == "DP").First().ulica;
                }


                // stevilo postavk
                attrs.SteviloPostavk = GetNodes(racun, "postavkeracuna").Count;
                attrs.SifrePostavk = GetNodes(racun, "postavkeracuna").Select(x => GetNode(x, "dodatnaidentifikacijaartikla:stevilkaartikladodatna").InnerText).ToList();
                
                var tmpzneski = GetNodes(racun, "povzetekzneskovracuna")
                    .Select(x => {
                        string vrsta = GetNode(x, "zneskiracuna:vrstazneska").InnerHtml;
                        string znesek = GetNode(x, "zneskiracuna:znesekracuna").InnerHtml;
                        return Tuple.Create<string, string>(vrsta, znesek);
                    });
                foreach (var tmp in tmpzneski) {
                    switch (tmp.Item1) {
                        case "176":
                            attrs.Totaltax = tmp.Item2;
                            break;
                        case "124":
                            attrs.Totaltax = tmp.Item2;
                            break;
                        case "53":
                            // popust
                            break;
                        case "79":
                            // znesek brez davka brez popusta vracunanega
                            break;
                        case "125":
                            // znesek brez davkom z vracunannim popistom
                            break;
                        case "86":
                            attrs.Total = tmp.Item2;
                            break;
                        case "9":
                            attrs.Total = tmp.Item2;
                            break;
                    }
                }

                return attrs;
            }

            throw new Exception("Invalid xml");
        }

        public HtmlNode GetNode(HtmlNode node, string path) {
            return GetNodesEnumerable(node, path).Single();
        }

        public List<HtmlNode> GetNodes(HtmlNode node, string path) {
            return GetNodesEnumerable(node, path).ToList();
        }

        public IEnumerable<HtmlNode> GetNodesEnumerable(HtmlNode node, string path) {
            if (path.Contains(":")) {

                string[] parts = path.Split(":");
                HtmlNode bodynode = node.ChildNodes.ToList().Where(x => x.Name == parts[0]).Single();
                return GetNodesEnumerable(bodynode, path.Substring(path.IndexOf(":") + 1));

            } else {
                return node.ChildNodes.ToList().Where(x => x.Name == path);
            }
        }
    }
}
