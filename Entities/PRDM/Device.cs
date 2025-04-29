using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entities
{
    public class Device
    {
        private string NameField;
        private string ErrorCodeField;
        private string DescriptionField;

        #region Propieties

        public string Name
        {
            get { return this.NameField; }
            set { this.NameField = value; }
        }
        public string ErrorCode
        {
            get { return this.ErrorCodeField; }
            set { this.ErrorCodeField = value; }
        }
        public string Description
        {
            get { return this.DescriptionField; }
            set { this.DescriptionField = value; }
        }
        #endregion

        #region Constructors

        public Device() { }
        public Device(string name, string errorCode, string description)
        {
            this.Name = name;
            this.ErrorCode = errorCode;
            this.Description = description;
        }

        #endregion
    }

}