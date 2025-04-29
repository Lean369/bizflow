using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entities.Functions
{
    public class MixCalculator
    {
        //private static MixCalculator _instance;
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        public Contents CurrentContents { get; set; }

        private TypeCassetteMapping TypeCassetteMapping;
        public string CurrencyValue { get; set; }

        public enum MixType
        {
            MIN_NUM_ITEMS,
            //MAX_NUM_ITEMS,
            //EQUAL_NUM_ITEMS,
        }

        public MixCalculator() { }

        public MixCalculator WithContents(Contents contents)
        {
            CurrentContents = contents;
            return this;
        }
        public MixCalculator SetCurrency(string currency)
        {
            CurrencyValue = currency;
            return this;
        }
        public MixCalculator SetTypeMapping(TypeCassetteMapping mapping)
        {
            TypeCassetteMapping = mapping;
            return this;
        }

        /* remove denomination notes that are not mapped to a TYPE */
        private Contents RemoveNoTypedNotes(Contents contentsIn)
        {
            if(TypeCassetteMapping is null)
                return contentsIn;

            string contents_str = Newtonsoft.Json.JsonConvert.SerializeObject(contentsIn);
            var contents = Newtonsoft.Json.JsonConvert.DeserializeObject<Contents>(contents_str);
            contents.LstDetail.FirstOrDefault(d => d.ContainerType.Equals("NOTEACCEPTOR") && d.Currency.Equals(this.CurrencyValue)).LstItems
                .RemoveAll(itm => !ExitsInMapping((int)ShifftDot(itm.Denomination, itm.Exponent), this.CurrencyValue));
            return contents;
        }
        private bool ExitsInMapping(int denomination, string currency)
        {
            return TypeCassetteMapping.TypeCassetteList.Exists(t => t.CurrencyIso.Equals(currency) && t.Denomination == denomination);
        }

        /*** Devuelve un Contents con una lista de Details conteniendo las denominaciones y sus num_items necesarios para cubrir el monto de 'amount' ***/
        public Contents CalculateContentsFromAmount(decimal amount, bool forceDispense = true)
        {
            this.CurrentContents = RemoveNoTypedNotes(this.CurrentContents);
            var contents = new Contents();
            Log.Trace("Contents con el que vamos a trabajar: {0}", Utilities.Utils.JsonSerialize(this.CurrentContents));
            if (forceDispense) //forzar para que el monto sea dispensable haciandolo divisible por la menor denominacion
            {
                Log.Trace("A - forceDispense on currency: {0}", this.CurrencyValue);
                var notes = new List<Item>();
                this.CurrentContents.LstDetail.Where(d => d.Currency != null && d.Currency.Equals(this.CurrencyValue)).ToList().ForEach(detail => {
                    Log.Trace("\t- cur:{0} | cid:{1} ", detail.Currency, detail.ContainerId);
                    notes.AddRange(detail.LstItems);
                });
                Log.Trace("B - forceDispense");
                var minDenomination = ShifftDot(notes.Where(i => i.Num_Items > 0).Min(n => n.Denomination), notes[0]?.Exponent ?? 0);
                Log.Info("Menor denominacion: {0}", minDenomination);
                amount = Decimal.Truncate(amount / minDenomination) * minDenomination;
            }
            Log.Trace("Monto divisible por la menor denominacion: {0}", amount);
            //get coin/note items with availability
            var demItems = new List<Item>();
            var coinItems = new List<Item>();
            this.CurrentContents.LstDetail.Where(d => d.Currency.Equals(this.CurrencyValue) && d.ContainerType.Equals("COINDISPENSER")).ToList().ForEach(dl =>
            {
                dl.LstItems?.ForEach(it => {
                    if (it.Num_Items > 0)
                        coinItems.Add(it);
                });
            });
            var noteItems = this.CurrentContents.LstDetail.FirstOrDefault(d => d.Currency.Equals(this.CurrencyValue) && d.ContainerType.Equals("NOTEACCEPTOR"))?.LstItems?.Where(i => i.Num_Items > 0).ToList();
            if (coinItems != null)
                demItems.AddRange(coinItems);
            if (noteItems != null)
                demItems.AddRange(noteItems);

            if (demItems == null || demItems.Count == 0)
            {
                Log.Error("No availability of any item for currency {0}", this.CurrencyValue);
                return contents;
            }
            var salItems = MinNumItems_BasicAlgorithm(amount, demItems);
            if (salItems.Count == 0)
            {
                foreach (var item in demItems)
                {
                    salItems = MinNumItems_BasicAlgorithm(amount, demItems, item); //(*1) re solicitar pero ignorando un item en especifico
                    if (salItems.Count > 0)
                        break;
                }
            }
            contents.LstDetail = new List<Detail>
            {
                new Detail
                {
                    LstItems = salItems,
                    Currency = this.CurrencyValue,
                }
            };
            return contents;
        }

        public decimal[] CalculateCoinAndNoteAmount(decimal totalAmount)
        {
            var content = CalculateContentsFromAmount(totalAmount);
            Log.Trace("Contents resultado de CalculateContentsFromAmout() DATA: {0}", Utilities.Utils.JsonSerialize(content));
            decimal noteAmount = 0;
            decimal coinAmount = 0;
            content.LstDetail.Where(d => d.Currency.Equals(this.CurrencyValue))?.ToList().ForEach(dl => {
                dl.LstItems.ForEach(it =>
                {
                    if (it.Type.Equals("NOTE"))
                        noteAmount += ShifftDot(it.Denomination, it.Exponent) * it.Num_Items;
                    if (it.Type.Equals("COIN"))
                        coinAmount += ShifftDot(it.Denomination, it.Exponent) * it.Num_Items;
                });
            });
            return new decimal[] { noteAmount, coinAmount };
        }



        /* METODO 1 - GREEDY
         * 
            PROBLEMA: hay casos que este metodo no puede resolver, ejemplo: dispensar 600 si tenemos solo disponible billetes de 500 y 200,
            porque intentará dispensar con el de 500 y luego no podrá sastifacer los 100 faltantes con el de 200 que le queda.

            (*1) SOLUCION temporal: si no obtengo un resultado, vuelvo a solicitar pero probando con una unidad menos por cada nota para remover la que cause el inconveniente.
            Este metodo es menos efectivo en encontrar combinaciones que el de fuerza bruta, pero es mas eficiente en performance y puede ser suficiente para la mayoria de casos,
            más aun teniendo en cuenta que se va a requerir un minimo para cada item para poder operar.
         */
        /// <summary>
        /// Calcula las notas/coins a dispensar requiriendo la menor cantidad de items.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="notes"></param>
        /// <returns></returns>
        public List<Item> MinNumItems_BasicAlgorithm(decimal amount, List<Item> notes, Item itemToBeReduced = null)
        {
            List<Item> result = new List<Item>();
            //alterar monto para obtener un valor dispensable
            var minDenomination = ShifftDot(notes.Min(n => n.Denomination), notes[0]?.Exponent ?? 0);
            amount = Decimal.Truncate(amount / minDenomination) * minDenomination;
            // Ordenar las notas de mayor a menor valor
            notes.Sort((x, y) => y.Denomination.CompareTo(x.Denomination));
            //if (itemsToBeIgnored > 0) 
            //    notes.GetRange(itemsToBeIgnored, notes.Count); //ignorar numero de items solicitado, de los de mayor denominacion
            // Iterar por cada nota
            foreach (Item note in notes)
            {
                var denomination = ShifftDot(note.Denomination, note.Exponent);
                decimal notesToDispense = Decimal.Truncate(Math.Min(amount / denomination, note.Num_Items)); //truncar decimales

                if (itemToBeReduced == note && notesToDispense >= 1)
                    notesToDispense--;  //reducir un item que pueda impedir calculo correcto de la combinacion

                if (notesToDispense > 0)
                {
                    result.Add(new Item { Denomination = note.Denomination, Num_Items = notesToDispense, Type = note.Type, Exponent = note.Exponent });
                    amount -= denomination * notesToDispense;
                }
                if (amount == 0)
                    break;
            }
            // Si no se puede dispensar la cantidad exacta, retornar una lista vacía
            if (amount > 0)
            {
                Log.Error("El monto exacto no pudo ser sastisfecho. Restan: {0}", amount);
                result.Clear();
            }
            return result;
        }










        /* METODO 2 - BRUTE FORCE
         * 
         Utiliza el enfoque de fuerza bruta para generar todas las combinaciones posibles de notas. 
         Luego, itera sobre estas combinaciones para encontrar la que requiere la menor cantidad de notas. 
         La función auxiliar GetCombinations utiliza una técnica recursiva para generar todas las combinaciones posibles.

         ATENCION: 
        Tener en cuenta que este enfoque de fuerza bruta puede ser ineficiente para un número grande de notas o montos grandes, 
        ya que la complejidad aumenta exponencialmente con el número de notas disponibles.
         */
        public List<Item> MinNumItems_BruteForceAlgorithm(decimal amount, List<Item> notes)
        {
            List<List<Item>> combinations = GetCombinations(amount, notes);

            List<Item> result = null;
            int minNoteCount = int.MaxValue;

            foreach (List<Item> combination in combinations)
            {
                int noteCount = (int)combination.Sum(note => note.Num_Items);

                if (noteCount < minNoteCount)
                {
                    minNoteCount = noteCount;
                    result = combination;
                }
            }

            return result ?? new List<Item>();
        }

        private List<List<Item>> GetCombinations(decimal amount, List<Item> notes)
        {
            List<List<Item>> combinations = new List<List<Item>>();
            List<Item> currentCombination = new List<Item>();

            GenerateCombinations(amount, notes, 0, currentCombination, combinations);

            return combinations;
        }

        private void GenerateCombinations(decimal amount, List<Item> notes, int index, List<Item> currentCombination, List<List<Item>> combinations)
        {
            if (amount == 0)
            {
                combinations.Add(new List<Item>(currentCombination));
                return;
            }

            if (index == notes.Count || amount < 0)
                return;

            Item note = notes[index];

            for (int quantity = 0; quantity <= note.Num_Items; quantity++)
            {
                currentCombination.Add(new Item { Denomination = note.Denomination, Num_Items = quantity });
                GenerateCombinations(amount - (note.Denomination * quantity), notes, index + 1, currentCombination, combinations);
                currentCombination.RemoveAt(currentCombination.Count - 1);
            }
        }

        //MixCalculator / Core.cs / CoinDispenser.cs
        /// <summary>
        /// Shifft dot on decimal number according to the number of places.
        /// </summary>
        public static decimal ShifftDot(decimal number, int places)
        {
            if (places > 0) // Si el número de lugares es positivo, desplaza el punto hacia la izquierda
            {
                for (int i = 0; i < places; i++)
                {
                    number *= 10;
                }
            }
            else if (places < 0) // Si el número de lugares es negativo, desplaza el punto hacia la derecha
            {
                for (int i = 0; i > places; i--)
                {
                    number /= 10;
                }
            }
            return number;
        }


    }
}
