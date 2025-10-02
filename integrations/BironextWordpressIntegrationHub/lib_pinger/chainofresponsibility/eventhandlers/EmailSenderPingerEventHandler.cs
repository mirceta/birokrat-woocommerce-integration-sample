using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace infrastructure_pinger.chainofresponsibility.eventhandlers
{
    public class EmailSenderPingerEventHandler : IPingerEventHandler
    {
        SmtpEmailSender emailSender;
        public EmailSenderPingerEventHandler(SmtpEmailSender emailSender)
        {
            this.emailSender = emailSender;
        }
        public async Task onLongHeartbeat(List<Deployment> deployments)
        {
            if (deployments.All(x => IsPingSuccessful(x)))
            {
                Notification(deployments, "[Bironext pinger] Everything works", "");
            }
            else
            {
                string failed = string.Join(", ", deployments.Where(x => !IsPingSuccessful(x)));
                Notification(deployments, "[Bironext pinger] Services are down", failed);
            }
        }

        public async Task onServiceFailure(List<Deployment> fails)
        {
            Notification(fails, "[Bironext pinger] New failures", string.Join(", ", fails.Select(x => x.Name)));
        }

        public async Task OnServiceRestore(List<Deployment> successes)
        {
            Notification(successes, "[Bironext pinger] Services have been restored", string.Join(", ", successes.Select(x => x.Name)));
        }

        public async Task onWarning(List<Deployment> potentiallyFailed)
        {
            
        }

        private void Notification(List<Deployment> deployments, string subject, string body)
        {
            string currentstate = string.Join("\n", deployments.Select(x => x.Name + ": " + (IsPingSuccessful(x) ? "online" : "offline: " + x.PingResult)).ToList());
            emailSender.SendEmail(subject, body + "\n\n" + currentstate);
        }

        private bool IsPingSuccessful(Deployment dep)
        {
            return dep.UnsuccessfulPingsInARow < 3;
        }

        
    }
}
