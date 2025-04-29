using Business.States;
using Entities;
using System;

namespace Business.DefaultCloseState
{
    [Serializable]
    public partial class PropertiesDefaultCloseState
    {
        private PropertiesScreens screensField;
        private PropertiesTimeOut timeOutField;
        private JournalProperties journalField;
        public StateEvent OnClose;

        public PropertiesDefaultCloseState()
        {
            this.screensField = new PropertiesScreens();
            this.timeOutField = new PropertiesTimeOut();
            this.journalField = new JournalProperties();
            this.OnClose = new StateEvent(StateEvent.EventType.navigate, "", "");
        }

        public PropertiesScreens Screens
        {
            get
            {
                return this.screensField;
            }
            set
            {
                this.screensField = value;
            }
        }

        public PropertiesTimeOut TimeOut
        {
            get
            {
                return this.timeOutField;
            }
            set
            {
                this.timeOutField = value;
            }
        }

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
    }

    [Serializable]
    public class PropertiesScreens
    {
        private string errorField;

        private string takeCardField;

        private string cardEjectFailField;

        private string cardCapturedField;

        public PropertiesScreens()
        {
            this.errorField = "E00";
            this.takeCardField = "";
            this.cardEjectFailField = "";
            this.cardCapturedField = "";
        }

        public string Error
        {
            get
            {
                return this.errorField;
            }
            set
            {
                this.errorField = value;
            }
        }

        public string TakeCard
        {
            get
            {
                return this.takeCardField;
            }
            set
            {
                this.takeCardField = value;
            }
        }

        public string CardEjectFail
        {
            get
            {
                return this.cardEjectFailField;
            }
            set
            {
                this.cardEjectFailField = value;
            }
        }

        public string CardCaptured
        {
            get
            {
                return this.cardCapturedField;
            }
            set
            {
                this.cardCapturedField = value;
            }
        }
    }

    [Serializable]
    public class PropertiesTimeOut
    {
        private int errorScreenField;

        private int cardEjectFailField;

        private int cardCapturedField;

        public PropertiesTimeOut()
        {
            this.errorScreenField = 5;
            this.cardEjectFailField = 5;
            this.cardCapturedField = 5;
        }

        public int ErrorScreen
        {
            get
            {
                return this.errorScreenField;
            }
            set
            {
                this.errorScreenField = value;
            }
        }

        public int CardEjectFail
        {
            get
            {
                return this.cardEjectFailField;
            }
            set
            {
                this.cardEjectFailField = value;
            }
        }

        public int CardCaptured
        {
            get
            {
                return this.cardCapturedField;
            }
            set
            {
                this.cardCapturedField = value;
            }
        }
    }
}