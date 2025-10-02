using biro_to_woo_common.executor.detection_actions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Threading;

public class ConsoleDetectionAction : IDetectionAction
{
    public Task NotifyChanges(List<string> successfulItemSifras, CancellationToken token)
    {
        foreach (var sifra in successfulItemSifras)
        {
            Console.WriteLine($"Article with Sifra: {sifra} has been successfully processed.");
        }

        return Task.CompletedTask;
    }
}