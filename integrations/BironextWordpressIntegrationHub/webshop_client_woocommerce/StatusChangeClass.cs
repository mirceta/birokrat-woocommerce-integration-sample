using BiroWoocommerceHubTests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace transfer_data.sql_accessors.order_transfer_creator.deps
{
    public class StatusChangeClass
    {

        IOutApiClient outclient;
        public StatusChangeClass(IOutApiClient client)
        {
            outclient = client;
        }

        public List<StatusChange> GetStatusChanges(string orderNotes)
        {
            var deserializedData = DeserializeOrderNotes(orderNotes);
            return ExtractStatusChanges(deserializedData);
        }

        private List<DeserializedOrderNote> DeserializeOrderNotes(string rawJson)
        {
            var definition = new { note = "", date_created_gmt = DateTime.MinValue, id = 0 };
            var deserializedData = JsonConvert.DeserializeAnonymousType(rawJson, new[] { definition });

            return deserializedData.Select(d => new DeserializedOrderNote
            {
                note = d.note,
                date_created_gmt = d.date_created_gmt,
                id = d.id
            }).ToList();
        }
        
        private List<StatusChange> ExtractStatusChanges(List<DeserializedOrderNote> orderNotes)
        {
            var statusChanges = new List<StatusChange>();

            foreach (var item in orderNotes)
            {
                var statusChange = ExtractStatusChangeFromNote(item.note, item.date_created_gmt);
                if (statusChange != null)
                {
                    statusChanges.Add(statusChange);
                }
            }

            return statusChanges;
        }

        private StatusChange ExtractStatusChangeFromNote(string note, DateTime dateCreated)
        {
            var regex = new Regex(@"Order status changed from (\w+) to (\w+).");
            var match = regex.Match(note);

            if (match.Success)
            {
                return new StatusChange
                {
                    From = match.Groups[1].Value.ToLower(),
                    To = match.Groups[2].Value.ToLower(),
                    EventDate = dateCreated
                };
            }


            if (Regex.IsMatch(note, "Stanje naro.*ila spremenjeno iz"))
            {
                var matches = new Matcher().FindMatches(note.ToLower());
                return new StatusChange
                {
                    From = matches.Item1,
                    To = matches.Item2,
                    EventDate = dateCreated
                };
            }


            
            return null;
        }

        public class Matcher
        {
            private Dictionary<string, string> slovenskaStanja = new Dictionary<string, string>()
            {
                {  " zadr.*ano", "on-hold" },
                { " v obdelavi","processing" },
                { " pla.*ilo v teku", "payment-underway"},
                { " zaklju.*eno", "completed"},
                { " preklicano", "cancelled" },
                { " neuspe.*no", "failed"}
            };

            public (string, string) FindMatches(string input)
            {
                if (!Regex.IsMatch(input, "stanje naro.*ila spremenjeno iz"))
                {
                    throw new Exception("No fitting pattern");
                }

                List<string> patternsFound = new List<string>();
                foreach (var pattern in slovenskaStanja.Keys)
                {
                    var match = Regex.Match(input, pattern);
                    if (match.Success)
                    {
                        patternsFound.Add(match.Value);
                        if (patternsFound.Count == 2) break; // If two patterns are found, break out
                    }
                }

                if (patternsFound.Count == 2)
                {
                    if (input.IndexOf(patternsFound[0]) < input.IndexOf(patternsFound[1]))
                    {
                        return (slovenskaStanja.FirstOrDefault(x => Regex.IsMatch(patternsFound[0], x.Key)).Value,
                                slovenskaStanja.FirstOrDefault(x => Regex.IsMatch(patternsFound[1], x.Key)).Value);
                    }
                    else
                    {
                        return (slovenskaStanja.FirstOrDefault(x => Regex.IsMatch(patternsFound[1], x.Key)).Value,
                                slovenskaStanja.FirstOrDefault(x => Regex.IsMatch(patternsFound[0], x.Key)).Value);
                    }
                }

                throw new Exception("No fitting pattern");
            }
        }
    



    private class DeserializedOrderNote
        {
            public string note { get; set; }
            public DateTime date_created_gmt { get; set; }
            public int id { get; set; }
        }
    }
}