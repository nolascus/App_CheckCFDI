using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Ionic.Zip;
using System.IO;
using System.ServiceModel;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Text;
using App_CheckCFDI_AspNet.ServicioSATCFDI;

namespace App_CheckCFDI_AspNet
{
    public partial class Default : System.Web.UI.Page
    {
        static List<XMLRow> ArchivosXML = new List<XMLRow>();
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnProcesar_Click(object sender, EventArgs e)
        {
            string path = HttpRuntime.AppDomainAppPath;
            if (XMLZipFile.HasFile)
            {
                try
                {
                    string myFileNameLocal = path + XMLZipFile.FileName;
                    XMLZipFile.SaveAs(path + XMLZipFile.FileName);
                    if (File.Exists(myFileNameLocal) && myFileNameLocal.Contains(".zip"))
                    {
                        lblSalida.Text = lblSalida.Text + "Procesando Archivo :"+myFileNameLocal+"\n";
                        ArchivosXML.Clear();
                        using (ZipFile zip = ZipFile.Read(myFileNameLocal))
                        {
                            int cont = 0;
                            foreach (ZipEntry entrada in zip)
                            {
                                XMLRow nuevoElemento = new XMLRow();
                                Comprobante comprobante = new Comprobante();
                                cont++;
                                string Xmlstring;
                                MemoryStream memory = new MemoryStream();
                                entrada.Extract(memory);
                                Xmlstring = Encoding.UTF8.GetString(memory.ToArray());
                                Xmlstring = Xmlstring.Substring(1);
                                nuevoElemento.XML_Nombre_Archivo = Path.GetFileName(entrada.FileName);
                                var serializer = new XmlSerializer(typeof(Comprobante));
                                try
                                {
                                    using (TextReader lector = new StringReader(Xmlstring))
                                    {
                                        comprobante = (Comprobante)serializer.Deserialize(lector);
                                    }
                                    nuevoElemento.XML_RFC_Emisor = comprobante.Emisor.rfc;
                                    nuevoElemento.XML_RFC_Receptor = comprobante.Receptor.rfc;
                                    nuevoElemento.XML_Total = comprobante.total;
                                    nuevoElemento.XML_Esquema_CFDI = "Esquema Valido";
                                }
                                catch (Exception ex)
                                {
                                    nuevoElemento.XML_Esquema_CFDI = "Esquema Invalido";
                                    nuevoElemento.XML_RFC_Emisor = "";
                                    nuevoElemento.XML_RFC_Receptor = "";
                                    nuevoElemento.XML_Status = "INVALIDO!!!";
                                    nuevoElemento.XML_Total = 0.00m;
                                    nuevoElemento.XML_UUID = "";
                                    lblSalida.Text = lblSalida.Text + "Error :" + ex.Message;
                                }
                                try
                                {
                                    var xdoc = XDocument.Parse(Xmlstring);
                                    var timbreFiscal = (from item in xdoc.Descendants()
                                                        where item.Name.LocalName == "TimbreFiscalDigital"
                                                        select item).First();
                                    TimbreFiscalDigital timbreXMLComplemento = new TimbreFiscalDigital();
                                    timbreXMLComplemento.FechaTimbrado = Convert.ToDateTime(timbreFiscal.Attribute("FechaTimbrado").Value);
                                    timbreXMLComplemento.UUID = timbreFiscal.Attribute("UUID").Value;
                                    timbreXMLComplemento.noCertificadoSAT = timbreFiscal.Attribute("noCertificadoSAT").Value;
                                    timbreXMLComplemento.selloCFD = timbreFiscal.Attribute("selloCFD").Value;
                                    timbreXMLComplemento.selloSAT = timbreFiscal.Attribute("selloSAT").Value;
                                    timbreXMLComplemento.version = timbreFiscal.Attribute("version").Value;
                                    nuevoElemento.XML_UUID = timbreXMLComplemento.UUID.ToLower();
                                    nuevoElemento.XML_Complemento_TFD = "Complemento Valido";
                                }
                                catch
                                {
                                    nuevoElemento.XML_Complemento_TFD = "Complemento Invalido";
                                }
                                memory.Dispose();
                                ArchivosXML.Add(nuevoElemento);
                            }
                            lblSalida.Text = lblSalida.Text + "Numero de XML en archivo ZIP:"+cont+"\n";
                        }
                    }
                    else
                    {
                        lblSalida.Text = lblSalida.Text + "El archivo no existe; o no es un archivo zip";
                    }
                }
                catch (Exception ex)
                {
                    lblSalida.Text = "Error : " + ex.Message;
                }
                procesar();
            }
        }

        private void procesar()
        {
            int cont = 0;
            try
            {
                ConsultaCFDIServiceClient consultar = new ConsultaCFDIServiceClient();
                consultar.Open();
                foreach (XMLRow row in ArchivosXML)
                {
                    cont++;
                    if (!(row.XML_Esquema_CFDI == "Esquema Invalido" || row.XML_Complemento_TFD == "Complemento Invalido"))
                    {
                        Acuse acuse = new Acuse();
                        if (consultar.State == CommunicationState.Opened)
                        {
                            acuse = consultar.Consulta("?re=" + row.XML_RFC_Emisor.Replace("&", "&amp;") + "&rr=" + row.XML_RFC_Receptor.Replace("&", "&amp;") + "&tt=" + row.XML_Total + "&id=" + row.XML_UUID);
                            row.XML_Status = acuse.CodigoEstatus;
                        }
                        if (consultar.State == CommunicationState.Closed)
                        {
                            row.XML_Status = "Comunicacion cerrada, error";
                        }
                        if (consultar.State == CommunicationState.Faulted)
                        {
                            row.XML_Status = "Comunicacion fallida, error. Reintente";
                        }
                        if (consultar.State == CommunicationState.Closed)
                        {
                            row.XML_Status = "Comunicacion cerrada, error. Reintente";
                        }
                    }
                }
                ActualizaGrid();

            }
            catch (Exception ex)
            {

            }

        }

        private void ActualizaGrid()
        {

            ConsultaCFDIServiceClient consultar = new ConsultaCFDIServiceClient();
            var lstXML = from xml in ArchivosXML
                         select new
                         {
                             xml.XML_Nombre_Archivo,
                             xml.XML_Esquema_CFDI,
                             xml.XML_Complemento_TFD,
                             XML_RFC_Emisor = xml.XML_RFC_Emisor.ToString(),
                             xml.XML_RFC_Receptor,
                             xml.XML_UUID,
                             xml.XML_Status,
                             xml.XML_Total
                         };

            GridView1.DataSource = lstXML.ToList();
            GridView1.DataBind();


            //GridView1.Columns["XML_Total"].DefaultCellStyle.Alignment 
            //dataGridView1.Refresh();

        }
    }
    class XMLRow
    {
        public string XML_Nombre_Archivo { get; set; }
        public string XML_Esquema_CFDI { get; set; }
        public string XML_Complemento_TFD { get; set; }
        public string XML_RFC_Emisor { get; set; }
        public string XML_RFC_Receptor { get; set; }
        public string XML_UUID { get; set; }
        public decimal XML_Total { get; set; }
        public string XML_Status { get; set; }

        public XMLRow()
        {
        }
    }
}