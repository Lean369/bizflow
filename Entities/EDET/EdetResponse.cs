using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;


namespace Entities
{

    public class EdetResponse
    {
        protected static string LOG_TAG = "EDETRES";


        public string DireccionSuministro { get; set; }
        public List<Factura> Facturas { get; set; }
        public string NombreCliente { get; set; }

        public double TotalPagar { get; set; }
        public double PagoMinimo { get; set; }

        // respuesta al pago
        public string Resultado { get; set; }

        //builder
        public EdetResponse()
        {
            /***DireccionSuministro = "";
            TotalPagar = 0.0;
            PagoMinimo = 0.0;
            Facturas = new List<Factura>();
            NombreCliente = "";

            Resultado = "";***/
            Initialize();
        }

        public EdetResponse(XmlDocument xml)
        {
            //Respuesta al pago
            //utils.Log(LOG_TAG, "EdetResponse[EdetResponse] Respuesta al pago: XML build -->");
            Initialize();
            try
            {
                //Si el primer nodo recibido es <?xml version="1.0" encoding="UTF-8"?>, pasamos al siguiente.
                /*XmlNode node1 = xml.FirstChild;
                utils.Log(LOG_TAG, "EdetResponse[EdetResponse] First Node Name: " + node1.Name);
                if (node1 != null && node1.Name.Equals("xml"))
                {
                    node1 = node1.NextSibling;
                }*/

                XmlNode noderesultado = xml.SelectSingleNode("//resultado");
                //utils.Log(LOG_TAG, "EdetResponse[EdetResponse] noderesultado -->" + noderesultado.InnerText);
                Resultado = noderesultado.InnerText;

            }
            catch (Exception)
            {
                //utils.Log(LOG_TAG, "EdetResponse[EdetResponse] - ERROR: Generic Exception: " + e.ToString());
            }
            //utils.Log(LOG_TAG, "EdetResponse[EdetResponse] <--");
        }

