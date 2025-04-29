using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Business.States
{
    public class PropertiesState
    {
        private bool EnableStateField = true;

        [XmlElement()]
        public bool EnableState
        {
            get
            {
                return this.EnableStateField;
            }
            set
            {
                this.EnableStateField = value;
            }
        }
    }
}
