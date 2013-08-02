using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Notification;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using FoneChime.Resources;

namespace FoneChime
{
    public partial class MainPage : PhoneApplicationPage
    {
        private static readonly int[] PickableValues = new[] { 1, 2, 5, 10, 15, 30, 45, 60 };
        private const string HttpNotificationChannelName = "FoneChime";

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            HttpNotificationChannel pushChannel = HttpNotificationChannel.Find(HttpNotificationChannelName);

            if (null != pushChannel)
            {
                EnabledCheckBox.IsChecked = true;
            }

            IntervalPicker.ItemsSource = PickableValues.Select(GetMinuteString).ToList();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            bool enabled = box.IsChecked ?? false;

            HttpNotificationChannel pushChannel = HttpNotificationChannel.Find(HttpNotificationChannelName);

            if (enabled)
            {
                if (null == pushChannel)
                {
                    pushChannel = new HttpNotificationChannel(HttpNotificationChannelName);
                    pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                    pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);
                    pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);
                    pushChannel.Open();
                    pushChannel.BindToShellToast();
                }
                else
                {
                    pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(PushChannel_ChannelUriUpdated);
                    pushChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(PushChannel_ErrorOccurred);
                    pushChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);
                }
            }
            else if (!enabled && pushChannel != null)
            {
                try
                {
                    pushChannel.UnbindToShellToast();
                    pushChannel.Close();
                }
                catch
                {
                }
            }
        }

        public string GetMinuteString(int minutes)
        {
            if (minutes == 1) return "1 minute";
            if (minutes == 60) return "1 hour";
            if (minutes > 0 && minutes % 60 == 0) return (minutes / 60) + " hours";
            return minutes + " minutes";
        }

        void PushChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            StringBuilder post = new StringBuilder();
            post.Append("resp=" + HttpUtility.UrlEncode(e.ChannelUri.ToString()) + "&");
            post.Append("minutes=1");
            byte[] postBytes = Encoding.UTF8.GetBytes(post.ToString());

            HttpWebRequest request = WebRequest.CreateHttp("http://www.thedandriscoll.org/periodicpush.php");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;

            IAsyncResult result = request.BeginGetRequestStream(r => CompleteSendRequest(r, postBytes), request);
        }

        void PushChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            // Error handling logic for your particular application would be here.
            Dispatcher.BeginInvoke(() =>
                MessageBox.Show(String.Format("A push notification {0} error occurred.  {1} ({2}) {3}",
                    e.ErrorType, e.Message, e.ErrorCode, e.ErrorAdditionalData))
                    );
        }

        void PushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            StringBuilder message = new StringBuilder();
            string relativeUri = string.Empty;

            message.AppendFormat("Received Toast {0}:\n", DateTime.Now.ToShortTimeString());

            // Parse out the information that was part of the message.
            foreach (string key in e.Collection.Keys)
            {
                message.AppendFormat("{0}: {1}\n", key, e.Collection[key]);

                if (string.Compare(
                    key,
                    "wp:Param",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.CompareOptions.IgnoreCase) == 0)
                {
                    relativeUri = e.Collection[key];
                }
            }

            // Display a dialog of all the fields in the toast.
            Dispatcher.BeginInvoke(() => MessageBox.Show(message.ToString()));
        }

        private void CompleteSendRequest(IAsyncResult ar, byte[] data)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            Stream stream = request.EndGetRequestStream(ar);
            stream.Write(data, 0, data.Length);
            stream.Close();
            request.BeginGetResponse(_ => {}, null);
        }
    }
}