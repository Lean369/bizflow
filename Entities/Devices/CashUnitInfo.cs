using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Entities
{
    /// <summary>
    /// Clase utilizada para almacenar el estado de los cassettes
    /// </summary>
    [DataContract]
    public class CashUnitInfo
    {
        [DataMember]
        public int TellerID;

        /// <summary>
        /// Specifies the number of cash unit structures returned.
        /// </summary>
        [DataMember]
        public int Count;

        /// <summary>
        /// Pointer to an array of pointers to WFSCDMCASHUNIT structures.
        /// </summary>
        [DataMember]
        public List<CashUnit> LstCashUnit = new List<CashUnit>();

        public CashUnitInfo()
        {
            this.TellerID = 0;
            this.Count = 0;
            this.LstCashUnit = new List<CashUnit>();
        }

        public CashUnitInfo(int tellerId, int count, List<CashUnit> _cashUnit)
        {
            this.TellerID = tellerId;
            this.Count = count;
            this.LstCashUnit = _cashUnit;
        }

        public object Clone()
        {
            return new CashUnitInfo
            {
                LstCashUnit = this.LstCashUnit,
                TellerID = this.TellerID,
                Count = this.Count
            };
        }

        public string ToString(string tab)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(tab);
            sb.AppendLine(this.GetType().Name);

            tab = tab + "\t";
            sb.Append(tab + "├> ");
            sb.Append("TellerID".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(TellerID + " (" + TellerID.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Count".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Count + " (" + Count.ToString("D") + ") ");

            for (int i = 0; i < this.LstCashUnit.Count; i++)
            {
                sb.AppendLine(this.LstCashUnit[i].ToString(tab));
            }

            return sb.ToString();
        }
    }

    [DataContract]
    public class CashUnit
    {
        public CashUnit() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CashUnit(int number, string type, string cashUnitName, string unitId, string currencyId,
            int values, int initialCount, int count, int rejectCount, int minimun, int maximun,
            bool appLock, string status, int numPhysicalCUs, List<PHCU> lstPhysical, int dispensedCount, int presentedCount, int retractedCount)
        {

            this.Number = number;
            this.Type = type;
            this.CashUnitName = cashUnitName;
            this.UnitID = unitId;
            this.CurrencyID = currencyId;
            this.Values = values;
            this.InitialCount = initialCount;
            this.Count = count;
            this.RejectCount = rejectCount;
            this.Minimum = minimun;
            this.Maximum = maximun;
            this.AppLock = appLock;
            this.Status = status;
            this.NumPhysicalCUs = numPhysicalCUs;
            this.LstPhysical = lstPhysical;
            this.DispensedCount = dispensedCount;
            this.PresentedCount = presentedCount;
            this.RetractedCount = retractedCount;
        }

        /// <summary>
        /// Index number of the cash unit structure.
        /// </summary>
        [DataMember]
        public int Number;

        /// <summary>
        /// Type of cash unit.
        /// </summary>
        [DataMember]
        public string Type;

        /// <summary>
        /// A name which helps to identify the Logical type of the cash unit.
        /// </summary>
        [DataMember]
        public string CashUnitName;

        /// <summary>
        /// The Cash Unit Identifier.
        /// </summary>
        [DataMember]
        public string UnitID;

        /// <summary>
        /// A three character array storing the ISO format Currency ID.
        /// </summary>
        [DataMember]
        public string CurrencyID;

        /// <summary>
        /// Supplies the value of a single item in the cash unit.
        /// </summary>
        [DataMember]
        public int Values;

        /// <summary>
        /// Initial number of items contained in the cash unit.
        /// </summary>
        [DataMember]
        public int InitialCount;

        /// <summary>
        /// The meaning of this count depends on the type of cash unit.
        /// </summary>
        [DataMember]
        public int Count;

        /// <summary>
        /// The number of items from this cash unit which are in the reject bin, and which have not been
        /// accessible to a customer.
        /// </summary>
        [DataMember]
        public int RejectCount;

        /// <summary>
        /// This field is not applicable to retract and reject cash units.
        /// </summary>
        [DataMember]
        public int Minimum;

        /// <summary>
        /// This field is only applicable to retract and reject cash units.
        /// </summary>
        [DataMember]
        public int Maximum;

        /// <summary>
        /// This field does not apply to reject or retract cash units.
        /// </summary>
        [DataMember]
        public bool AppLock;

        /// <summary>
        /// Supplies the status of the cash unit.
        /// </summary>
        [DataMember]
        public string Status;

        /// <summary>
        /// The number of physical cash unit structures returned in the following lppPhysical array.
        /// </summary>
        [DataMember]
        public int NumPhysicalCUs;

        /// <summary>
        /// Pointer to an array of pointers to WFSCDMPHCU structures.
        /// </summary>
        [DataMember]
        public List<PHCU> LstPhysical = new List<PHCU>();

        /// <summary>
        /// The number of items dispensed from all the physical cash units associated with this cash unit.
        /// </summary>
        [DataMember]
        public int DispensedCount;

        /// <summary>
        /// The number of items from all the physical cash units associated with this cash unit that have
        /// been presented to the customer.
        /// </summary>
        [DataMember]
        public int PresentedCount;

        /// <summary>
        /// The number of items that have been accessible to a customer and retracted into all the physical
        /// cash units associated with this cash unit.
        /// </summary>
        [DataMember]
        public int RetractedCount;

        /// <summary>
        /// Retorna una cadena que representa al objeto actual.
        /// </summary>
        /// <param name="tab">Se le pasa una cadena como parámetro.</param>
        /// <returns>Retorna una cadena que representa al objeto actual.</returns>
        public string ToString(string tab)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(tab);
            sb.AppendLine(this.GetType().Name);

            tab = tab + "\t";
            sb.Append(tab + "├> ");
            sb.Append("Number".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Number + " (" + Number.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Type".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Type + " (" + Type.ToString() + ") ");

            sb.Append(tab + "├> ");
            sb.Append("CashUnitName".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(CashUnitName);

            sb.Append(tab + "├> ");
            sb.Append("UnitID".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(UnitID);

            sb.Append(tab + "├> ");
            sb.Append("CurrencyID".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(CurrencyID);

            sb.Append(tab + "├> ");
            sb.Append("Values".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Values + " (" + Values.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("InitialCount".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(InitialCount + " (" + InitialCount.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Count".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Count + " (" + Count.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("RejectCount".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(RejectCount + " (" + RejectCount.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Minimum".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Minimum + " (" + Minimum.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Maximum".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Maximum + " (" + Maximum.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("AppLock".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(AppLock.ToString());

            sb.Append(tab + "├> ");
            sb.Append("Status".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Status.ToString());

            sb.Append(tab + "├> ");
            sb.Append("NumPhysicalCUs".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(NumPhysicalCUs + " (" + NumPhysicalCUs.ToString("D") + ") ");

            for (int i = 0; i < LstPhysical.Count; i++)
            {
                sb.AppendLine(LstPhysical[i].ToString(tab));
            }

            sb.Append(tab + "├> ");
            sb.Append("DispensedCount".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(DispensedCount + " (" + DispensedCount.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("PresentedCount".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(PresentedCount + " (" + PresentedCount.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("RetractedCount".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(RetractedCount + " (" + RetractedCount.ToString("D") + ") ");

            return sb.ToString();
        }
    }

    [DataContract]
    public class PHCU
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PHCU()
        {

        }

        public PHCU(string physicalPositionName, string unitID, int initialCount, int count, int rejectCount, int maximum, string pStatus,
                    bool hardwareSensor, int dispensedCount, int presentedCount, int retractedCount)
        {
            this.PhysicalPositionName = physicalPositionName;
            this.UnitID = unitID;
            this.InitialCount = initialCount;
            this.Count = count;
            this.RejectCount = rejectCount;
            this.Maximum = maximum;
            this.PStatus = pStatus;
            this.HardwareSensor = hardwareSensor;
            this.DispensedCount = dispensedCount;
            this.PresentedCount = presentedCount;
            this.RetractedCount = retractedCount;
        }
        /// <summary>
        /// A name identifying the physical location of the cash unit within the CDM.
        /// </summary>
        [DataMember]
        public string PhysicalPositionName;

        /// <summary>
        /// A 5 character array uniquely identifying the physical cash unit.
        /// </summary>
        [DataMember]
        public string UnitID;

        /// <summary>
        /// Initial number of items contained in the cash unit.
        /// </summary>
        [DataMember]
        public int InitialCount;

        /// <summary>
        /// As defined by the Logical ulCount description but applies to a single physical cash unit.
        /// </summary>
        [DataMember]
        public int Count;

        /// <summary>
        /// As defined by the Logical ulRejectCount description but applies to a single physical cash unit.
        /// </summary>
        [DataMember]
        public int RejectCount;

        /// <summary>
        /// The maximum number of items the cash unit can hold.
        /// </summary>
        [DataMember]
        public int Maximum;

        /// <summary>
        /// Supplies the status of the physical cash unit.
        /// </summary>
        [DataMember]
        public string PStatus;

        /// <summary>
        /// Specifies whether or not threshold events can be generated based on hardware sensors in
        /// the device.
        /// </summary>
        [DataMember]
        public bool HardwareSensor;

        /// <summary>
        /// As defined by the Logical ulDispensedCount description but applies to a single physical
        /// cash unit.
        /// </summary>
        [DataMember]
        public int DispensedCount;

        /// <summary>
        /// As defined by the Logical ulPresentedCount description but applies to a single physical
        /// cash unit.
        /// </summary>
        [DataMember]
        public int PresentedCount;

        /// <summary>
        /// As defined by the Logical ulRetractedCount description but applies to a single physical
        /// cash unit.
        /// </summary>
        [DataMember]
        public int RetractedCount;

        /// <summary>
        /// Retorna una cadena que representa al objeto actual.
        /// </summary>
        /// <param name="tab">Se le pasa una cadena como parámetro.</param>
        /// <returns>Retorna una cadena que representa al objeto actual</returns>
        public string ToString(string tab)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(tab);
            sb.AppendLine(this.GetType().Name);

            tab = tab + "\t";

            sb.Append(tab + "├> ");
            sb.Append("PhysicalPositionName".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(PhysicalPositionName);

            sb.Append(tab + "├> ");
            sb.Append("UnitID".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(UnitID);

            sb.Append(tab + "├> ");
            sb.Append("InitialCount".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(InitialCount + " (" + InitialCount.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Count".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Count + " (" + Count.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("RejectCount".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(RejectCount + " (" + RejectCount.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("Maximum".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(Maximum + " (" + Maximum.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("PStatus".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(PStatus + " (" + PStatus.ToString() + ") ");

            sb.Append(tab + "├> ");
            sb.Append("HardwareSensor".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(HardwareSensor.ToString());

            sb.Append(tab + "├> ");
            sb.Append("DispensedCount".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(DispensedCount + " (" + DispensedCount.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("PresentedCount".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(PresentedCount + " (" + PresentedCount.ToString("D") + ") ");

            sb.Append(tab + "├> ");
            sb.Append("RetractedCount".PadRight(19, ' '));
            sb.Append(": ");
            sb.AppendLine(RetractedCount + " (" + RetractedCount.ToString("D") + ") ");

            return sb.ToString();
        }
    }
}
