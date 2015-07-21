using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EjemploTimbrado
{
    public partial class FrmPrincipal : Form
    {
        public FrmPrincipal()
        {
            InitializeComponent();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Archivo XML|*.xml|Todos los archivos|*.*";
            fd.Multiselect = false;

            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtXML.Text = fd.FileName;
        }

        private string ObtenerConsulta()
        {
            System.IO.StreamReader lector = new System.IO.StreamReader(txtXML.Text);
            var xml = lector.ReadToEnd();
            lector.Close();

            /* Crear el JSON que se envía al servicio web */

            // El servidor espera los datos en formato JSON, por ello en este ejemplo
            // estaremos utilizando la popular librería Newtonsoft.Json.dll, también
            // conocida como Json.NET

            var jObj = new Newtonsoft.Json.Linq.JObject(
                // Método, este valor identifica el servicio solicitado
                new Newtonsoft.Json.Linq.JProperty("method", "cfd2cfdi"),
                new Newtonsoft.Json.Linq.JProperty("xml_version", "3.2"),
                new Newtonsoft.Json.Linq.JProperty("cuenta", "demo"),
                new Newtonsoft.Json.Linq.JProperty("user", "administrador"),
                new Newtonsoft.Json.Linq.JProperty("password", "Administr4dor"),
                new Newtonsoft.Json.Linq.JProperty("include_stamps", true),
                new Newtonsoft.Json.Linq.JProperty("xml_data", xml)
            );

            /* Creación de la consulta */

            // Aquí es importante utilizar HttpUtility.UrlEncode para una correcta 
            // interpretación de los datos del lado del servidor.
            string consulta = "q=" + System.Web.HttpUtility.UrlEncode(jObj.ToString());
            // - NOTA: Asegúrese de incluir una referencia al ensamblado System.Web 
            // para poder utilizar HttpUtility.UrlEncode 
            // - NOTA 2: El ensamblado System.Web no forma parte del 'Client Profile'
            // por ello asegúrece de que su aplicación está configurada para utilizar,
            // por ejemplo, '.NET Framework 4' y no '.NET Framework 4 Client Profile'.
            // - NOTA 3: Lo anterior puede configurarse en la ventana:
            // Propiedades del Proyecto / Pestaña 'Application' / Combo 'Target framework' 

            return consulta;
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            /* Crear la consulta */

            string consulta = ObtenerConsulta();

            /* Extraer el contenido de la consulta */

            // Aquí es importante prestar atención a la codificación. 
            // Los datos en el servidor se esperan en UTF-8
            byte[] content = Encoding.UTF8.GetBytes(consulta);

            /* Crear la petición */

            // En producción debe indicarse el URL apropiado según el manual de integración
            string url = "https://facturacion.itimbre.com/service.php";


            System.Net.HttpWebRequest peticion = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

            peticion.Method = "POST";
            peticion.ContentType = "application/x-www-form-urlencoded";
            peticion.ContentLength = content.Length;

            /* Enviar la petición */
            System.IO.Stream requestStream = peticion.GetRequestStream();
            requestStream.Write(content, 0, content.Length);
            requestStream.Close();

            /* Recibir la respuesta del servidor */

            var resp = peticion.GetResponse();
            string respuesta = new System.IO.StreamReader(resp.GetResponseStream()).ReadToEnd();
            
            /* Trabajar con la respuesta del servidor */

            // El servidor devuelve una respuesta en formato JSON. Para comodidad
            // se puede utilizar una librería popular para su procesamiento, en 
            // este ejemplo estaremos utilizando Newtonsoft.Json.dll también
            // conocida como Json.NET

            var respJson = Newtonsoft.Json.Linq.JObject.Parse(respuesta);
            string text = "";

            if (respJson["result"].Value<int>("retcode") == 1)
            {
                // Folio Fiscal del CFDI
                text += string.Format("UUID:\r\n{0}\r\n\r\n", respJson["result"]["UUID"].ToString());
                // Acuse del SAT
                text += string.Format("ACUSE:\r\n{0}\r\n\r\n", respJson["result"]["acuse"].ToString());
                // CFDI Timbrado
                text += string.Format("CFDI:\r\n{0}\r\n\r\n", respJson["result"]["data"].ToString());
            }
            else
            {
                text = string.Format("Ha ocurrido un error:\r\nCódigo: {0}\r\nMensaje: {1}",
                    respJson["result"]["retcode"].ToString(),
                    respJson["result"]["error"].ToString()
                );

                MessageBox.Show(text, "Ejemplo de Timbrado - iTimbre", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            

           



            txtResultado.Text = text;
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {

        }
    }
}