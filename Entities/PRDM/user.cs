using System.Runtime.Serialization;

namespace Entities
{
    [DataContract]
    public class user
    {
        private string loginField;
        private string passwordField;
        private string nameField;
        private string identifierField;
        private string roleField;
        private string deactivatedField;

        public user()
        {

        }

        #region Properties
        [DataMember]
        public string login
        {
            get
            {
                return this.loginField;
            }
            set
            {
                this.loginField = value;
            }
        }

        [DataMember]
        public string password
        {
            get
            {
                return this.passwordField;
            }
            set
            {
                this.passwordField = value;
            }
        }

        [DataMember]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [DataMember]
        public string identifier
        {
            get
            {
                return this.identifierField;
            }
            set
            {
                this.identifierField = value;
            }
        }

        [DataMember]
        public string role
        {
            get
            {
                return this.roleField;
            }
            set
            {
                this.roleField = value;
            }
        }

        [DataMember]
        public string deactivated
        {
            get
            {
                return this.deactivatedField;
            }
            set
            {
                this.deactivatedField = value;
            }
        }
        #endregion Properties
    }
}
