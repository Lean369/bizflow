using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Entities
{
    [ComVisible(true)]
    public class Factura
    {
        public string CodigoIdentificador { get; set; } //<codigoIdentificador>16909078-98  -6</codigoIdentificador>
        public string Detalle { get; set; } //<detalle>FACTURA ENERGIA</detalle>
        //public DateTime FechaEmision { get; set; } //<fechaEmision>2016-12-22T00:00:00</fechaEmision>
        public string FechaEmision { get; set; } //<fechaEmision>2016-12-22T00:00:00</fechaEmision>
        public string FechaEmisionStr { get; set; } //dd/mm/aaaa, de fechaEmision
        //public DateTime FechaVencimiento { get; set; } //<fechaVencimiento>2017-01-02T00:00:00</fechaVencimiento>
        public string FechaVencimiento { get; set; } //<fechaVencimiento>2017-01-02T00:00:00</fechaVencimiento>
        public string FechaVencimientoStr { get; set; } //dd/mm/aaaa, de fechaVencimiento
        public long NumeroFactura { get; set; } //<numeroFactura>16909078</numeroFactura>
        public int OrdenCorrelativo { get; set; } //<ordenCorrelativo>1</ordenCorrelativo>
        public double PagoMinimo { get; set; } //<pagoMinimo>49.8</pagoMinimo>
        public double TotalFactura { get; set; } //<totalFactura>166</totalFactura>
        public double TotalPagar { get; set; } //<totalPagar>166</totalPagar>


        public Factura(string codigoIdentificador, string detalle, string fechaEmision, string fechaEmisionStr, string fechaVencimiento, String fechaVencimientoStr, long numeroFactura, int ordenCorrelativo, double pagoMinimo, double totalFactura, double totalPagar)
        {
            CodigoIdentificador = codigoIdentificador;
            Detalle = detalle;
            FechaEmision = fechaEmision;
            FechaEmisionStr = fechaEmisionStr;
            FechaVencimiento = fechaVencimiento;
            FechaVencimientoStr = fechaVencimientoStr;
            NumeroFactura = numeroFactura;
            OrdenCorrelativo = ordenCorrelativo;
            PagoMinimo = pagoMinimo;
            TotalFactura = totalFactura;
            TotalPagar = totalPagar;
        }

        public Factura()
        {
        }
    }
}
