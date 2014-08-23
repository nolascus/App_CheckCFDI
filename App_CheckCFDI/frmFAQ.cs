using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace App_CheckCFDI
{
    public partial class frmFAQ : Form
    {
        string htmlstring = 
"<p><strong>P : </strong>&iquest;Cu&aacute;nto cuesta el programa?</p> " +
"<p><strong>R : </strong>Ni un peso. Sin embargo si entras a <a href='http://www.nolascus.com/'>www.nolascus.com</a> y ah&iacute; veras un bot&oacute;n para comprarme un caf&eacute;; (El caf&eacute; regularmente se transforma en lineas c&oacute;digo :D ).</p> " +
"<p><strong>P: </strong>&iquest;Cu&aacute;les son los requerimientos m&iacute;nimos para ejecutar el programa?</p>  " +
"<p><strong>R: </strong>Microsoft DotNet Framework 4.0, Sistema Operativo : Windows Vista, Windows 7, Windows 8, etc. y desde luego conexi&oacute;n a Internet.</p>  " +
"<p><strong>P : </strong>&iquest;C&oacute;mo se realiza la validaci&oacute;n y qu&eacute; criterios se utilizan?</p>  " +
"<p>El programa utiliza la documentaci&oacute;n proporcionada en el archivo PDF <a href='ftp://ftp2.sat.gob.mx/asistencia_servicio_ftp/publicaciones/cfdi/WS_ConsultaCFDI.pdf'>ftp://ftp2.sat.gob.mx/asistencia_servicio_ftp/publicaciones/cfdi/WS_ConsultaCFDI.pdf</a> el cual contiene la informaci&oacute;n acerca del Web Service para comprobar los CFDI emitidos.</p> " +
"<p><strong>P : </strong>&iquest;Cu&aacute;ntos CFDI puedo validar?</p> " +
"<p><strong>R : </strong>La documentaci&oacute;n (WS_ConsultaCFDI.pdf) en la secci&oacute;n 5 Capacidad de Respuesta indica &ldquo;El servicio de Consulta de CFDI&acute;s tiene la capacidad de atender hasta 2 millones de consultas por hr., debido a que estas consultas acceden las Bases de Datos transaccionales del SAT se solicita&nbsp; no aumentar la cantidad de consultas por hora para evitar impactos en la respuesta del servicio.&rdquo;</p> " +
"<p><strong>P :</strong> &iquest;Tengo mis XML guardados en un archivo zip, el programa puede comprobar estos archivos?</p> " +
"<p><strong>R : </strong>Buena pregunta, lamentablemente, la respuesta es no, en cuanto tenga un tiempo libre tratare de agregar esta funcionalidad.</p> " +
"<p><strong>P:</strong> &iquest;Si encuentro un error que hago?</p> " +
"<p><strong>R: </strong>Puedes reportarlo a <a href='mailto:albino.nolasco@gmail.com'>albino.nolasco@gmail.com</a> , env&iacute;ame un pantallazo, y una descripci&oacute;n detallada de lo que ocurri&oacute;, para tratar de eliminar dicho error.</p> " +
"<p><strong>P:</strong> &iquest;Necesito agregar nuevas funcionalidades al programa?</p> " +
"<p><strong>R:</strong> Cont&aacute;ctame, m&aacute;ndame un correo electr&oacute;nico con las nuevas funcionalidades y esperemos que tengamos tiempo. :D</p> ";

        public frmFAQ()
        {
            InitializeComponent();
            webBrowser1.DocumentText = htmlstring;
        }
    }
}
