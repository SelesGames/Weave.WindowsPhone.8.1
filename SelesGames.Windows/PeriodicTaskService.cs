using Microsoft.Phone.Scheduler;
using System;
using System.Linq;

namespace SelesGames.Phone
{
    public class PeriodicTaskService
    {
        string agentName;

        public PeriodicTaskService(string agentName)
        {
            this.agentName = agentName;
        }

        public string Description { get; set; }
        public Exception RegistrationException { get; private set; }

        public bool TryRegister()
        {
            // Obtain a reference to the period task, if one exists
            var periodicTask = ScheduledActionService.Find(agentName) as PeriodicTask;

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            try
            {
                if (periodicTask != null)
                    ScheduledActionService.Remove(agentName);

                var actions = ScheduledActionService.GetActions<PeriodicTask>().ToList();
                foreach (var action in actions)
                    ScheduledActionService.Remove(action.Name);
            }
            catch (Exception ex)
            {
                RegistrationException = ex;
                return false;
            }


            periodicTask = new PeriodicTask(agentName) { Description = Description };

            // Place the call to Add in a try block in case the user has disabled agents
            try
            {
                ScheduledActionService.Add(periodicTask);
#if BGTASKTEST 
                ScheduledActionService.LaunchForTest(agentName, TimeSpan.FromSeconds(10));
#endif
            }
            catch (InvalidOperationException exception)
            {                
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    //MessageBox.Show("Background agents for this application have been disabled by the user.");
                }
                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.
                }
                RegistrationException = exception;
                return false;
            }
            catch (SchedulerServiceException exception)
            {
                RegistrationException = exception;
                return false;
            }

            return true;
        }
    }
}
