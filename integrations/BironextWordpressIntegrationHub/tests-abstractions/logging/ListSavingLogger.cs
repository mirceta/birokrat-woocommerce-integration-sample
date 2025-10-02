using System;
using System.Collections.Generic;
using System.Linq;
using si.birokrat.next.common.logging;



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

using System.Collections.Concurrent;
using BiroWoocommerceHubTests;

public class ListSavingLogger : IMyLogger
{

    const int MaxLogSize = 2000;
    const int BulkRemoveSize = 1000;

    ConcurrentQueue<LogEntry> entries;

    public ListSavingLogger()
    {
        entries = new ConcurrentQueue<LogEntry>();
    }

    public void LogError(string message, Exception ex = null)
    {
        Enqueue(new LogEntry(message, DateTime.Now, ex));
        Notify();
    }

    public void LogInformation(string message)
    {
        Enqueue(new LogEntry(message, DateTime.Now));
        Notify();
    }

    public void LogWarning(string message, Exception ex = null)
    {
        Enqueue(new LogEntry(message, DateTime.Now, ex));
        Notify();
    }

    public string GetLogEntries()
    {
        if (entries == null || !entries.Any())
            return "";
        return string.Join(Environment.NewLine, entries.Select(x => $"{DateTime.Now.Subtract(x.Timestamp).TotalSeconds} ago: {x.Message} {(x.Ex == null ? "" : x.Ex.ToString())}").Reverse());
    }

    ConcurrentDictionary<string, Func<string, ListSavingLogger, int>> subscribers = new ConcurrentDictionary<string, Func<string, ListSavingLogger, int>>();
    public void Subscribe(Tuple<string, Func<string, ListSavingLogger, int>> subscriber)
    {
        if (subscribers.ContainsKey(subscriber.Item1))
            throw new Exception("Existing observable subscriber already has this id");
        subscribers[subscriber.Item1] = subscriber.Item2;
    }

    public void Unsubscribe(Tuple<string, Func<string, ListSavingLogger, int>> subscriber)
    {
        if (subscribers.ContainsKey(subscriber.Item1))
            subscribers.TryRemove(subscriber.Item1, out _);
    }

    void Notify()
    {
        foreach (var x in subscribers)
        {
            var t = subscribers[x.Key];
            x.Value(x.Key, this);
        }
    }

    void Enqueue(LogEntry entry)
    {
        entries.Enqueue(entry);
        while (entries.Count > MaxLogSize)
        {
            for (int i = 0; i < BulkRemoveSize; i++)
            {
                entries.TryDequeue(out _);
            }
        }
    }
}
