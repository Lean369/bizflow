using Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Utilities;

namespace Business
{
    [Serializable()]
    [XmlRoot("Counters")]
    public class Counters
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private string Path = $"{Const.appPath}Counters\\Counters.xml";
        private int TSNField = 1;
        private int BATCHField = 1;
        private string COLLECTIONIDField = DateTime.Now.ToString("yyyyMMddHHmmss");
        private int CLOSEField = 1;
        private int RETRACTField = 1;
        private Contents ContentsField;//Guarda los contadores de valores declarados y validados
        public bool LogicalFullBin = false;
        public int LogicalFullBinThreshold = 0;//Se carga por defecto al iniciar la aplicación
        public int TotalDepositedNotes = 0;
        private object ALock = new object();
        [XmlIgnore]
        internal List<String> AcceptedCurrencies = new List<string>();
        public enum TransactionType { DEPOSIT, DEPOSIT_DECLARED, COLLECTION, COLLECTION_DECLARED, DISPENSE, REFILL, MOVE_IN, MOVE_OUT };

        public Counters()
        {
            this.GenerateInitialCounters();
        }

        /// <summary>
        /// Genera una lista de contadores por defecto a partir del archivo CashInAcceptedNotes.xml
        /// </summary>
        /// <param name="terminalModel"></param>
        private void GenerateInitialCounters()
        {
            List<CashInAcceptedNotes> cimAcceptedNotes;
            List<Item> lstItems;
            try
            {
                this.ContentsField = new Contents();
                this.ContentsField.LstDetail = new List<Detail>();
                if (CashInAcceptedNotes.GetCashInAcceptedNotes(out cimAcceptedNotes))
                {
                    var items = from i in cimAcceptedNotes
                                select i.CurId;
                    this.AcceptedCurrencies = items.Distinct().ToList();
                    if (this.AcceptedCurrencies.Count > 0)
                    {
                        foreach (string curr in this.AcceptedCurrencies)//Carga los elementos Content por defecto
                        {
                            lstItems = new List<Item>();
                            foreach (CashInAcceptedNotes note in cimAcceptedNotes)
                            {
                                if (note.CurId.Equals(curr) && note.Configured)
                                {
                                    lstItems.Add(new Item(note.Values * 100, 0, 0, "NOTE", "", ""));
                                }
                            }
                            if (lstItems.Count > 0)
                                this.ContentsField.LstDetail.Add(new Detail(curr, Detail.ContainerIDType.CashAcceptor, "NOTEACCEPTOR", null, lstItems));
                        }
                    }
                    else
                        Log.Warn("CashIn accepted notes list is empty");
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Obtiene los contadores desde el archivo Counters.xml o crea por defecto dicho archivo si no existiera
        /// </summary>
        /// <param name="counters"></param>
        /// <param name="logicalFullBinThreshold"></param>
        /// <param name="terminalModel"></param>
        /// <returns></returns>
        public bool GetInitialCounters(out Counters counters, int logicalFullBinThreshold) //ojo, cambié de Internal a Public
        {
            bool ret = false;
            counters = new Counters();//Para MEI no carga valores por defecto, los genera a partir del template del CIM
            counters.LogicalFullBinThreshold = logicalFullBinThreshold; //Asigna el valor de carga máxima de la bolsa
            try
            {
                Log.Debug("/--->");
                if (counters.Contents.LstDetail.Count != 0)//Si no levanta contadores por defecto, reintenta la operaciòn luego de recibir los datos del template del CIM
                {
                    counters = Utilities.Utils.GetGenericXmlData<Counters>(out ret, this.Path, counters);
                    counters.Contents.LstDetail.RemoveAll(r => r.LstItems.Count == 0); //Fix para issue de Details sin Items
                    if (ret)
                    {
                        counters.UpdateTotalDepositedNotes();//Obtiene el contador total de billetes depositados
                    }
                    else
                        Log.Error("Can't get counters");
                }
                else
                {
                    Log.Warn("LstDetail is empty");
                    ret = true;
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        internal int GetTSN()
        {
            return this.TSN;
        }

        internal int GetBATCH()
        {
            return this.BATCH;
        }

        internal string GetCOLLECTIONID()
        {
            return COLLECTIONID;
        }

        internal int GetCLOSE()
        {
            return this.CLOSE;
        }

        internal int GetRETRACT()
        {
            return this.RETRACT;
        }

        internal Contents GetContents()
        {
            List<Detail> listDetail = new List<Detail>();
            List<Item> listItem = new List<Item>();
            foreach (Detail d in this.Contents.LstDetail)
            {
                //if (d.ContainerId == Detail.ContainerIDType.Depository)
                //{
                d.LstItems.ForEach(x => { listItem.Add(new Item(x.Denomination, x.Num_Items, x.Total, x.Type, x.Barcode, "")); });//Remuevo el item "Category"
                listDetail.Add(new Detail(d.Currency, d.ContainerId, d.ContainerType, null, new List<Item>(listItem)));
                listItem.Clear();
                //}
                //else if (d.ContainerId == Detail.ContainerIDType.CashAcceptor)
                //{
                //    d.LstItems.ForEach(x => { listItem.Add(new Item(x.Denomination, x.Num_Items, x.Total, x.Type, "", "")); });//Remuevo el item "Category"
                //    listDetail.Add(new Detail(d.Currency, d.ContainerId, d.ContainerType, null, new List<Item>(listItem)));
                //    listItem.Clear();
                //}
                //listDetail.Add(d);
            }
            return new Contents(listDetail);
        }

        internal Contents GetContents(Detail.ContainerIDType containerID)
        {
            Contents contents = new Contents();
            //var list = this.Contents.LstDetail.Where(x => x.ContainerId.Equals(containerID));
            List<Detail> list = this.Contents.LstDetail.FindAll(x => x.ContainerId == containerID);
            return new Contents(list);
        }

        internal List<Detail> GetDetails(Detail.ContainerIDType containerID)
        {
            return Contents.LstDetail.FindAll((Detail x) => x.ContainerId == containerID);
        }

        public bool ClearItem(Detail.ContainerIDType containerID, Item item, string currency)
        {
            //this search is necessary in case the item received is a new object or a copy, otherwise it could be modified directlty
            var citem = this.Contents.LstDetail.FirstOrDefault(x => x.ContainerId == containerID && x.Currency.Equals(currency))?.LstItems.Find(x => x.Denomination == item.Denomination);
            if (citem == null)
                return false;
            citem.Num_Items = 0;
            citem.Total = 0;
            //item.Num_Items = 0;
            //item.Total = 0;
            this.SerializeCounters();
            //Task.Run(async () => await this.SerializeCounters()).Wait();
            //this.SerializeCounters().GetAwaiter().GetResult();
            return true;
        }

        /// <summary>
        /// Set num items to zero for all items in the received list and then serialize the counters
        /// </summary>
        /// <param name="refDetails">A reference list of Detail/Items to set to zero in local counters.</param>
        /// <returns></returns>
        public bool ClearDetails(List<Detail> refDetails)
        {
            try
            {
                this.Contents.LstDetail.Where(d => refDetails.Any(rd => rd.Currency.Equals(d.Currency) && rd.ContainerType == d.ContainerType && rd.ContainerId == d.ContainerId))
                .ToList()
                .ForEach(dt =>
                {
                    var refItems = refDetails.Find(rd => rd.Currency.Equals(dt.Currency) && rd.ContainerType == dt.ContainerType && rd.ContainerId == dt.ContainerId).LstItems;
                    dt.LstItems.Where(it => refItems?.Any(ri => ri.Denomination == it.Denomination) ?? false).ToList().ForEach(i => { i.Num_Items = 0; i.Total = 0; });
                });
                this.SerializeCounters();
                return true;
            }
            catch(Exception ex) { Log.Fatal(ex); return false; }
        }

        public bool ClearContents() //params Detail.ContainerIDType[] containerIDs
        {
            //if (containerIDs.Length == 0)
            //    containerIDs = new Detail.ContainerIDType[] { Detail.ContainerIDType.CashAcceptor }; //default value
            //bool ret = false;
            List<Detail> lstDetailClear = new List<Detail>();
            try
            {
                Log.Debug("/--->");
                var items = from j in this.Contents.LstDetail
                            where j.ContainerId == Detail.ContainerIDType.CashAcceptor
                            select j;
                lstDetailClear = items.ToList();//Genero una lista solo con los billetes validados
                if (lstDetailClear.Count != 0)
                {
                    foreach (Detail d in lstDetailClear)
                    {
                        foreach (Item i in d.LstItems)
                        {
                            i.Num_Items = 0;
                            i.Total = 0;
                        }
                    }
                }
                this.Contents.LstDetail.Clear();
                this.Contents.LstDetail.AddRange(lstDetailClear);
                this.SerializeCounters();
                //Actualizo la cantidad de billetes en bolsa
                this.UpdateTotalDepositedNotes();
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return true;
        }

        public bool UpdateContents(List<Detail> lstDetailNew, TransactionType transactionType = TransactionType.DEPOSIT) //pase de internal a public
        {
            bool ret = false;
            List<Item> lstAddItems = new List<Item>();
            Counters counters = new Counters();
            var incomeTypes = new TransactionType[] { TransactionType.DEPOSIT, TransactionType.DEPOSIT_DECLARED, TransactionType.REFILL, TransactionType.MOVE_IN };
            var isIncomeType = incomeTypes.Contains(transactionType);
            try
            {
                Log.Debug("/--->");
                foreach (Detail detNew in lstDetailNew)
                {
                    if (detNew.LstItems.Count > 0)
                    {
                        if (detNew.ContainerId == Detail.ContainerIDType.Depository)//Si es sobre lo agrego de una.
                        {
                            this.Contents.LstDetail.Add(detNew);
                        }
                        else if (detNew.ContainerId == Detail.ContainerIDType.CashAcceptor)//Si es dinero validado tengo que sumar contadores
                        {
                            //A)- Agrego los Items ya existen en la lista | CORREGIR!: this.Contents.LstDetail no esta llegando actualizado luego de add cash en menuAdmin
                            Detail currentDetail = this.Contents.LstDetail.Find(x => x.ContainerId == Detail.ContainerIDType.CashAcceptor && x.Currency.Equals(detNew.Currency));
                            if (currentDetail != null)//TRUE: Ya exixte un Detail de dinero validado con el mismo currency entrante
                            {
                                var items = from i in currentDetail.LstItems
                                            join j in detNew.LstItems on i.Type equals j.Type
                                            where i.Denomination == j.Denomination
                                            select j;

                                List<Item> detInter = items.ToList();
                                if (detInter.Count != 0) //Verifico si existen billetes con la misma denominaciòn
                                {
                                    //A)- Adiciono los Items duplicados
                                    foreach (Item itemNew in detInter)
                                    {
                                        foreach (Item itemCurr in currentDetail.LstItems)
                                        {
                                            if (itemCurr.Denomination == itemNew.Denomination) //sumo los items de la misma denominación
                                            {
                                                itemCurr.Num_Items += isIncomeType ? itemNew.Num_Items : -itemNew.Num_Items;
                                                itemCurr.Total = itemCurr.Num_Items * itemCurr.Denomination;
                                            }
                                        }
                                    }
                                    //B)- Agrego Items que no esten previamente en la lista
                                    if (detNew.LstItems.Count > detInter.Count)
                                    {
                                        foreach (Item itm in detNew.LstItems)//Obtengo los billetes NO repetidos
                                        {
                                            var existingBill = detInter.FirstOrDefault(x => x.Denomination == itm.Denomination);
                                            if (existingBill == null)
                                            {
                                                itm.Num_Items = isIncomeType ? itm.Num_Items : -itm.Num_Items; //ATENCION: valor negativo ó cero (?).. (si no existe un Detail.Item del que se quiere sustraer)
                                                currentDetail.LstItems.Add(itm);
                                            }
                                        }
                                    }
                                }
                                else//Si hay Details previos pero de distinta denominación, ingreso todo sin duplicar denominaciones
                                {
                                    this.LoadNewItems(detNew, isIncomeType);
                                }
                            }
                            else //FALSE: NO existe un Detail de dinero validado con el mismo currency entrante
                            {
                                this.LoadNewItems(detNew, isIncomeType); //ATENCION: esto se ejecuta?? porque dentro de LoadNewItems esta de nuevo el filtro de currency...
                            }
                            //Actualizo la cantidad de billetes en bolsa
                            this.UpdateTotalDepositedNotes();
                        }
                        else // if (hoppersType.Contains(detNew.ContainerId))
                        {
                            //buscar si ya existe el detail
                            Detail currentDetail = this.Contents.LstDetail.Find(x => x.ContainerId == detNew.ContainerId && x.Currency.Equals(detNew.Currency));
                            if (currentDetail != null)
                            {
                                foreach (var item in detNew.LstItems)
                                {
                                    var priorItem = currentDetail.LstItems.FirstOrDefault(i => i.Type == item.Type && i.Denomination == item.Denomination);
                                    if (priorItem != null) //if already exits, updates it
                                    {
                                        priorItem.Num_Items += isIncomeType ? item.Num_Items : -item.Num_Items;
                                        priorItem.Total = priorItem.Num_Items * priorItem.Denomination;
                                    }
                                    else if (isIncomeType) //if item does not exist, add it
                                        currentDetail.LstItems.Add(item);
                                    else
                                        Log.Error("COUNTERS ERROR: An outcome type transaction over 'CONTENTS' failed to execute due to missing 'ITEM'");
                                }
                            }
                            else
                            {
                                if (isIncomeType)
                                    this.Contents.LstDetail.Add(detNew);
                                else
                                    Log.Error("COUNTERS ERROR: An outcome type transaction over 'CONTENTS' failed to execute due to missing 'DETAIL'");
                            }
                            //if(transactionType == TransactionType.DEPOSIT && lstDetailNew.Any(a => a.ContainerId == Detail.ContainerIDType.CashAcceptor)) //si es de cashtoday
                            //    this.UpdateTotalDepositedNotes();
                        }
                    }
                    else
                    {
                        Log.Error("List of Items is empty");
                    }
                }
                this.SerializeCounters();
                ret = true;
            }
            catch (Exception ex) { Log.Fatal(ex); }
            return ret;
        }

        private void LoadNewItems(Detail newDetail, bool isIncomeType)
        {
            Detail det = this.Contents.LstDetail.Find(x => x.ContainerId == Detail.ContainerIDType.CashAcceptor && x.Currency.Equals(newDetail.Currency));
            if (det is null) //EqualityComparer<Detail>.Default.Equals(det, default)
            {
                det = new Detail(newDetail.Currency, newDetail.ContainerId, newDetail.ContainerType, newDetail.CollectionId, new List<Item>());
                this.Contents.LstDetail.Add(det);
            }

            foreach (Item itm in newDetail.LstItems)//Obtengo los billetes NO repetidos
            {
                var existingBill = det.LstItems.FirstOrDefault(x => x.Denomination == itm.Denomination);
                if (existingBill != null)
                {
                    existingBill.Num_Items += itm.Num_Items;
                    existingBill.Total = itm.Num_Items * itm.Denomination;
                }
                else
                {
                    itm.Num_Items = isIncomeType ? itm.Num_Items : -itm.Num_Items; //ATENCION: valor negativo ó cero (?).. (si no existe un Detail.Item del que se quiere sustraer)
                    det.LstItems.Add(itm);
                }
            }
        }

        private int HandleTransactionOverItems(bool isIncomeType, int numItems, bool isItemNew = false)
        {
            bool allowNegativeValues = false;
            numItems = !isItemNew ? numItems : allowNegativeValues ? numItems : 0;
            return isIncomeType ? numItems : -numItems;
        }

        //Removes an item and its container Detail object if it has no items left
        public bool RemoveItem(string currency, string containerType, int denomination)
        {
            var fdetails = this.Contents.LstDetail.Where(x => x.Currency.Equals(currency) && x.ContainerType.Equals(containerType));
            Detail detail = null;
            foreach (var fdetail in fdetails)
            {
                var fitem = fdetail.LstItems?.FirstOrDefault(i => i.Denomination == denomination);
                if (fitem != null)
                {
                    if (fitem.Num_Items > 0)
                    {
                        Log.Warn("Detail.Item Denomination {0} has Num_Items so it cannot be deleted.", fitem.Denomination);
                        return false;
                    }
                    fdetail.LstItems.Remove(fitem); //remove item
                    detail = fdetail;
                }
            }
            if (detail != null && detail.LstItems.Count == 0) //if there is no more items, remove Detail as well.
            {
                this.Contents.LstDetail.Remove(detail);
                this.SerializeCounters();
            }
            return true;
        }

        private void SerializeCounters()
        {
            bool flagSeri = false;
            bool flagDeseri = false;
            Counters counters = new Counters();
            string pathTemp = $"{Const.appPath}Counters\\Counters{DateTime.Now.ToString("yyyyMMddHHmmss")}{DateTime.Now.Millisecond.ToString("000")}.xml";
            string pathBk = $"{Const.appPath}Counters\\Counters.xml.bk";
            string pathArchive = $"{Const.appPath}Counters\\Counters.xml.{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            try
            {
                lock (this.ALock)
                {
                    if (File.Exists(Path))
                        File.Copy(this.Path, pathBk, overwrite: true);
                    Utils.ObjectToXml<Counters>(out flagSeri, this, this.Path);
                    Utils.GetGenericXmlData<Counters>(out flagDeseri, this.Path, new Counters());
                    //Verificación
                    if (flagSeri && flagDeseri)
                        Log.Info("Serialize counters OK");
                    else
                    {
                        File.Copy(pathBk, pathTemp, true);
                        Log.Error("Serialize Counters error");
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        internal void UpdateTotalDepositedNotes()
        {
            try
            {
                Log.Debug("/--->");
                this.TotalDepositedNotes = 0;
                foreach (Detail detail in this.Contents.LstDetail)
                {
                    if (detail.ContainerId == Detail.ContainerIDType.CashAcceptor)//Solo contabilizo los billetes
                    {
                        foreach (Item item in detail.LstItems)
                        {
                            this.TotalDepositedNotes += (int)item.Num_Items;
                        }
                    }
                }
                this.LogicalFullBin = this.TotalDepositedNotes > this.LogicalFullBinThreshold ? true : false;
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        internal int UpdateTSN()
        {
            Log.Debug("/--->");
            if (this.TSN >= 9999)
                this.TSN = -1;
            this.TSN++;
            this.SerializeCounters();
            return this.TSN;
        }

        internal void ResetTSN(int tsn)
        {
            Log.Debug("/--->");
            if (tsn >= 9999)
                this.TSN = 0;
            else
                this.TSN = tsn;
            this.SerializeCounters();
        }

        internal void ReplaceTSN(int value)
        {
            Log.Debug("/--->");
            this.TSN = value;
            this.SerializeCounters();
        }

        internal void UpdateLogicalFullBinThreshold(int value)
        {
            Log.Debug("/--->");
            this.LogicalFullBinThreshold = value;
            this.SerializeCounters();
        }

        internal int UpdateBATCH()
        {
            Log.Debug("/--->");
            if (this.BATCH >= 9999)
                this.BATCH = -1;
            this.BATCH++;
            this.SerializeCounters();
            return this.BATCH;
        }

        internal string UpdateCOLLECTIONID()
        {
            this.COLLECTIONID = DateTime.Now.ToString("yyyyMMddHHmmss");
            Log.Debug("/--->{0}", this.COLLECTIONID);
            this.SerializeCounters();
            return this.COLLECTIONID;
        }
        internal int UpdateCLOSE()
        {
            Log.Debug("/--->");
            if (this.CLOSE >= 9999)
                this.CLOSE = -1;
            this.CLOSE++;
            this.SerializeCounters();
            return this.CLOSE;
        }

        internal void UpdateRETRACT(int value)
        {
            Log.Debug("/--->");
            this.RETRACT += value;
            this.SerializeCounters();
        }

        #region "Properties"
        //Transaction secuence number
        [XmlElement("TSN")]
        public int TSN
        {
            get { return this.TSNField; }
            set
            {
                this.TSNField = value;
            }
        }

        //Lote
        [XmlElement("BATCH")]
        public int BATCH
        {
            get { return this.BATCHField; }
            set
            {
                this.BATCHField = value;
            }
        }

        [XmlElement("COLLECTIONID")]
        public string COLLECTIONID
        {
            get
            {
                return COLLECTIONIDField;
            }
            set
            {
                COLLECTIONIDField = value;
            }
        }

        //Cierre contable
        [XmlElement("CLOSE")]
        public int CLOSE
        {
            get { return this.CLOSEField; }
            set
            {
                this.CLOSEField = value;
            }
        }

        //Contador de retracts
        [XmlElement("RETRACT")]
        public int RETRACT
        {
            get { return this.RETRACTField; }
            set
            {
                this.RETRACTField = value;
            }
        }

        [XmlElement("Contents")]
        public Contents Contents
        {
            get { return this.ContentsField; }
            set
            {
                this.ContentsField = value;
            }
        }

        #endregion "Properties"
    }
}
