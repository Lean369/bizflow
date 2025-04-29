//using System.Xml.Serialization;
using Entities;

namespace Business.PreSetOperationCodeBufferState
{

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PropertiesPreSetOperationCodeBufferState/1.0.0.0/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.AlephATM.com/states/PropertiesPreSetOperationCodeBufferState/1.0.0.0/", IsNullable = false)]
    public partial class PropertiesPreSetOperationCodeBufferState
    {
        private string nextStateNumberField;
        private int bufferEntriesClearedField;
        private int bufferEntriesSetToAField;
        private int bufferEntriesSetToBField;
        private int bufferEntriesSetToCField;
        private int bufferEntriesSetToDField;
        private Extension extensionField;
        private PropertiesJournal journalField;

        public PropertiesPreSetOperationCodeBufferState()
        {
           this.nextStateNumberField = "";
            this.bufferEntriesClearedField = -1;
            this.bufferEntriesSetToAField = -1;
            this.bufferEntriesSetToBField = -1;
            this.bufferEntriesSetToCField = -1;
            this.bufferEntriesSetToDField = -1;
            this.extensionField = new Extension();
            this.journalField = new PropertiesJournal();
        }

        public string NextStateNumber
        {
            get
            {
                return this.nextStateNumberField;
            }
            set
            {
                this.nextStateNumberField = value;
            }
        }

        public int BufferEntriesCleared
        {
            get
            {
                return this.bufferEntriesClearedField;
            }
            set
            {
                this.bufferEntriesClearedField = value;
            }
        }

        public int BufferEntriesSetToA
        {
            get
            {
                return this.bufferEntriesSetToAField;
            }
            set
            {
                this.bufferEntriesSetToAField = value;
            }
        }

        public int BufferEntriesSetToB
        {
            get
            {
                return this.bufferEntriesSetToBField;
            }
            set
            {
                this.bufferEntriesSetToBField = value;
            }
        }

        public int BufferEntriesSetToC
        {
            get
            {
                return this.bufferEntriesSetToCField;
            }
            set
            {
                this.bufferEntriesSetToCField = value;
            }
        }

        public int BufferEntriesSetToD
        {
            get
            {
                return this.bufferEntriesSetToDField;
            }
            set
            {
                this.bufferEntriesSetToDField = value;
            }
        }

        /// <comentarios/>
        public Extension Extension
        {
            get
            {
                return this.extensionField;
            }
            set
            {
                this.extensionField = value;
            }
        }
        public PropertiesJournal Journal
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

    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PreSetOperationCodeBufferState/1.0.0.0/")]
    public partial class PropertiesJournal
    {

        private bool enableJournalField;

        private string inputCancelFileNameField;

        private string inputTimeoutFileNameField;

        public PropertiesJournal()
        {
            this.enableJournalField = true;
            this.inputCancelFileNameField = "InputCancel.txt";
            this.inputTimeoutFileNameField = "InputTimeout.txt";
        }

        /// <comentarios/>
        public bool EnableJournal
        {
            get
            {
                return this.enableJournalField;
            }
            set
            {
                this.enableJournalField = value;
            }
        }

        /// <comentarios/>
        public string InputCancelFileName
        {
            get
            {
                return this.inputCancelFileNameField;
            }
            set
            {
                this.inputCancelFileNameField = value;
            }
        }

        /// <comentarios/>
        public string InputTimeoutFileName
        {
            get
            {
                return this.inputTimeoutFileNameField;
            }
            set
            {
                this.inputTimeoutFileNameField = value;
            }
        }
    }


    /// <comentarios/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AlephATM.com/states/PreSetOperationCodeBufferState/1.0.0.0/")]
    public partial class Extension
    {
        private string stateNumberField;
        private int bufferEntriesSetToFField;
        private int bufferEntriesSetToGField;
        private int bufferEntriesSetToHField;
        private int bufferEntriesSetToIField;
        private EmptyElement_Type itemField;
        private EmptyElement_Type item1Field;
        private EmptyElement_Type item2Field;
        private EmptyElement_Type item3Field;

        public Extension()
        {
            this.stateNumberField = "";
            this.bufferEntriesSetToFField = -1;
            this.bufferEntriesSetToGField = -1;
            this.bufferEntriesSetToHField = -1;
            this.bufferEntriesSetToIField = -1;
        }

        /// <comentarios/>
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

        /// <comentarios/>
        public int BufferEntriesSetToF
        {
            get
            {
                return this.bufferEntriesSetToFField;
            }
            set
            {
                this.bufferEntriesSetToFField = value;
            }
        }

        /// <comentarios/>
        public int BufferEntriesSetToG
        {
            get
            {
                return this.bufferEntriesSetToGField;
            }
            set
            {
                this.bufferEntriesSetToGField = value;
            }
        }

        /// <comentarios/>
        public int BufferEntriesSetToH
        {
            get
            {
                return this.bufferEntriesSetToHField;
            }
            set
            {
                this.bufferEntriesSetToHField = value;
            }
        }

        /// <comentarios/>
        public int BufferEntriesSetToI
        {
            get
            {
                return this.bufferEntriesSetToIField;
            }
            set
            {
                this.bufferEntriesSetToIField = value;
            }
        }
    }

}