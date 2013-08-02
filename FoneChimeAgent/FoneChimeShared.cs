using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneChimeAgent
{
    public class FoneChime
    {
        private const string IntervalSettingName = "Interval";
        private const string LastChimeSettingName = "LastChime";
        private readonly static TimeSpan DefaultInterval = TimeSpan.FromMinutes(5);
        private readonly static DateTimeOffset DefaultLastChime = DateTimeOffset.Now;
        private readonly IsolatedStorageSettings _settings = IsolatedStorageSettings.ApplicationSettings;
        private TimeSpan _interval;
        private DateTimeOffset _lastChime;

        public FoneChime()
        {
            _interval = _settings.Contains(IntervalSettingName) ? (TimeSpan)_settings[IntervalSettingName] : DefaultInterval;
            _lastChime = _settings.Contains(LastChimeSettingName) ?  (DateTimeOffset)_settings[LastChimeSettingName] : DefaultLastChime;
        }

        public TimeSpan Interval
        {
            get { return _interval; }
            set { _interval = value; Save(); }
        }

        public DateTimeOffset LastChime
        {
            get { return _lastChime; }
            set { _lastChime = value; Save(); }
        }

        public bool Advance()
        {
            if (LastChime + Interval <= DateTimeOffset.Now)
            {
                LastChime = DateTimeOffset.Now;
                return true;
            }

            return false;
        }

        private void Save()
        {
            _settings[IntervalSettingName] = Interval;
            _settings[LastChimeSettingName] = LastChime;
        }
    }
}
