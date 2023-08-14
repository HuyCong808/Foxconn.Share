using Foxconn.Editor.Enums;
using System;

namespace Foxconn.Editor
{
    public class Statistics : NotifyProperty
    {
        private Properties.Settings _settings = Properties.Settings.Default;
        private MachineParams _param = MachineParams.Current;
        public DateTime TimeUpdateRate
        {
            get => _settings.TimeUpdateRate;
            set => _settings.TimeUpdateRate = value;
        }

        private static Statistics _current;
        public static Statistics Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new Statistics();
                }
                return _current;
            }
        }
        public Statistics() { }
        static Statistics() { }

        public bool IsNewDay()
        {
            var now = DateTime.Now;
            var dateCreated = Properties.Settings.Default.DateCreated;
            if ((now - dateCreated).Days > 0)
            {
                dateCreated = DateTime.Now.Date;
                Properties.Settings.Default.DateCreated = dateCreated;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateNewStatistic()
        {
            if (IsNewDay())
            {
                _settings.TotalPass = 0;
                _settings.TotalFail = 0;
                _settings.TotalChecked = 0;
                _settings.YeildRate = 0;
                _settings.Save();
                NotifyPropertyChanged();
            }
        }

        public void ResetStatistic()
        {
            _settings.TotalPass = 0;
            _settings.TotalFail = 0;
            _settings.TotalChecked = 0;
            _settings.YeildRate = 0;
            _settings.Save();
        }

        public void ResetStatisticButton()
        {
            _settings.TotalPass = 0;
            _settings.TotalFail = 0;
            _settings.TotalChecked = 0;
            _settings.YeildRate = 0;
            _settings.Save();

            MessageShow.Info("Reseted Statistic", "Reset");
        }

        public void ResetFailRate()
        {
            _settings.TotalFail = 0;
            _settings.TotalChecked = _settings.TotalPass + _settings.TotalFail;
            if (_settings.TotalPass == 0)
            {
                _settings.YeildRate = 0;
            }
            else
            {
                _settings.YeildRate = (float)Math.Round((float)_settings.TotalPass / _settings.TotalChecked * 100, 2);
            }

            _settings.Save();
            MessageShow.Info("Reseted FailRate", "Reset");
        }

        public void UpdateRate(int numPass, int numFail)
        {
            DateTime now = DateTime.Now;
            if (_param.WorkType == WorkType.ByDay)
            {
                if (IsNewDay())
                {
                    ResetStatistic();
                    _settings.TotalPass += numPass;
                    _settings.TotalFail += numFail;
                }
                else
                {
                    _settings.TotalPass += numPass;
                    _settings.TotalFail += numFail;
                }
            }

            else if (_param.WorkType == WorkType.ByShift)
            {
                if (IsSameShift(now, TimeUpdateRate))
                {
                    _settings.TotalPass += numPass;
                    _settings.TotalFail += numFail;
                }
                else
                {
                    ResetStatistic();
                    _settings.TotalPass += numPass;
                    _settings.TotalFail += numFail;
                }

            }
            _settings.TotalChecked = _settings.TotalPass + _settings.TotalFail;
            _settings.YeildRate = (float)Math.Round((float)_settings.TotalPass / _settings.TotalChecked * 100, 2);
            _settings.TimeUpdateRate = DateTime.Now;
            _settings.Save();
            NotifyPropertyChanged();
        }

        public bool InDayShift(DateTime now)
        {
            return now.Hour > 8 && now.Hour < 19 || now.Hour >= 7 && now.Minute >= 30 && now.Hour < 19 || now.Hour > 7 && now.Hour <= 19 && now.Minute < 30;
        }

        public bool InNightShift(DateTime now)
        {
            return now.Hour > 19 || now.Hour >= 19 && now.Minute >= 30 || now.Hour < 7 || now.Hour <= 7 && now.Minute < 30;
        }

        public bool IsSameShift(DateTime now, DateTime timeUpdate)
        {
            bool fRet = false;
            if (InDayShift(now) && InDayShift(timeUpdate))
            {
                if (now.Day == timeUpdate.Day)
                {
                    fRet = true;
                }
            }
            else if (InNightShift(now) && InNightShift(timeUpdate))
            {
                if (now.Day == timeUpdate.Day)
                {
                    if (now.Hour < 7 && timeUpdate.Hour < 7 || now.Hour <= 7 && now.Minute < 30 && timeUpdate.Hour < 8)
                    {
                        fRet = true;
                    }
                    else if (now.Hour == 19 && now.Minute >= 30 && timeUpdate.Hour == 19 && timeUpdate.Minute >= 30 || now.Hour > 19 && timeUpdate.Hour > 19)
                    {
                        fRet = true;
                    }
                }
                else if (now.Day - timeUpdate.Day == 1)
                {
                    if (now.Hour < 7 && timeUpdate.Hour > 19 || now.Hour < 7 && timeUpdate.Hour == 19 && timeUpdate.Minute >= 30 || now.Hour == 7 && now.Minute < 30 && timeUpdate.Hour > 19 || now.Hour == 7 && now.Minute < 30 && timeUpdate.Hour == 19 && timeUpdate.Minute >= 30)
                    {
                        fRet = true;
                    }
                }
            }
            return fRet;
        }
    }
}
