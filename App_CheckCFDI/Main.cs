using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;
using System.Web;
using System.Xml.Linq;
using App_CheckCFDI.ServicioSATCFDI;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Threading;
using System.Diagnostics;

namespace App_CheckCFDI
{
    public partial class Main : Form
    {
        string AppPath = Application.StartupPath;

        List<XMLRow> ArchivosXML = new List<XMLRow>();
        public Main()
        {
            InitializeComponent();
            

        }

        void IniciaAplicacion()
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\App_CheckCFDI");
            if (key != null)
            {
                lblDirectorio.Text = key.GetValue("Path","").ToString();
                dlgEstableceDirectorio.SelectedPath = lblDirectorio.Text;
            }
            else
            {
                lblDirectorio.Text = AppPath;
            }
            Lee_XML_En_Directorio(dlgEstableceDirectorio.SelectedPath);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\App_CheckCFDI");
            key.SetValue("Path", dlgEstableceDirectorio.SelectedPath);
        }

        void disable_UI()
        {
            this.Cursor = Cursors.WaitCursor;
            btnIniciar.Enabled = false;
            btnEstableceDirectorio.Enabled = false;
            dataGridView1.Enabled = false;
        }

        void enable_UI()
        {
            btnIniciar.Enabled = true;
            btnEstableceDirectorio.Enabled = true;
            dataGridView1.Enabled = true;
            this.Cursor = Cursors.Default;
        }


        private void btnEstableceDirectorio_Click(object sender, EventArgs e)
        {
            if (dlgEstableceDirectorio.ShowDialog()== System.Windows.Forms.DialogResult.OK)
            {
                disable_UI();
                lblDirectorio.Text = dlgEstableceDirectorio.SelectedPath;
                lblDirectorio.ToolTipText = dlgEstableceDirectorio.SelectedPath;
                Lee_XML_En_Directorio(dlgEstableceDirectorio.SelectedPath);
                enable_UI();
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            IniciaAplicacion();
        }

        void Lee_XML_En_Directorio(string path)
        {

            try
            {
                ArchivosXML.Clear();
                string[] filePaths = Directory.GetFiles(path, "*.xml");
                lblStatus.Text = "Leyendo directorio...";
                statusStrip1.Refresh();
                int cont = 0;
                foreach (string filename in filePaths)
                {
                    XMLRow nuevoElemento = new XMLRow();
                    StreamReader reader = new StreamReader(filename);
                    string xmlstring = reader.ReadToEnd();
                    Comprobante comprobante = new Comprobante();

                    nuevoElemento.XML_Nombre_Archivo = Path.GetFileName(filename);
                    try
                    {
                        var serializer = new XmlSerializer(typeof(Comprobante));
                        cont++;
                        using (TextReader lector = new StringReader(xmlstring))
                        {
                            lblStatus.Text = "Archivo " + cont.ToString() + " de " + filePaths.Count().ToString() + "; Serializando " + nuevoElemento.XML_Nombre_Archivo + " a Comprobante CFDI...";
                            statusStrip1.Refresh();
                            comprobante = (Comprobante)serializer.Deserialize(lector);
                        }
                        nuevoElemento.XML_RFC_Emisor = comprobante.Emisor.rfc;
                        nuevoElemento.XML_RFC_Receptor = comprobante.Receptor.rfc;
                        nuevoElemento.XML_Total = comprobante.total;
                        nuevoElemento.XML_Esquema_CFDI = "Esquema Valido";
                    }
                    catch
                    {
                        nuevoElemento.XML_Esquema_CFDI = "Esquema Invalido";
                        nuevoElemento.XML_RFC_Emisor = "";
                        nuevoElemento.XML_RFC_Receptor = "";
                        nuevoElemento.XML_Status = "INVALIDO!!!";
                        nuevoElemento.XML_Total = 0.00m;
                        nuevoElemento.XML_UUID = "";
                    }
                    try
                    {
                        var xdoc = XDocument.Parse(xmlstring);
                        var timbreFiscal = (from item in xdoc.Descendants()
                                            where item.Name.LocalName == "TimbreFiscalDigital"
                                            select item).First();
                        lblStatus.Text = "Archivo " + cont.ToString() + " de " + filePaths.Count().ToString() + "; Extrayendo Comprobante CFDI de " + nuevoElemento.XML_Nombre_Archivo + "...";
                        statusStrip1.Refresh();
                        TimbreFiscalDigital timbreXMLComplemento = new TimbreFiscalDigital();
                        timbreXMLComplemento.FechaTimbrado = Convert.ToDateTime(timbreFiscal.Attribute("FechaTimbrado").Value);
                        timbreXMLComplemento.UUID = timbreFiscal.Attribute("UUID").Value;
                        timbreXMLComplemento.noCertificadoSAT = timbreFiscal.Attribute("noCertificadoSAT").Value;
                        timbreXMLComplemento.selloCFD = timbreFiscal.Attribute("selloCFD").Value;
                        timbreXMLComplemento.selloSAT = timbreFiscal.Attribute("selloSAT").Value;
                        timbreXMLComplemento.version = timbreFiscal.Attribute("version").Value;
                        nuevoElemento.XML_UUID = timbreXMLComplemento.UUID.ToLower();
                        nuevoElemento.XML_Complemento_TFD = "Complemento Valido";
                        reader.Close();
                    }
                    catch
                    {
                        nuevoElemento.XML_Complemento_TFD = "Complemento Invalido";
                    }
                    ArchivosXML.Add(nuevoElemento);

                }
                ActualizaGrid();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error :"+ex.ToString());
            }
        }

        private void ActualizaGrid()
        {
            var lstXML = from xml in ArchivosXML
                select new
                {
                    xml.XML_Nombre_Archivo,
                    xml.XML_Esquema_CFDI,
                    xml.XML_Complemento_TFD,
                    XML_RFC_Emisor=xml.XML_RFC_Emisor.ToString(),
                    xml.XML_RFC_Receptor,
                    xml.XML_UUID,
                    xml.XML_Status,
                    xml.XML_Total
                };

            dataGridView1.DataSource = lstXML.ToList();
            
            dataGridView1.Columns["XML_Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Refresh();
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {

            disable_UI();
            
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
                        lblStatus.Text = "Archivo " + cont.ToString() + " de " + ArchivosXML.Count().ToString() + " Comprobando "+row.XML_Nombre_Archivo+" con UUID :" + row.XML_UUID + "...";
                        statusStrip1.Refresh();
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
            catch(Exception ex)
            {
                MessageBox.Show("Error:" + ex.Message);
            }
            lblStatus.Text = "";
            statusStrip1.Refresh();
            enable_UI();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void documentacionWebServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("ftp://ftp2.sat.gob.mx/asistencia_servicio_ftp/publicaciones/cfdi/WS_ConsultaCFDI.pdf");
        }

        private void acercaDelProgramaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Acerca nuevo = new Acerca();
            nuevo.ShowDialog();
        }

        private void preguntasFrecuentesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmFAQ nuevo = new frmFAQ();
            nuevo.ShowDialog();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
        }

    }

    class XMLRow
    {
        public string XML_Nombre_Archivo { get; set; }
        public string XML_Esquema_CFDI { get; set; }
        public string XML_Complemento_TFD { get; set; }
        public string XML_RFC_Emisor { get; set; }
        public string XML_RFC_Receptor { get; set; }
        public string XML_UUID{ get; set; }
        public decimal XML_Total { get; set; }
        public string XML_Status { get; set; }

        public XMLRow()
        {

        }
    }
}
