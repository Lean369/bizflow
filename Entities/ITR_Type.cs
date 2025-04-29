using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace Entities
{
    public class ITR_Type
    {
        private int timeout = 45;

        private bool enableFdkA;

        private bool enableFdkB;

        private bool enableFdkC;

        private bool enableFdkD;

        private bool enableFdkF;

        private bool enableFdkG;

        private bool enableFdkH;

        private bool enableFdkI;

        private bool enableCancel;

        private bool enableNumeric;

        private string displayFlagMasc = "Z";

        public int Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                this.timeout = value;
            }
        }

        public bool EnableFdkA
        {
            get
            {
                return this.enableFdkA;
            }
            set
            {
                this.enableFdkA = value;
            }
        }

        public bool EnableFdkB
        {
            get
            {
                return this.enableFdkB;
            }
            set
            {
                this.enableFdkB = value;
            }
        }

        public bool EnableFdkC
        {
            get
            {
                return this.enableFdkC;
            }
            set
            {
                this.enableFdkC = value;
            }
        }

        public bool EnableFdkD
        {
            get
            {
                return this.enableFdkD;
            }
            set
            {
                this.enableFdkD = value;
            }
        }

        public bool EnableFdkF
        {
            get
            {
                return this.enableFdkF;
            }
            set
            {
                this.enableFdkF = value;
            }
        }

        public bool EnableFdkG
        {
            get
            {
                return this.enableFdkG;
            }
            set
            {
                this.enableFdkG = value;
            }
        }

        public bool EnableFdkH
        {
            get
            {
                return this.enableFdkH;
            }
            set
            {
                this.enableFdkH = value;
            }
        }

        public bool EnableFdkI
        {
            get
            {
                return this.enableFdkI;
            }
            set
            {
                this.enableFdkI = value;
            }
        }

        public bool EnableCancel
        {
            get
            {
                return this.enableCancel;
            }
            set
            {
                this.enableCancel = value;
            }
        }

        public bool EnableNumeric
        {
            get
            {
                return this.enableNumeric;
            }
            set
            {
                this.enableNumeric = value;
            }
        }

        public string DisplayFlagMasc
        {
            get
            {
                return this.displayFlagMasc;
            }
            set
            {
                this.displayFlagMasc = value;
            }
        }
    }
}
