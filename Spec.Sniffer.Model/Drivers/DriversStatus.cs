﻿using Spec.Sniffer.Model.Drivers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Spec.Sniffer.Model
{
    public class DriversStatus: INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer;
        private List<Driver> faultyDrivers;

        public List<Driver> FaultyDrivers
        {
            get => faultyDrivers;
            set
            {
                faultyDrivers = value;
                RaisePropertyChanged("FaultyDrivers");
            }
        }

        public DriversStatus(int timersSpan)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(timersSpan);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private List<Driver> GetDriversList()
        {
            List<Driver> DeviceList = new List<Driver>();
            try
            {
                foreach (var queryObj in new ManagementObjectSearcher("root\\CIMV2",
                    $"SELECT Caption,ConfigManagerErrorCode " +
                    $"FROM Win32_PnPEntity").Get())
                {
                    DeviceList.Add(
                        new Driver
                        {
                            Caption = (string)queryObj["Caption"],
                            ErrorCode = (uint)queryObj["ConfigManagerErrorCode"]
                        });
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"GetDriversList Error:\n{ex.Message}");
            }
            return DeviceList;
        }

        private void RefreshDrivers()
        {
            FaultyDrivers = GetDriversList().Where(d => d.ErrorCode > 0).ToList();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            RefreshDrivers();
        }

        #region INotify Property handler

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
