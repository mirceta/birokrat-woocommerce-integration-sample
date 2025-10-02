using common_patterns.command;
using System;
using System.Collections.Generic;
using System.Linq;

namespace continuous_deployment
{
    public class DeploymentExecutor {

        public void ExecuteDeploymentPipeline(List<IStage> stages, string storefile)
        {
            PersistentStageTracker tracker = new PersistentStageTracker(stages, storefile);
            int current_stage = tracker.Get();

            stages = stages.Skip(current_stage).ToList();
            foreach (IStage stage in stages)
            {
                Console.WriteLine(string.Join("", Enumerable.Range(0, 50).Select(x => "//").ToArray()));
                Console.WriteLine($"Starting stage {tracker.Get()}: {stage.GetType().ToString()}");
                Console.WriteLine(string.Join("", Enumerable.Range(0, 50).Select(x => "//").ToArray()));
                stage.Execute();
                tracker.Increase();
            }
            tracker.Clear();
        }
    }
}
