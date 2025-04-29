using Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Entities.Detail;

namespace Business
{
    public class CountersManager
    {
        private static CountersManager _instance { get; set; }
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private Business.Counters CountersCore;
        private DeviceConfigurations DevConf;
        private TypeCassetteMapping TypeCassettesConfig;
        private AlephATMAppData AlephATMAppData;

        public static CountersManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CountersManager();
                return _instance;
            }
        }

        private CountersManager()
        {
            CountersCore = new Business.Counters();
            getDevConf();
        }

        private void getDevConf()
        {
            if (!Directory.Exists($"{Const.appPath}Config"))
                Directory.CreateDirectory($"{Const.appPath}Config");
            AlephATMAppData = AlephATMAppData.GetAppData(out bool ret);
            if (ret)
                this.DevConf = DeviceConfigurations.GetObject(out ret, AlephATMAppData.TerminalModel, AlephATMAppData.DefaultCurrency);
            else
                this.DevConf = DeviceConfigurations.GetObject(out ret, Enums.TerminalModel.NONE, "ARS");
            if (!ret)
                Log.Error("Can´t get aleph app data configuration.");
        }

        /// <summary>
        /// Load and return COUNTERS from xml file.
        /// </summary>
        /// <returns></returns>
        public Counters GetCountersFromFile()
        {
            if (!CountersCore.GetInitialCounters(out CountersCore, this.DevConf.BAGconfig.BagCapacity))
            {
                Log.Error("Can´t get counters.");
                return null;
            }
            return CountersCore;
        }

        public TypeCassetteMapping GetCassetteConfig()
        {
            if(TypeCassettesConfig != null)
                return TypeCassettesConfig;

            if (!TypeCassetteMapping.GetMapping(out TypeCassettesConfig, AlephATMAppData.DefaultCurrency))
            {
                Log.Error("Could not get TypeCassetteMapping configuration.");
                return null;
            }
            return TypeCassettesConfig;
        }

        /// <summary>
        /// Adds the received number of items to the specified ContainerId that matches denomination.
        /// </summary>
        /// <param name="coinCounter"></param>
        /// <returns></returns>
        public (CompletionCodeEnum code, string message, List<Detail> details) AddItemsToCounter(CounterItem coinCounter)
        {
            return ItemsTransactionOnCounter(coinCounter, Counters.TransactionType.REFILL);
        }

        /// <summary>
        /// Subtract the received number of items to the specified ContainerId that matches denomination.
        /// </summary>
        /// <param name="coinCounter"></param>
        /// <returns></returns>
        public (CompletionCodeEnum code, string message, List<Detail> details) SubtractItemsFromCounter(CounterItem coinCounter)
        {
            return ItemsTransactionOnCounter(coinCounter, Counters.TransactionType.DISPENSE);
        }

        private (CompletionCodeEnum code, string message, List<Detail> details) ItemsTransactionOnCounter(CounterItem coinCounter, Counters.TransactionType transactionType)
        {
            GetCountersFromFile(); //make sure we got latest updates
            try
            {
                ContainerIDType counterTypeVal;
                if (string.IsNullOrEmpty(coinCounter.ContainerId) && Enum.IsDefined(typeof(ContainerIDType), coinCounter.Index))
                    counterTypeVal = (ContainerIDType)coinCounter.Index;
                else Enum.TryParse(coinCounter.ContainerId, out counterTypeVal);
                var cc = DeepCopy(CountersCore.Contents.LstDetail.FirstOrDefault(x => x.ContainerId == counterTypeVal && x.Currency.Equals(coinCounter.Currency))); //get DETAIL that matches container id type | make clone to avoid manipulate original
                cc.LstItems.RemoveAll(x => Entities.Functions.MixCalculator.ShifftDot(x.Denomination, x.Exponent) != coinCounter.Denomination); //remove all items that dont match denomination
                var citm = IsHopper(counterTypeVal) ? cc.LstItems[0] : cc.LstItems.FirstOrDefault(x => Entities.Functions.MixCalculator.ShifftDot(x.Denomination, x.Exponent) == coinCounter.Denomination); //only edit ITEM that matches denomination or first item if it's a hopper (sinces it's expected to be only one item per detail in case of hopper)
                citm.Num_Items = (int)coinCounter.NumItems;
                citm.Total = (int)(citm.Denomination * citm.Num_Items);
                var details_affected = new List<Detail> { cc };
                var res = CountersCore.UpdateContents(new List<Detail> { cc }, transactionType);
                var operationMessage = transactionType == Counters.TransactionType.REFILL ? "added" : "subtracted";
                return (res ? CompletionCodeEnum.Success : CompletionCodeEnum.InternalError, res ? $"Items {operationMessage}" : $"Could not be {operationMessage}.", details_affected);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
                return (CompletionCodeEnum.InternalError, "Failed on items transaction counter.", null);
            }
        }

        public (CompletionCodeEnum code, string message) RemoveFromContents(string currency, string containerType, int denomination)
        {
            var remove = CountersCore.RemoveItem(currency, containerType, denomination);
            return (remove ? CompletionCodeEnum.Success : CompletionCodeEnum.InternalError, remove ? "Items removed" : "Could not be removed.");
        }

        /// <summary>
        /// Set to zero number of items and total of all ITEMS belonging to DETAIL that matches container id.
        /// </summary>
        public (CompletionCodeEnum code, string message, Entities.Item Item, ContainerIDType ContainerIDType, bool AlreadyZero) SetZeroNumItemValue(CounterItem coinCounter)
        {
            GetCountersFromFile(); //make sure we got latest updates

            ContainerIDType counterTypeVal;
            if (string.IsNullOrEmpty(coinCounter.ContainerId) && Enum.IsDefined(typeof(ContainerIDType), coinCounter.Index))
                counterTypeVal = (ContainerIDType)coinCounter.Index;
            else Enum.TryParse(coinCounter.ContainerId, out counterTypeVal);

            var cc = CountersCore.Contents.LstDetail.FirstOrDefault(x => x.ContainerId == counterTypeVal && x.Currency.Equals(coinCounter.Currency)); //get DETAIL that matches container id type 
            var citm = IsHopper(counterTypeVal) ? cc.LstItems[0] : cc.LstItems.FirstOrDefault(x => Entities.Functions.MixCalculator.ShifftDot(x.Denomination, x.Exponent) == coinCounter.Denomination); //only edit ITEM that matches denomination or first item if it's a hopper (sinces it's expected to be only one item per detail if is a hopper)
            var affectedItem = DeepCopy(citm);
            bool alreadyZero = citm.Num_Items == 0;
            CountersCore.ClearItem(counterTypeVal, citm, coinCounter.Currency); //save changes on file
            return (CompletionCodeEnum.Success, "Item Cleared.", affectedItem, counterTypeVal, alreadyZero);
        }

        public (CompletionCodeEnum code, string message) SetZeroNumItemValue(List<Detail> details)
        {
            var clear = CountersCore.ClearDetails(details);
            return (clear ? CompletionCodeEnum.Success : CompletionCodeEnum.InternalError, clear ? "Item Cleared." : "Could not be cleared.");
        }

        private ContainerIDType NextAvailableHopperID()
        {
            var availableContainerID = new List<Detail.ContainerIDType> { ContainerIDType.Hopper_1, ContainerIDType.Hopper_2, ContainerIDType.Hopper_3, ContainerIDType.Hopper_4, ContainerIDType.Hopper_5, ContainerIDType.Hopper_6, ContainerIDType.Hopper_7, ContainerIDType.Hopper_8, ContainerIDType.Hopper_9 };
            var fdetails = CountersCore.Contents.LstDetail.Where(d => d.ContainerType.Equals("COINDISPENSER"));
            foreach (var fdetail in fdetails)
            {
                if (availableContainerID.Contains(fdetail.ContainerId))
                    availableContainerID.Remove(fdetail.ContainerId);
            }
            return availableContainerID.First();
        }

        /* Verifies if a Content.Detail exists for a given Currency and Denomination, if not, it creates it.
         * ONLY for type COINDISPENSER
         */
        public bool COINDISPENSER_AddAsDetailIfNotExists(string currency, int denomination, int exponent, string old_currency = null, int old_denomination = 0) 
        {
            var result = true;
            GetCountersFromFile(); //make sure we got latest updates

            Detail fdetail = null;
            Item fitem = null;
            if (old_currency != null && old_denomination != 0) //if old_currency and old_denomination are provided, it means we want to move an item from one denomination/currency to another
            {
                var fdetails = CountersCore.Contents.LstDetail.Where(d => d.ContainerType.Equals("COINDISPENSER") && d.Currency.Equals(old_currency));
                fdetail = fdetails.FirstOrDefault(d => d.LstItems.Any(i => i.Denomination == old_denomination));
                fitem = fdetail?.LstItems?.FirstOrDefault(i => i.Denomination == old_denomination);
            }   
            else
            {
                var fdetails = CountersCore.Contents.LstDetail.Where(d => d.ContainerType.Equals("COINDISPENSER") && d.Currency.Equals(currency));
                fitem = fdetails?.FirstOrDefault(d => d.LstItems.Any(i => i.Denomination == denomination))?.LstItems?.FirstOrDefault(i => i.Denomination == denomination);
            }
            if (fitem == null)
            {
                var newdetail = new Detail
                {
                    Currency = currency,
                    ContainerId = NextAvailableHopperID(),
                    ContainerType = "COINDISPENSER",
                    LstItems = new List<Item> {
                        new Item
                        {
                            Denomination = denomination,
                            Type = "COIN",
                            Num_Items = 0,
                            Total = 0,
                            Exponent = exponent
                        }
                    }
                };
                result = CountersCore.UpdateContents(new List<Detail> { newdetail }, Counters.TransactionType.MOVE_IN);
            }
            else if(fitem != null && old_currency != null && old_denomination != 0) //si el item existe pero con los valores viejos
            {
                fdetail.Currency = currency;
                fitem.Denomination = denomination;
                result = CountersCore.UpdateContents(new List<Detail> { fdetail }, Counters.TransactionType.MOVE_IN); //verificar si esto sirve para simplemente actualizar un detail existente....
            }
            return result;
        }

        private bool IsHopper(ContainerIDType type)
        {
            var containerIDTypes = new ContainerIDType[] {
                ContainerIDType.Hopper_1,
                ContainerIDType.Hopper_2,
                ContainerIDType.Hopper_3,
                ContainerIDType.Hopper_4,
                ContainerIDType.Hopper_5,
                ContainerIDType.Hopper_6,
                ContainerIDType.Hopper_7,
                ContainerIDType.Hopper_8,
                ContainerIDType.Hopper_9
            };
            return containerIDTypes.Contains(type);
        }

        public static T DeepCopy<T>(T self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }


        public class CounterItem
        {
            public int Index { get; set; }
            public string ContainerId { get; set; }
            public string Currency { get; set; }
            public decimal Denomination { get; set; }
            public int NumItems { get; set; }
            public int Exponent { get; set; }
        }
    }
}
