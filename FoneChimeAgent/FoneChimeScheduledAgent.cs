using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;

namespace FoneChimeAgent
{
    public class FoneChimeScheduledAgent : ScheduledTaskAgent
    {
        public const string ScheduledAgentName = "FoneChimeAgent";
        public const string ScheduledAgentDescription = "FoneChime background task";

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static FoneChimeScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            FoneChime chime = new FoneChime();

            if (chime.Advance())
            {
                ShellToast toast = new ShellToast();
                toast.Title = "FoneChime";
                toast.Content = "Chime!";
                toast.Show();
            }

            NotifyComplete();
        }
    }
}