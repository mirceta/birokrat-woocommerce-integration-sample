using biro_to_woo.logic.change_trackers.exhaustive;
using biro_to_woo.loop;
using BiroWooHub.logic.integration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.change_trackers.loop.async_per_integration {
    
	/*
	public class IndependentConcurrentTaskRunner<T>
    {

		List<IWorkloadObj<T>> integrations;
		string program_data_path;
		ILogger logger;

		public IndependentConcurrentTaskRunner(List<IWorkloadObj<T>> integrations) {
			this.integrations = integrations;
		}

		public async Task Start() {
			var syncobjs = integrations.Select(x => new SyncObj<T>(x)).ToList();
			syncobjs.ForEach(async x => await x.Start());
		}

    }
	public class SyncObj<T> {

		CancellationTokenSource cancellation;
		Task runner;
		IWorkloadObj<T> workloadObj;

		public SyncObj(IWorkloadObj<T> workloadObj) {
			this.workloadObj = workloadObj;
		}

		public bool isRunning { get; set; } // and other attributes

		public Task Start() {
			if (runner != null && (runner.IsCompleted || runner.IsFaulted || runner.IsCanceled)) {
				Stop();
			}
			if (runner == null) {
				cancellation = new CancellationTokenSource();
				var token = cancellation.Token;
				runner = Task.Run(async () => await loop(), token);
			}
			return runner;
		}

		private async Task loop() {
			await workloadObj.Execute();
		}

		private void Stop() {
			if (cancellation != null)
				cancellation.Cancel();
			if (runner != null) {
				runner.Dispose();
				runner = null;
			}
		}
	}
	*/
}