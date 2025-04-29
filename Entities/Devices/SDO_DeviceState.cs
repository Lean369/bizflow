using System.ComponentModel;
using System.Collections.Generic;

namespace Entities
{
    public  class SDO_DeviceState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Enums.Devices DeviceField;
        private Const.Supplies SuppliesField;
        private Const.Fitness FitnessField;
        private Enums.DeviceStatus InternalCodeField; //Código interno generado por la app
        private List<string> DetailsField; //Detalle del estado del dispositivo

        public SDO_DeviceState(Enums.Devices device, Const.Supplies supplies, Const.Fitness fitness, Enums.DeviceStatus internalCode)
        {
            this.DeviceField = device;
            this.SuppliesField = supplies;
            this.FitnessField = fitness;
            this.InternalCode = internalCode;
        }
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public SDO_DeviceState() { }

        #region Properties
        public Enums.Devices Device
        {
            get { return this.DeviceField; }
            set { this.DeviceField = value; }
        }

        public Const.Supplies Supplies
        {
            get { return this.SuppliesField; }
            set { SetField(ref this.SuppliesField, value, $"{this.DeviceField}.Supplies"); }
        }

        public Const.Fitness Fitness
        {
            get { return this.FitnessField; }
            set { SetField(ref this.FitnessField, value, $"{this.DeviceField}.Fitness"); }                
        }

        public Enums.DeviceStatus InternalCode
        {
            get { return this.InternalCodeField; }
            set { this.InternalCodeField = value; }
        }

        public List<string> Details
        {
            get { return this.DetailsField; }
            set { this.DetailsField = value; }
        }
        #endregion Properties
    }
}
