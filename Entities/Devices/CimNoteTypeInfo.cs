using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Entities
{
    /// <summary>
    /// Clase utilizada para manejar los datos del template del aceptador
    /// </summary>
    [DataContract]
    public class CimNoteTypeInfo
    {
        [DataMember]
        public List<Note> Notes;

        [DataMember]
        public string SelectedCurrency;

        public CimNoteTypeInfo()
        {
            Notes = new List<Entities.Note>();
        }

        public CimNoteTypeInfo(List<Note> _Notes)
        {
            Notes = _Notes;
        }
    }

    [DataContract]
    public class Note
    {
        [DataMember]
        public bool configured;
        [DataMember]
        public long values;
        [DataMember]
        public int release;
        [DataMember]
        public int noteId;
        [DataMember]
        public string curId;


        public Note(bool _configured, long _values, int _release, int _noteId, string _curId)
        {
            configured = _configured;
            values = _values;
            release = _release;
            noteId = _noteId;
            curId = _curId;
        }
    }
}
