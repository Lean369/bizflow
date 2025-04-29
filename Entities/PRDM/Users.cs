using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Entities
{
    public class Users
    {
        #region Propieties

        private string dTYPEField;
        private int idField;
        private string customerCardField;
        private bool deactivatedField;
        private string identifierField;
        private string internalIdField;
        private string loginField;
        private string nameField;
        private string passMD5Field;
        private string passwordField;
        private string roleField;
        private string availableChannelField;
        private string logicalIdField;


        public string DTYPE { get => dTYPEField; set => dTYPEField = value; }
        public int Id { get => idField; set => idField = value; }
        public string CustomerCard { get => customerCardField; set => customerCardField = value; }
        public bool Deactivated { get => deactivatedField; set => deactivatedField = value; }
        public string Identifier { get => identifierField; set => identifierField = value; }
        public string InternalId { get => internalIdField; set => internalIdField = value; }
        public string Login { get => loginField; set => loginField = value; }
        public string Name { get => nameField; set => nameField = value; }
        public string PassMD5 { get => passMD5Field; set => passMD5Field = value; }
        public string Password { get => passwordField; set => passwordField = value; }
        public string Role { get => roleField; set => roleField = value; }
        public string AvailableChannel { get => availableChannelField; set => availableChannelField = value; }
        public string LogicalId { get => logicalIdField; set => logicalIdField = value; }


        #endregion

        #region Constructor
        public Users() { this.Init(); }
        public Users(string dTYPE, int id, string customerCard, bool deactivated, string identifier, string internalId,
                     string login, string name, string passMD5, string password, string role, string availableChannel, string logicalId)
        {
            this.DTYPE = dTYPE;
            this.Id = id;
            this.CustomerCard = customerCard;
            this.Deactivated = deactivated;
            this.Identifier = identifier;
            this.InternalId = internalId;
            this.Login = login;
            this.Name = name;
            this.PassMD5 = passMD5;
            this.Password = password;
            this.Role = role;
            this.AvailableChannel = availableChannel;
            this.LogicalId = logicalId;
        }
        public void Init()
        {
            this.DTYPE = String.Empty;
            this.Id = 0;
            this.CustomerCard = String.Empty;
            this.Deactivated = true;
            this.Identifier = String.Empty;
            this.InternalId = String.Empty;
            this.Login = String.Empty;
            this.Name = String.Empty;
            this.PassMD5 = String.Empty;
            this.Password = String.Empty;
            this.Role = String.Empty;
            this.AvailableChannel = String.Empty;
            this.LogicalId = String.Empty;
        }
        #endregion
    }
}
