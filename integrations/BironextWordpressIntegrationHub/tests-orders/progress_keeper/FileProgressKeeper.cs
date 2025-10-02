using BironextWordpressIntegrationHub.structs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using tests.tests.estrada;

namespace tests.tools
{

    public class AlreadyProcessedFilter
    {
        public List<string> MessageContains { get; set; }
    }
    public class FileProgressKeeper<T> : IProgressKeeper
    {
        string storagefilePath;
        ISigner<T> signer;
        AlreadyProcessedFilter filter;
        public FileProgressKeeper(string storagefilePath,
            ISigner<T> signer,
            AlreadyProcessedFilter filter) {
            this.storagefilePath = storagefilePath;
            this.signer = signer;
            this.filter = filter;

            string directoryPath = Path.GetDirectoryName(storagefilePath);
            Directory.CreateDirectory(directoryPath);
        }

        public List<ProgressState> GetFullState() {
            if (File.Exists(storagefilePath)) {
                string content = File.ReadAllText(storagefilePath);
                List<ProgressState> some = JsonConvert.DeserializeObject<List<ProgressState>>(content);
                if (some == null)
                    return new List<ProgressState>();
                return some;
            }
            return new List<ProgressState>();
        }

        public bool IsAlreadyProcessed(string jsonObj) {
            string signature = signer.GetSignature(JsonConvert.DeserializeObject<T>(jsonObj));
            var state = GetFullState().Where(x => x.signature == signature).ToList();
            if (state.Count == 1) {
                return filter.MessageContains.Any(x => state.First().message.Contains(x));
            } else if (state.Count > 1) {
                throw new Exception("Multiple states have been saved for the same order!");
            } else {
                return false;
            }
        }

        public void Restart() {
            SaveState(new List<ProgressState>());
        }

        public void SaveState(ProgressState state) {
            state.signature = signer.GetSignature(JsonConvert.DeserializeObject<T>(state.signature));
            List<ProgressState> states = GetFullState();
            int cnt = states.Where(x => x.signature == state.signature).Count();
            if (cnt == 1) {
                foreach (var curr in states) {
                    if (curr.signature == state.signature) {
                        curr.message = state.message;
                        curr.additionalinfo = state.additionalinfo;
                        curr.error = state.error;
                        curr.timeProcessed = DateTime.Now;
                        break;
                    }
                }
            } else if (cnt == 0) {
                states.Add(state);
            } else {
                throw new Exception("ProgressState should not have multiple records for the same signature!");
            }
            SaveState(states);
        }

        public void Setup() {

        }

        private void SaveState(List<ProgressState> state) {
            File.WriteAllText(storagefilePath, JsonConvert.SerializeObject(state));
        }

        public override string ToString() {
            return File.ReadAllText(storagefilePath);
        }
    }
}
