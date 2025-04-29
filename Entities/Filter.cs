using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entities
{
    public class Filter
    {
        public enum Ordering { DESC, ASC }

        public string OrderByColumn { get; set; }
        public Ordering Order { get; set; }
        public DateTime FROM { get; set; }
        public DateTime TO { get; set; }

        private string _format = "dd/MM/yyyy";

        public Filter()
        {

        }

        public Filter(string dateFrom, string dateTo, string orderby, string order)
        {
            DateTime _from = DateTime.MinValue;
            DateTime _to = DateTime.MaxValue;

            if(!DateTime.TryParseExact(dateFrom, _format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _from))
            {
                _from = DateTime.MinValue;
            }
            if (!DateTime.TryParseExact(dateTo, _format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _to))
            {
                _to = DateTime.Now;
            }

            FROM = _from;
            TO = _to;

            OrderByColumn = orderby;

            try
            {
                Order = (Ordering)Enum.Parse(typeof(Ordering), order.ToUpper());
            }
            catch (Exception)
            {
                Order = Ordering.DESC;
            }
        }

        public void SetFormat(string format)
        {
            _format = format;
        }
    }
}
