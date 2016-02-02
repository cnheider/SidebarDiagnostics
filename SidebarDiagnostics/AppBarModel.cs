﻿using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Threading;
using SidebarDiagnostics.Monitor;

namespace SidebarDiagnostics.Models
{
    public class AppBarModel : INotifyPropertyChanged, IDisposable
    {
        public AppBarModel()
        {
            InitClock();
            InitMonitors();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeClock();
                    DisposeMonitors();
                }

                _disposed = true;
            }
        }

        ~AppBarModel()
        {
            Dispose(false);
        }

        public void Restart()
        {
            DisposeMonitors();
            InitMonitors();
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler _handler = PropertyChanged;

            if (_handler != null)
            {
                _handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InitClock()
        {
            if (!Properties.Settings.Default.ShowClock)
            {
                return;
            }

            ShowDate = !Properties.Settings.Default.DateSetting.Equals(Properties.DateSetting.Disabled);

            UpdateClock();

            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += new EventHandler(ClockTimer_Tick);
            _clockTimer.Start();
        }

        private void InitMonitors()
        {
            MonitorManager = new MonitorManager();

            foreach (MonitorConfig _config in Properties.Settings.Default.MonitorConfig.Where(c => c.Enabled).OrderBy(c => c.Order))
            {
                MonitorManager.AddPanel(_config);
            }

            MonitorManager.Initialize();
            MonitorManager.Update();

            _monitorTimer = new DispatcherTimer();
            _monitorTimer.Interval = TimeSpan.FromMilliseconds(Properties.Settings.Default.PollingInterval);
            _monitorTimer.Tick += new EventHandler(MonitorTimer_Tick);
            _monitorTimer.Start();
        }

        private void UpdateClock()
        {
            DateTime _now = DateTime.Now;

            Time = _now.ToString(Properties.Settings.Default.Clock24HR ? "H:mm:ss" : "h:mm:ss tt");

            if (ShowDate)
            {
                Date = _now.ToString(Properties.Settings.Default.DateSetting.Format);
            }
        }

        private void UpdateMonitors()
        {
            MonitorManager.Update();
        }

        private void DisposeClock()
        {
            if (_clockTimer != null)
            {
                _clockTimer.Stop();
                _clockTimer = null;
            }
        }

        private void DisposeMonitors()
        {
            if (_monitorTimer != null)
            {
                _monitorTimer.Stop();
                _monitorTimer = null;
            }
            
            if (MonitorManager != null)
            {
                MonitorManager.Dispose();
                _monitorManager = null;
            }
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            UpdateClock();
        }

        private void MonitorTimer_Tick(object sender, EventArgs e)
        {
            UpdateMonitors();
        }

        private string _time { get; set; }

        public string Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;

                NotifyPropertyChanged("Time");
            }
        }

        private bool _showDate { get; set; }

        public bool ShowDate
        {
            get
            {
                return _showDate;
            }
            set
            {
                _showDate = value;

                NotifyPropertyChanged("ShowDate");
            }
        }

        private string _date { get; set; }

        public string Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;

                NotifyPropertyChanged("Date");
            }
        }

        private MonitorManager _monitorManager { get; set; }

        public MonitorManager MonitorManager
        {
            get
            {
                return _monitorManager;
            }
            set
            {
                _monitorManager = value;

                NotifyPropertyChanged("MonitorManager");
            }
        }

        private DispatcherTimer _clockTimer { get; set; }

        private DispatcherTimer _monitorTimer { get; set; }

        private bool _disposed { get; set; } = false;
    }
}
