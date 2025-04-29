using Entities;
using System.Collections.Generic;
using System.ComponentModel;

namespace Business
{
    public class Health : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Const.TerminalMode ModeField;
        private Const.LineMode LineField = Const.LineMode.OffLine;
        private Const.InUseMode InUseStateField = Const.InUseMode.NotInUse;
        private SensorsState SensorsStateField;
        private Enums.DeviceStatus BagFillLevelField;
        public bool ShipOutAvailable = true; //to prevent unexpected mechanic shipouts       
        public List<SDO_DeviceState> SDO_DevicesState; //Observable of devices states

        public Health() { }

        #region Properties
        public SensorsState SensorsState
        {
            get { return this.SensorsStateField; }
            set
            {
                if (this.SensorsStateField != value)
                {
                    this.SensorsStateField = value;
                    this.OnPropertyValueChanged("SensorsState");
                }
            }
        }

        public Const.LineMode Line
        {
            get { return this.LineField; }
            set
            {
                if (this.LineField != value)
                {
                    this.LineField = value;
                    this.OnPropertyValueChanged("Line");
                }
            }
        }

        public Const.TerminalMode Mode
        {
            get { return this.ModeField; }
            set
            {
                if (this.ModeField != value)
                {
                    this.ModeField = value;
                    this.OnPropertyValueChanged("Mode");
                }
            }
        }

        public Const.InUseMode InUseState
        {
            get { return this.InUseStateField; }
            set
            {
                if (this.InUseStateField != value)
                {
                    this.InUseStateField = value;
                    this.OnPropertyValueChanged("InUseState");
                }
            }
        }

        public Enums.DeviceStatus BagFillLevel
        {
            get { return this.BagFillLevelField; }
            set
            {
                if (this.BagFillLevelField != value)
                {
                    this.BagFillLevelField = value;
                    this.OnPropertyValueChanged("BagFillLevel");
                }
            }
        }

        protected void OnPropertyValueChanged(string propName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion Properties

    }
}