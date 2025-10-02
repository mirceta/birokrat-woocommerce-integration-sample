using common_patterns.command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace continuous_deployment
{

    /*
    - Will keep track to which stage of the pipeline you got the last time you ran the program.
    - Will reset itself if you change the sequence of stages
    - Will reset after you have finished the last stage of the pipeline
    - Can be tampered with by modifying temp-stage-supervisor.txt
    */
    public class PersistentStageTracker
    {
        int current_stage = 0;
        List<IStage> stages;
        string storefile;

        public PersistentStageTracker(List<IStage> stages, string storefile)
        {
            this.storefile = storefile;
            this.stages = stages;
            
            if (File.Exists(this.storefile))
            {
                var anon = new { current_stage = 0, stage_names = new string[] { } };
                var content = JsonConvert.DeserializeAnonymousType(File.ReadAllText(this.storefile), anon);
                if (StageSequenceChanged(stages, content.stage_names))
                {
                    current_stage = 0;
                }
                else
                {
                    current_stage = content.current_stage;
                }
            }
            else
            {
                current_stage = 0;
            }
        }

        public int Get() {
            return current_stage;
        }

        public void Increase() {
            current_stage++;
            Save();
        }

        public void Clear()
        {
            File.Delete(storefile);
        }

        private void Save()
        {
            string[] signatures = stages.Select(x => GetStageSignature(x)).ToArray();
            var anon = new { current_stage = current_stage.ToString(), stage_names = signatures };
            File.WriteAllText(storefile, JsonConvert.SerializeObject(anon));
        }

        private bool StageSequenceChanged(List<IStage> stages, string[] old_stage_names) {
            try
            {
                return !stages.Select(x => GetStageSignature(x))
                        .Zip(old_stage_names, (x, y) => x == y)
                        .Aggregate((x, y) => x && y);
            }
            catch (Exception ex) { }
            return true;
        }

        private string GetStageSignature(IStage stage) {
            return stage.Name();
        }
    }
}
