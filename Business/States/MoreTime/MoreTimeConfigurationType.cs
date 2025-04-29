using Entities;
using System.IO;
using System.Xml.Serialization;

namespace Business
{
    public class MoreTimeConfigurationType
    {
        private bool enableCancelKeyField;

        private bool enableEnterKeyField;

        private FDK_Types yesKeysField;

        private FDK_Types noKeysField;

        public StateEvent OnMoreTimeAdvice;


        public MoreTimeConfigurationType()
        {

            this.YesKeys = FDK_Types.A;
            this.NoKeys = FDK_Types.B;
            this.EnableEnterKey = true;
            this.EnableCancelKey = true;
            this.OnMoreTimeAdvice = new StateEvent(StateEvent.EventType.runScript, "MoreTimeAdvice", "");
        }

        public static T LoadConfig<T>(out bool ret)
        {
            ret = false;
            MoreTimeConfigurationType moreTimeConfigurationType = new MoreTimeConfigurationType();
            string configFolder = string.Format(@"{0}Config", Entities.Const.appPath);
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);
            string fileName = string.Format(@"{0}\MoreTimeConfig.xml", configFolder);
            return Utilities.Utils.GetGenericXmlData<T>(out ret, fileName, moreTimeConfigurationType);
        }

        public bool EnableCancelKey
        {
            get
            {
                return this.enableCancelKeyField;
            }
            set
            {
                this.enableCancelKeyField = value;
            }
        }

        public bool EnableEnterKey
        {
            get
            {
                return this.enableEnterKeyField;
            }
            set
            {
                this.enableEnterKeyField = value;
            }
        }

        [XmlElement("YesKeys")]
        public FDK_Types YesKeys
        {
            get
            {
                return this.yesKeysField;
            }
            set
            {
                this.yesKeysField = value;
            }
        }

        [XmlElement("NoKeys")]
        public FDK_Types NoKeys
        {
            get
            {
                return this.noKeysField;
            }
            set
            {
                this.noKeysField = value;
            }
        }
    }
}
