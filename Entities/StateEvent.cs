using System;
using System.Xml;
using System.Xml.Serialization;

namespace Entities
{
    [Serializable()]
    public class StateEvent
    {
        [SerializableAttribute()]
        public enum EventType { ignore, navigate, runScript, ndcScreen, printReceipt, printJournal, printJournalAndReceipt, sendTicketToBD, sendToHost }

        [XmlElement()]
        public EventType Action { get; set; }

        [XmlElement()]
        public string HandlerName { get; set; }

        [XmlElement()]
        public object Parameters { get; set; }

        public StateEvent() { }

        public StateEvent(EventType action, string handlerName, object parameters)
        {
            this.Action = action;
            this.HandlerName = handlerName;
            this.Parameters = parameters;
        }

        public StateEvent Clone()
        {
            return new StateEvent
            {
                Action = this.Action,
                HandlerName = this.HandlerName,
                Parameters = this.Parameters
            };
        }

        public StateEvent Clone(string parameters)
        {
            return new StateEvent
            {
                Action = this.Action,
                HandlerName = this.HandlerName,
                Parameters = parameters
            };
        }
    }
}
