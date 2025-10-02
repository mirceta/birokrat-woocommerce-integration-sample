using biro_to_woo.logic.change_trackers.exhaustive;
using biro_to_woo.loop;
using BiroWooHub.logic.integration;
using Microsoft.Extensions.Logging;
using si.birokrat.next.common.logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace biro_to_woo_common.change_trackers.loop.async_per_integration {
    public interface IWorkloadObj<T> {

		string Signature { get; set; }
		Task Execute(CancellationToken token);
		Task<string> GetInfo();
		T GetResult();

		Task<Exception> GetError();

	}

	public class TestWorkloadObj : MyLoggable, IWorkloadObj<string> {

		string name;
		public TestWorkloadObj(string name) {
			this.name = name;
		}

        public string Signature { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public async Task Execute(CancellationToken token) {
			int cnt = 0;
			while (true) {
				cnt++;
				Console.WriteLine($"Hello from {name}" + cnt);
				await Task.Delay(new Random().Next(0, 5000));
			}
		}

        public Task<Exception> GetError()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetInfo() {
            throw new NotImplementedException();
        }

		public string GetResult() {
			throw new NotImplementedException();
		}
	}


}