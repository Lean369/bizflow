using System;
using System.Xml;
using System.Xml.Serialization;
using Business.States;
using System.Collections.Generic;
using Entities;

namespace Business.LoginState
{
    [Serializable]
    public partial class PropertiesLoginState
    {
        private string screenNumberField;
        private string goodANextStateNumberField;
        private string goodBNextStateNumberField;
        private string goodCNextStateNumberField;
        private string cancelNextStateNumberField;
        private string timeOutNextStateNumberField;
        private List<RoleAttributes> rolesField;
        private string hostNameField;
        private bool operationByEnableField;
        private GetDataOperationMode_Type operationCodeDataField;
        private Extension1 extension1Field;
        private JournalProperties journalField;
        private MoreTimeProperties moreTimeField;
        public StateEvent OnInsertUserData;
        public StateEvent OnUserValidateError;
        public StateEvent OnVisiblePassword;

        public PropertiesLoginState()
        {
            this.screenNumberField = "";
            this.goodANextStateNumberField = "";
            this.goodBNextStateNumberField = "";
            this.goodCNextStateNumberField = "";
            this.cancelNextStateNumberField = "";
            this.timeOutNextStateNumberField = "";
            this.rolesField = new List<RoleAttributes>();
            this.hostNameField = "PrdmHost";
            this.operationByEnableField = true;
            this.operationCodeDataField = GetDataOperationMode_Type.fromList;
            this.extension1Field = new Extension1();
            this.journalField = new JournalProperties();
            this.moreTimeField = new MoreTimeProperties();
            this.OnInsertUserData = new StateEvent(StateEvent.EventType.navigate, "login.htm", "");
            this.OnUserValidateError = new StateEvent(StateEvent.EventType.runScript, "UserValidateError", "");
            this.OnVisiblePassword = new StateEvent(StateEvent.EventType.runScript, "VisiblePassword", "");            
        }

        internal void LoadDefaultConfiguration()
        {
            this.rolesField.Add(new RoleAttributes(Enums.UserRoles.USER, false));
            this.rolesField.Add(new RoleAttributes(Enums.UserRoles.SUPERVISOR, false));
            this.rolesField.Add(new RoleAttributes(Enums.UserRoles.ADMIN, true));
        }

        #region "Properties"
        [XmlElement()]
        public string ScreenNumber
        {
            get
            {
                return this.screenNumberField;
            }
            set
            {
                this.screenNumberField = value;
            }
        }

        [XmlElement()]
        public string GoodANextStateNumber
        {
            get
            {
                return this.goodANextStateNumberField;
            }
            set
            {
                this.goodANextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string GoodBNextStateNumber
        {
            get
            {
                return this.goodBNextStateNumberField;
            }
            set
            {
                this.goodBNextStateNumberField = value;
            }
        }

        [XmlElement()]
        public string GoodCNextStateNumber
        {
            get
            {
                return this.goodCNextStateNumberField;
            }
            set
            {
                this.goodCNextStateNumberField = value;
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
        public string HostName
        {
            get
            {
                return this.hostNameField;
            }
            set
            {
                this.hostNameField = value;
            }
        }
        
        [XmlElement()]
        public bool OperationByEnable
        {
            get
            {
                return this.operationByEnableField;
            }
            set
            {
                this.operationByEnableField = value;
            }
        }

        [XmlArrayAttribute()]
        public List<RoleAttributes> Roles
        {
            get
            {
                return this.rolesField;
            }
            set
            {
                this.rolesField = value;
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
        #endregion "Properties"
    }

    [Serializable]
    public class RoleAttributes
    {
        private Enums.UserRoles roleField;
        private bool CheckPasswordField;

        public RoleAttributes() { }

        public RoleAttributes(Enums.UserRoles _role, bool _checkPass) { this.roleField = _role; this.CheckPassword = _checkPass; }

        [XmlAttributeAttribute()]
        public Enums.UserRoles Role { get => roleField; set => roleField = value; }

        [XmlAttributeAttribute()]
        public bool CheckPassword { get => CheckPasswordField; set => CheckPasswordField = value; }
    }


    [Serializable]
    public partial class Extension1
    {
        private string stateNumberField;
        private string language1Field;
        private string language2Field;
        private string language3Field;
        private string language4Field;
        private string language5Field;
        private string language6Field;
        private string language7Field;
        private string language8Field;

        public Extension1() { }

        #region "Properties"
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
        public string Language1
        {
            get
            {
                return this.language1Field;
            }
            set
            {
                this.language1Field = value;
            }
        }

        [XmlElement()]
        public string Language2
        {
            get
            {
                return this.language2Field;
            }
            set
            {
                this.language2Field = value;
            }
        }

        [XmlElement()]
        public string Language3
        {
            get
            {
                return this.language3Field;
            }
            set
            {
                this.language3Field = value;
            }
        }

        [XmlElement()]
        public string Language4
        {
            get
            {
                return this.language4Field;
            }
            set
            {
                this.language4Field = value;
            }
        }

        [XmlElement()]
        public string Language5
        {
            get
            {
                return this.language5Field;
            }
            set
            {
                this.language5Field = value;
            }
        }

        [XmlElement()]
        public string Language6
        {
            get
            {
                return this.language6Field;
            }
            set
            {
                this.language6Field = value;
            }
        }

        [XmlElement()]
        public string Language7
        {
            get
            {
                return this.language7Field;
            }
            set
            {
                this.language7Field = value;
            }
        }

        [XmlElement()]
        public string Language8
        {
            get
            {
                return this.language8Field;
            }
            set
            {
                this.language8Field = value;
            }
        }
        #endregion "Properties"
    }
}