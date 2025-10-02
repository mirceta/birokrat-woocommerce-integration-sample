using System;
using System.Threading.Tasks;
using tests;
using System.Collections.Generic;
using System.Threading;



/*
 Story:
Problem importance reasoning:
Get bironext server under control. Bironext server and birokrat integrations are actually strongly related.
BIROWOO TESTS are tests for bironext server as well as birowoo project.
Birowoo tests should be faking metalno dobro odradjeno, because it is the most important project.
So BIROWOO TESTS ARE THE MOST IMPORTANT GOAL.
Problem specification:
For parameter list List<integration name> find all integrations they have.
Concurrently execute all of the tests for these firms.
Generate comprehensive reports on the tests so that you know exactly what works and what doesn’t.
Generate reports on how many times bironext was restarted and what errors occurred during the time bironext was restarted.
Problem dissection:
Task 1: If failed order/product retry {PARAMETER} times, if still fail, then restart bironext! - this should be encapsulated into a failure handling class which is injected into relevant mechanisms.
If tests are running async for multiple firms - this needs to be synchronized.
Task 2:
The tests should run asynchronously.
This is rather simple, as in this case the tests are not looping but rather only executed one time. Thus you can just start a new thread and run it - the only shared resources are BironextResetter and DatabaseResetter.
Task 3:
Need another top level abstraction - MultipleTestDriver, SingleTestDriver.
MultipleTestDriver(List<IIntegration> integrations) { // can be both!
SingleTestDriver(IIntegration integration) // if BIROTOWOO … product tests else ordertests
Task 3:
For products: webshop must be either mocked, or we have multiple woocommerces.
Task 4: 
Test report generation: how do we know that we pass?

 */

namespace tests.async
{
    public class BirowooTestWorkloadObj : IWorkloadObj<string>
    {

        ITests<string> tests;
        string signature;
        public BirowooTestWorkloadObj(ITests<string> tests, string signature)
        {
            this.tests = tests;

            this.signature = signature;
        }

        bool isFinished = false;
        bool successful = false;
        DateTime finishedTime = DateTime.MaxValue;

        public async Task Execute(CancellationToken token)
        {
            
            try
            {
                await tests.Work(token);
                successful = true;
                NotifyOfOutcomeEvent("success", "");
            }
            catch (Exception ex) {
                this.outcome = ex;
                NotifyOfOutcomeEvent("error", ex.Message + ex.StackTrace.ToString());
            }
            finally
            {
                isFinished = true;
                finishedTime = DateTime.Now;
            }
        }
        public string Signature { get => signature; set => throw new NotImplementedException(); }

        public async Task<Exception> GetError()
        {
            return this.outcome;
        }

        public async Task<string> GetInfo()
        {
            return finishedTime.ToString("yyyy-MM-ddHH:mm:ss");
        }

        Exception outcome = null;
        public Exception getExecutionException() {
            return outcome;
        }

        public string GetResult()
        {

            if (isFinished)
            {
                return tests.GetResult();
            }
            return "There are no results because the task has not finished yet!";
        }

        Dictionary<string, Func<string, string, string, int>> outcomeSubscribers = new Dictionary<string, Func<string, string, string, int>>();

        public void SubscribeToOutcomeEvent(string some, Func<string, string, string, int> callback) {
            if (outcomeSubscribers.ContainsKey(some))
                throw new Exception("Subscriber already exists");
            outcomeSubscribers[some] = callback;
        }


        // Should never be made public!!! This channel is not meant for anything other than
        // outcome events! -> This is meant to signify that a task has been finished, so maybe
        // in some use cases it will be replaced with new tasks, or archived or something.
        private void NotifyOfOutcomeEvent(string evt, string message) {
            foreach (var sub in outcomeSubscribers) {
                sub.Value(sub.Key, evt, message);
            }
        }

        Dictionary<string, Func<string, string, string, int>> externalEventSubscribers = new Dictionary<string, Func<string, string, string, int>>();
        public void SubscribeToExternalEvents(string some, Func<string, string, string, int> callback)
        {
            if (externalEventSubscribers.ContainsKey(some))
                throw new Exception("Subscriber already exists");
            externalEventSubscribers[some] = callback;
        }

        
        public void NotifyExternalEvent(string evt, string message)
        {
            /*
             EXTERNAL EVENTS IN THIS CONTEXTS MEAN EVENTS THAT ARE OUTSIDE THE CONTEXT OF THE INTEGRATION OBJECT. FOR EXAMPLE AN INTERNAL EVENT
             IS AN INVOICE BEING CREATED IN BIROKRAT (THIS HAPPENS WITHIN THE INTEGRATION OBJECT), WHILE AN EXTERNAL EVENT IS FOR EXAMPLE
             THE TASK ENTERING A SEMAPHORE GUARDED CRITICAL SECTION WITHIN THE MultithreadedTaskExecutionStrategy object - IT DOES SOMETHING WITH 
             THE INTEGRATION OBJECT, BUT WHAT HAPPENS IS OUTSIDE THE BOUNDS OF THE INTEGRATION OBJECT ITSELF.
            */
            foreach (var sub in externalEventSubscribers)
            {
                sub.Value(sub.Key, evt, message);
            }
        }
    }
}
