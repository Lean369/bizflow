using System;
using System.Xml;
using System.Xml.Serialization;
using Business.States;
using System.Collections.Generic;
using Entities;

namespace Business.FingerPrintCaptureState
{
    [Serializable]
    public partial class PropertiesFingerPrintCaptureState
    {
        private string placeFingerScreenNumberField;
        private string timeOutNextStateNumberField;
        private string cancelNextStateNumberField;
        private string fDKPressededNextStateNumberField;
        private string fDKActiveMaskField;
        private Extension1 extension1Field;
        private Extension2 extension2Field;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;

        public PropertiesFingerPrintCaptureState()
        {
            this.placeFingerScreenNumberField = "";
            this.timeOutNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.fDKPressededNextStateNumberField = "";
            this.fDKActiveMaskField = "";
            this.extension1Field = new Extension1();
            this.extension2Field = new Extension2();
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
        }

        #region Properties
        [XmlElement()]
        public string PlaceFingerScreenNumber
        {
            get
            {
                return this.placeFingerScreenNumberField;
            }
            set
            {
                this.placeFingerScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string TimeOutNextStateNumber
        {
            get
            {
                return this.timeOutNextStateNumberField;
            }
            set
            {
                this.timeOutNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string CancelNextStateNumber
        {
            get
            {
                return this.cancelNextStateNumberField;
            }
            set
            {
                this.cancelNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string FDKPressededNextStateNumber
        {
            get
            {
                return this.fDKPressededNextStateNumberField;
            }
            set
            {
                this.fDKPressededNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string FDKActiveMask
        {
            get
            {
                return this.fDKActiveMaskField;
            }
            set
            {
                this.fDKActiveMaskField = value;
            }
        }

        [XmlElement()]
        public Extension1 Extension1
        {
            get
            {
                return this.extension1Field;
            }
            set
            {
                this.extension1Field = value;
            }
        }


        [XmlElement()]
        public Extension2 Extension2
        {
            get
            {
                return this.extension2Field;
            }
            set
            {
                this.extension2Field = value;
            }
        }

        [XmlElement()]
        public JournalProperties Journal
        {
            get
            {
                return this.journalField;
            }
            set
            {
                this.journalField = value;
            }
        }

        [XmlElement()]
        public MoreTimeProperties MoreTime
        {
            get
            {
                return this.moreTimeField;
            }
            set
            {
                this.moreTimeField = value;
            }
        }
        #endregion Properties
    }

    [Serializable]
    public partial class Extension1
    {
        private string stateNumberField;
        private string minimumAcceptableWhitePercentageField;
        private string maximunAcceptableWhitePercentageField;
        private string imageCapturedNextStateNumberField;
        private string imageNotCapturedNextStateNumberField;
        private string hardwareErrorOrDeviceNotPresentNextStateNumberField;
        private string reserved1Field;
        private string reserved2Field;
        private string reserved3Field;

        public Extension1()
        {

        }

        #region Properties
        [XmlElement()]
        public string StateNumber
        {
            get
            {
                return this.stateNumberField;
            }
            set
            {
                this.stateNumberField = value;
            }
        }

        [XmlElement()]
        public string MinimumAcceptableWhitePercentage
        {
            get
            {
                return this.minimumAcceptableWhitePercentageField;
            }
            set
            {
                this.minimumAcceptableWhitePercentageField = value;
            }
        }

        [XmlElement()]
        public string MaximunAcceptableWhitePercentage
        {
            get
            {
                return this.maximunAcceptableWhitePercentageField;
            }
            set
            {
                this.maximunAcceptableWhitePercentageField = value;
            }
        }

        [XmlElement()]
        public string ImageCapturedNextStateNumber
        {
            get
            {
                return this.imageCapturedNextStateNumberField;
            }
            set
            {
                this.imageCapturedNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string ImageNotCapturedNextStateNumber
        {
            get
            {
                return this.imageNotCapturedNextStateNumberField;
            }
            set
            {
                this.imageNotCapturedNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string HardwareErrorOrDeviceNotPresentNextStateNumber
        {
            get
            {
                return this.hardwareErrorOrDeviceNotPresentNextStateNumberField;
            }
            set
            {
                this.hardwareErrorOrDeviceNotPresentNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string Reserved1
        {
            get
            {
                return this.reserved1Field;
            }
            set
            {
                this.reserved1Field = value;
            }
        }

        [XmlElement()]
        public string Reserved2
        {
            get
            {
                return this.reserved2Field;
            }
            set
            {
                this.reserved2Field = value;
            }
        }

        [XmlElement()]
        public string Reserved3
        {
            get
            {
                return this.reserved3Field;
            }
            set
            {
                this.reserved3Field = value;
            }
        }
        #endregion Properties
    }

    [Serializable]
    public partial class Extension2
    {
        private string stateNumberField;
        private string readingFingerScreenNumberField;
        private string checkFingerPositionScreenNumberField;
        private string removeFingerScreenNumberField;
        private string imageLocationScreenNumberField;
        private string pleaseWaitScreenNumberField;
        private string reserved1Field;
        private string reserved2Field;
        private string reserved3Field;

        public Extension2()
        {

        }

        #region Properties
        [XmlElement()]
        public string StateNumber
        {
            get
            {
                return this.stateNumberField;
            }
            set
            {
                this.stateNumberField = value;
            }
        }

        [XmlElement()]
        public string ReadingFingerScreenNumber
        {
            get
            {
                return this.readingFingerScreenNumberField;
            }
            set
            {
                this.readingFingerScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string CheckFingerPositionScreenNumber
        {
            get
            {
                return this.checkFingerPositionScreenNumberField;
            }
            set
            {
                this.checkFingerPositionScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string RemoveFingerScreenNumber
        {
            get
            {
                return this.removeFingerScreenNumberField;
            }
            set
            {
                this.removeFingerScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string ImageLocationScreenNumber
        {
            get
            {
                return this.imageLocationScreenNumberField;
            }
            set
            {
                this.imageLocationScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string PleaseWaitScreenNumber
        {
            get
            {
                return this.pleaseWaitScreenNumberField;
            }
            set
            {
                this.pleaseWaitScreenNumberField = value;
            }
        }

        [XmlElement()]
        public string Reserved1
        {
            get
            {
                return this.reserved1Field;
            }
            set
            {
                this.reserved1Field = value;
            }
        }

        [XmlElement()]
        public string Reserved2
        {
            get
            {
                return this.reserved2Field;
            }
            set
            {
                this.reserved2Field = value;
            }
        }

        [XmlElement()]
        public string Reserved3
        {
            get
            {
                return this.reserved3Field;
            }
            set
            {
                this.reserved3Field = value;
            }
        }
        #endregion Properties
    }
}