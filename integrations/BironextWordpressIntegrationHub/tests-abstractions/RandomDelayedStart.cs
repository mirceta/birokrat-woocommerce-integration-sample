using si.birokrat.next.common.logging;
using System;
using System.Text;
using System.Threading.Tasks;
using BirokratNext;
using System.Security.Cryptography;

namespace tests.composition.common
{
    public class RandomDelayedStart
    {


        IApiClientV2 api;
        IMyLogger logger;
        Random rnd;

        static int sleeptimerandom = 30000;

        public RandomDelayedStart(string rndseed, IApiClientV2 client, IMyLogger logger = null)
        {
            api = client;
            this.logger = logger;

            MD5 md5Hasher = MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(rndseed));
            var ivalue = BitConverter.ToInt32(hashed, 0);
            rnd = new Random(ivalue);

        }

        public async Task Test(Func<Task> work)
        {
            /*
             * In test version we start the program immediately because we assume that there are few parallel workloads running at the same time
             */

            try
            {
                await api.Logout();
                logger.LogInformation("Logged out successfully");
                await work();
            }
            catch (Exception ex)
            {
                logger.LogWarning("Exception caught in root. Proceed with next iteration of loop in root. Exception: " + ex.Message + ex.StackTrace.ToString());
            }
            finally
            {
                var sleeptime = rnd.Next(10, sleeptimerandom);
                logger.LogInformation($"Now sleeping for {sleeptime / 1000} seconds");
                await Task.Delay(sleeptime);
            }
        }

        public async Task Prod(Func<Task> work)
        {
            /*
             * In Prod version we first sleep randomly because we assume there will be many concurrent workloads running, and by randomizing the execution time,
             * we ensure that not all of them will be bombarding the proc / server at the same time.
             */
            var sleeptime = rnd.Next(10, sleeptimerandom);
            logger.LogInformation($"Now sleeping for {sleeptime / 1000} seconds");
            await Task.Delay(sleeptime);
            try
            {
                await api.Logout();
                logger.LogInformation("Logged out successfully");
                await work();
            }
            catch (Exception ex)
            {
                logger.LogWarning("Exception caught in root. Proceed with next iteration of loop in root. Exception: " + ex.Message + ex.StackTrace.ToString());
            }
        }
    }

}