        public EdetResponse(XmlDocument xml, string currency)
        {
            //respuesta a la consulta
            //utils.Log(LOG_TAG, "EdetResponse[EdetResponse] respuesta a la consulta: XML build -->");
            try
            {
                //Si el primer nodo recibido es <?xml version="1.0" encoding="UTF-8"?>, pasamos al siguiente.
                /*XmlNode node1 = xml.FirstChild;
                utils.Log(LOG_TAG, "EdetResponse[EdetResponse] First Node Name: " + node1.Name);
                if (node1 != null && node1.Name.Equals("xml"))
                {
                    node1 = node1.NextSibling;
                }*/
                CultureInfo provider = CultureInfo.CurrentCulture;

                XmlNode nodeNombreCliente = xml.SelectSingleNode("//nombrecliente");
                XmlNode nodeDireccionSuministro = xml.SelectSingleNode("//direccionSuministro");
                XmlNodeList nodeDatosFactura = xml.SelectNodes("//datosFactura"); //todos los nodos datosFactura de este documento. Cada uno tiene los detalles por factura.

                //inicializa
                /***DireccionSuministro = ""; // a veces no llega
                TotalPagar = 0.0;
                PagoMinimo = 0.0;
                Facturas = new List<Factura>();
                NombreCliente = ""; // a veces no llega***/
                Initialize();

                DateTime auxFechaEmision = new DateTime();
                DateTime auxFechaVencimiento = new DateTime();

                if (nodeNombreCliente != null && nodeNombreCliente.Name.Equals("nombrecliente"))
                {
                    //utils.Log(LOG_TAG, "EdetResponse[EdetResponse] - Info: nombreCliente = " + nodeNombreCliente.InnerText);
                    NombreCliente = nodeNombreCliente.InnerText;
                }

                if (nodeDireccionSuministro != null && nodeDireccionSuministro.Name.Equals("direccionSuministro"))
                {
                    //utils.Log(LOG_TAG, "EdetResponse[EdetResponse] - Info: direccionSuministro = " + nodeDireccionSuministro.InnerText);
                    DireccionSuministro = nodeDireccionSuministro.InnerText;
                }

                foreach (XmlNode nodeFactura in nodeDatosFactura)
                {
                    if (nodeFactura.HasChildNodes) //tenemos nodos de información de factura.
                    {
                        Factura fac = new Factura();
                        XmlNodeList nodeDetallesFactura = nodeFactura.ChildNodes;
                        foreach (XmlNode nodeDetalle in nodeDetallesFactura)
                        {
                            if (nodeDetalle != null && nodeDetalle.Name.Equals("codigoIdentificador"))
                            {
                                fac.CodigoIdentificador = nodeDetalle.InnerText;
                                //utils.Log(LOG_TAG, "fac.CodigoIdentificador: " + fac.CodigoIdentificador);
                            }
                            else if (nodeDetalle != null && nodeDetalle.Name.Equals("detalle"))
                            {
                                fac.Detalle = nodeDetalle.InnerText;
                                //utils.Log(LOG_TAG, "fac.Detalle: " + fac.Detalle);
                            }
                            else if (nodeDetalle != null && nodeDetalle.Name.Equals("fechaEmision"))
                            {
                                //nodeDetalle.InnerText.Replace('T', ' ');
                                //fac.FechaEmision = DateTime.ParseExact(nodeDetalle.InnerText, "yyyy-MM-ddTHH:mm:ss", provider);
                                //fac.FechaEmisionStr = fac.FechaEmision.Day + "/" + fac.FechaEmision.Month + "/" + fac.FechaEmision.Year;
                                //utils.Log(LOG_TAG, "fac.FechaEmision: " + fac.FechaEmision + "; fac.FechaEmisionStr: " + fac.FechaEmisionStr);
                                fac.FechaEmision = nodeDetalle.InnerText;
                                auxFechaEmision = DateTime.ParseExact(nodeDetalle.InnerText, "yyyy-MM-ddTHH:mm:ss", provider);
                                fac.FechaEmisionStr = auxFechaEmision.Day + "/" + auxFechaEmision.Month + "/" + auxFechaEmision.Year;
                                //utils.Log(LOG_TAG, "fac.FechaEmision: " + fac.FechaEmision + "; auxFechaEmision: " + auxFechaEmision + "; fac.FechaEmisionStr: " + fac.FechaEmisionStr);
                            }
                            else if (nodeDetalle != null && nodeDetalle.Name.Equals("fechaVencimiento"))
                            {
                                //nodeDetalle.InnerText.Replace('T', ' ');
                                //fac.FechaVencimiento = DateTime.ParseExact(nodeDetalle.InnerText, "yyyy-MM-ddTHH:mm:ss", provider);
                                //fac.FechaVencimientoStr = fac.FechaVencimiento.Day + "/" + fac.FechaVencimiento.Month + "/" + fac.FechaVencimiento.Year;
                                //utils.Log(LOG_TAG, "fac.FechaVencimiento: " + fac.FechaVencimiento + "; fac.FechaVencimientoStr: " + fac.FechaVencimientoStr);
                                fac.FechaVencimiento = nodeDetalle.InnerText;
                                auxFechaVencimiento = DateTime.ParseExact(nodeDetalle.InnerText, "yyyy-MM-ddTHH:mm:ss", provider);
                                fac.FechaVencimientoStr = auxFechaVencimiento.Day + "/" + auxFechaVencimiento.Month + "/" + auxFechaVencimiento.Year;
                                //utils.Log(LOG_TAG, "fac.FechaVencimiento: " + fac.FechaVencimiento + "; auxFechaVencimiento: " + auxFechaVencimiento + "; fac.FechaVencimientoStr: " + fac.FechaVencimientoStr);
                            }
                            else if (nodeDetalle != null && nodeDetalle.Name.Equals("numeroFactura"))
                            {
                                long.TryParse(nodeDetalle.InnerText, out long var);
                                fac.NumeroFactura = var;
                                //utils.Log(LOG_TAG, "fac.NumeroFactura: " + fac.NumeroFactura);
                            }
                            else if (nodeDetalle != null && nodeDetalle.Name.Equals("ordenCorrelativo"))
                            {
                                int.TryParse(nodeDetalle.InnerText, out int var);
                                fac.OrdenCorrelativo = var;
                                //utils.Log(LOG_TAG, "fac.OrdenCorrelativo: " + fac.OrdenCorrelativo);
                            }
                            else if (nodeDetalle != null && nodeDetalle.Name.Equals("pagoMinimo"))
                            {
                                /*
                                string aux = nodeDetalle.InnerText;
                                utils.Log(LOG_TAG, "fac.PagoMinimo: aux:" + aux);
                                if (aux.Contains(".") || aux.Contains(","))
                                {
                                    int k = 0;
                                    if (aux.Contains(".")) k = aux.IndexOf('.');
                                    else k = aux.IndexOf(',');
                                    if (k+1==aux.Length) aux += "00";       //, sin decimales
                                    else if (k+2 == aux.Length) aux += "0"; //, con 1 decimal
                                    else if (k + 3 == aux.Length) aux += ""; //, con 2 decimales
                                } else aux += ".00";
                                */
                                //double.TryParse(nodeDetalle.InnerText, out double var);
                                //fac.PagoMinimo = (Int64)var;

                                ////////////////////double.TryParse(KALGeneric.VerifyAmountFormat(nodeDetalle.InnerText), out double var);
                                ////////////////////fac.PagoMinimo = var;
                                //utils.Log(LOG_TAG, "fac.PagoMinimo: " + fac.PagoMinimo);
                            }
                            else if (nodeDetalle != null && nodeDetalle.Name.Equals("totalFactura"))
                            {
                                /*
                                string aux = nodeDetalle.InnerText;
                                utils.Log(LOG_TAG, "fac.totalFactura: aux:" + aux);
                                if (aux.Contains(".") || aux.Contains(","))
                                {
                                    int k = 0;
                                    if (aux.Contains(".")) k = aux.IndexOf('.');
                                    else k = aux.IndexOf(',');
                                    if (k + 1 == aux.Length) aux += "00";       //, sin decimales
                                    else if (k + 2 == aux.Length) aux += "0"; //, con 1 decimal
                                    else if (k + 3 == aux.Length) aux += ""; //, con 2 decimales
                                }
                                else aux += ".00";
                                */
                                //double.TryParse(nodeDetalle.InnerText, out double var);
                                //fac.TotalFactura = (Int64)var;
                                //////////////////////////double.TryParse(KALGeneric.VerifyAmountFormat(nodeDetalle.InnerText), out double var);
                                //////////////////////////fac.TotalFactura = var;
                                //utils.Log(LOG_TAG, "fac.TotalFactura: " + fac.TotalFactura);
                            }
                            else if (nodeDetalle != null && nodeDetalle.Name.Equals("totalPagar"))
                            {
                                /*
                                string aux = nodeDetalle.InnerText;
                                utils.Log(LOG_TAG, "fac.totalPagar: aux:" + aux);
                                if (aux.Contains(".") || aux.Contains(","))
                                {
                                    int k = 0;
                                    if (aux.Contains(".")) k = aux.IndexOf('.');
                                    else k = aux.IndexOf(',');
                                    if (k + 1 == aux.Length) aux += "00";       //, sin decimales
                                    else if (k + 2 == aux.Length) aux += "0"; //, con 1 decimal
                                    else if (k + 3 == aux.Length) aux += ""; //, con 2 decimales
                                }
                                else aux += ".00";
                                */
                                //double.TryParse(nodeDetalle.InnerText, out double var);
                                //fac.TotalPagar = (Int64)var;
                                //////////////////////////////////double.TryParse(KALGeneric.VerifyAmountFormat(nodeDetalle.InnerText), out double var);
                                //////////////////////////////////fac.TotalPagar = var;
                                //utils.Log(LOG_TAG, "fac.TotalPagar: " + fac.TotalPagar);
                            }
                        }
                        Facturas.Add(fac);
                        TotalPagar += fac.TotalPagar;
                        /***
                         * cambiado julio 2020. El PagoMinimo es el valor que venga en la factura más antigua
                         * "solo puede hacer un pago parcial de la factura más antigua"
                         * lo tomamos una vez ordenadas
                         * 
                        //if (fac.FechaVencimiento < DateTime.Now)
                        if (auxFechaVencimiento < DateTime.Now)
                        {
                            PagoMinimo += fac.TotalPagar;
                        }
                        else
                        {
                            PagoMinimo += fac.PagoMinimo;
                        }
                        utils.Log(LOG_TAG, "TotalPagar: " + TotalPagar + "; PagoMinimo: " + PagoMinimo);
                        **/
                        Facturas = Facturas.OrderBy(x => x.OrdenCorrelativo).ToList();

                        PagoMinimo = Facturas[0].PagoMinimo;
                        //utils.Log(LOG_TAG, "TotalPagar: " + TotalPagar + "; PagoMinimo: " + PagoMinimo);
                    }
                }

            }
            catch (Exception)
            {
                //utils.Log(LOG_TAG, "EdetResponse[EdetResponse] - ERROR: Generic Exception: " + e.ToString());
            }
            //utils.Log(LOG_TAG, "EdetResponse[EdetResponse] <--");
        }

        private void Initialize()
        {
            DireccionSuministro = ""; // a veces no llega
            TotalPagar = 0.0;
            PagoMinimo = 0.0;
            Facturas = new List<Factura>();
            NombreCliente = ""; // a veces no llega

            Resultado = "";
        }

    }
}