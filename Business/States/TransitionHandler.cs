using Entities;
using System;
using System.Collections.Generic;
//using System.Threading.Tasks;
using System.Threading;

namespace Business
{
    public class TransitionHandler
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        public bool paused = false;
        public bool end = false;
        private StateTransition newStateTransition;
        private Core Core;

        public TransitionHandler(Core _core)
        {
            //this.name = name;
            this.Core = _core;
            this.end = false;
            this.paused = false;
        }

        public void Work()
        {
            try
            {
                Log.Debug("/--->");
                while (!end)
                {
                    if (this.Core.CurrentTransitionState != null)
                    {
                        if (!this.Core.CurrentTransitionState.Equals(this.Core.NextTransitionState) && !paused)//--->COMPARADOR PARA CAMBIO DE ESTADO
                        {
                            if (this.Core.NextTransitionState != null)
                            {
                                //1)- Verifico que todos los procesos esten finalizados.
                                this.VerifyAndCloseProcessState();
                                //2)- Si se va al estado "000" se asume que el equipo NO esta en uso (por si no se ejecuta el EndVisit)
                                if (this.Core.NextTransitionState != null)
                                    if (this.Core.NextTransitionState.Equals("000"))
                                        this.Core.Sdo.SOH.InUseState = Const.InUseMode.NotInUse;
                                //3)- Seteo en el core el nuevo estado que se ejecutará 
                                this.Core.CurrentTransitionState = this.Core.NextTransitionState;
                                //4)- Inicio el nuevo estado.
                                this.StartActivity(this.Core.NextTransitionState);
                            }
                            else//lo llevo al Default Close
                            {
                                this.NextToDefaultClose();
                            }
                        }
                    }
                    //evitar uso de procesador innecesario
                    //no se cambia de estado miles de veces por segundo, asi que es suficiente
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        public void StartActivity(string stateNumber)
        {
            try
            {
                if (this.Core.Download.DicOfStatesTransitions.TryGetValue(stateNumber, out newStateTransition))
                {
                    Log.Info("=> Next Transition State: {0}", stateNumber);
                    if (newStateTransition.InitializeActivity(this.Core))
                    {
                        this.Core.CurrentStateName = newStateTransition.ActivityName;
                        newStateTransition.ActivityStart();
                    }
                    else
                        Log.Error("Transition State doesn´t initialize: {0}", newStateTransition.ActivityName);
                }
                else//Si el estado no existe en la lista, lo llevo al Default Close
                {
                    this.NextToDefaultClose();
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        private void NextToDefaultClose()
        {
            try
            {
                Log.Error("/--->");
                string nextTransitionState = string.IsNullOrEmpty(this.Core.NextTransitionState) ? "Empty" : this.Core.NextTransitionState;
                Log.Error("Transition State not found: {0}", nextTransitionState);
                this.Core.ErrorTransitionState = nextTransitionState;
                this.Core.NextTransitionState = "ZZZ"; //Al default close
                this.Core.CurrentStateName = "DefaultClose";
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }


        public void StopActivity(string stateNumber)
        {
            bool flag = false;
            try
            {
                foreach (KeyValuePair<string, StateTransition> entry in this.Core.Download.DicOfStatesTransitions)
                {
                    if (entry.Key.Equals(stateNumber))
                    {
                        Log.Info("Stop State: \"{0}\"", entry.Key);
                        entry.Value.Quit();
                        flag = true;
                    }
                }
                if (!flag)
                    Log.Error("State \"{0}\" not found", stateNumber);
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        /// <summary>
        /// Recorre el diccionario de StatesTransitions y cierra los estados que NO hayan finalizado.
        /// </summary>
        public void VerifyAndCloseProcessState()
        {
            try
            {
                foreach (KeyValuePair<string, StateTransition> entry in this.Core.Download.DicOfStatesTransitions)
                {
                    if (entry.Value.CurrentState != StateTransition.ProcessState.FINALIZED)
                    {
                        Log.Warn("State: \"{0}\" hasn´t finalized.", entry.Value.ActivityName);
                        entry.Value.Quit();
                        //GlobalAppData.Instance.GetSOH().InUseState = Entities.Health.InUseMode.NotInUse;                       
                    }
                }
            }
            catch (Exception ex) { Log.Fatal(ex); }
        }

        //Pausa la transición de estados
        public void Pause()
        {
            lock (this)
            {
                if (this.newStateTransition != null)
                    this.newStateTransition.Quit();
                this.paused = true;
                Log.Debug("/--->");
            }
        }

        //Reanuda la transición de estados
        public void Play()
        {
            lock (this)
            {
                Log.Debug("/--->");
                this.paused = false;
                this.Core.CurrentTransitionState = string.Empty;
                Monitor.PulseAll(this);
            }
        }

        //Finaliza la ejecución total de las transiciones
        public void End()
        {
            lock (this)
            {
                this.end = true;
                Log.Debug("/--->");
                Monitor.PulseAll(this);
            }
        }
    }

}