using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace POC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DataTable dtDisplay = new DataTable();
            dtDisplay.Columns.Add("Nombre");
            dtDisplay.Columns.Add("Especialidad");
            dtDisplay.Columns.Add("Tipo");
            dtDisplay.Columns.Add("Codigo");
            dtDisplay.Columns.Add("Estado");
            dtDisplay.Columns.Add("Trabajo");
            dtDisplay.Columns.Add("Telefono");
            dtDisplay.Columns.Add("Email");
            dtDisplay.Columns.Add("Detalles");

            for (int i = 1; i < 3000; i++)
            {
                webScrappingMedicos(i, ref dtDisplay);
            }

            dtDisplay.TableName = "MedicosCR";

            // Obtiene la ruta de la carpeta especial "Descargas"
            string rutaDescargas = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";

            dtDisplay.WriteXml($"{rutaDescargas}\\ListadoMedicoCR{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}.xml");
        }

        public static string DecodificarHTMLEntities(string input)
        {
            // Utiliza WebUtility.HtmlDecode para decodificar los caracteres HTML
            string textoDecodificado = System.Net.WebUtility.HtmlDecode(input);

            return textoDecodificado;
        }

        public static void webScrappingMedicos(int page, ref DataTable dtDisplay)
        {           
            string content = execute_request(page);

            var articles = content.Split(new string[] { "article" }, StringSplitOptions.None);

            foreach (var currentArticle in articles)
            {
                if (!currentArticle.Contains("doctor-premium"))
                {
                    continue;
                }

                string nombre = string.Empty;
                string especialidad = string.Empty;
                string tipo = string.Empty;
                string codigo = string.Empty;
                string detalles = string.Empty;
                string estado = string.Empty;
                string trabajo = string.Empty;
                string telefono = string.Empty;
                string email = string.Empty;

                try
                {
                    nombre = currentArticle.Split(new string[] { "title=" }, StringSplitOptions.None)[1].Split(new string[] { "class" }, StringSplitOptions.None)[0].ToString().Replace("\"", "").Replace("\\", "").Trim();
                }
                catch { }

                try
                {
                    especialidad = currentArticle.Split(new string[] { "specialty" }, StringSplitOptions.None)[2].Split('<')[0].Replace("\\", "").Replace(">", "").Replace("\"", "");
                }
                catch { }

                try
                {
                    tipo = currentArticle.Split(new string[] { "professional-license-type" }, StringSplitOptions.None)[1].Split(new string[] { "span" }, StringSplitOptions.None)[0].Replace("\\", "").Replace("<", "").Replace(">", "").Replace("\"", "").Replace("/", "");
                }
                catch { }

                try
                {
                    codigo = currentArticle.Split(new string[] { "license-number" }, StringSplitOptions.None)[1].Split(new string[] { "span" }, StringSplitOptions.None)[0].Replace("\\", "").Replace("<", "").Replace(">", "").Replace("\"", "").Replace("/", "");
                }
                catch { }

                try
                {
                    estado = currentArticle.Split(new string[] { "icense-status" }, StringSplitOptions.None)[1].Split('>')[1].ToString().Split('<')[0].Trim();
                }
                catch { }

                try
                {
                    trabajo = currentArticle.Split(new string[] { "clinic-name" }, StringSplitOptions.None)[1].Split('>')[1].Split('<')[0].ToString().Trim();
                }
                catch { }

                try
                {
                    trabajo = trabajo + " | " + currentArticle.Split(new string[] { "clinic-location" }, StringSplitOptions.None)[2].Split('>')[1].Split('<')[0].Trim();
                }
                catch { }

                try
                {
                    telefono = currentArticle.Split(new string[] { "phone" }, StringSplitOptions.None)[3].Split('>')[2].Split('<')[0].Trim();
                }
                catch { }

                try
                {
                    email = currentArticle.Split(new string[] { "email" }, StringSplitOptions.None)[2].Split(new string[] { "mailto:" }, StringSplitOptions.None)[1].Split('>')[0].Replace("\"", "").Replace("\\", "");
                }
                catch { }

                try
                {
                    detalles = currentArticle;
                }
                catch { }
 
                dtDisplay.Rows.Add(
                    DecodificarHTMLEntities(nombre.ToUpper()),
                    DecodificarHTMLEntities(especialidad),
                    DecodificarHTMLEntities(tipo),
                    codigo,
                    estado,
                    DecodificarHTMLEntities(trabajo), 
                    telefono,
                    email,
                    detalles);
            }
        }


        public static string RemoveHtml(string input)
        {
            // Utiliza una expresión regular para eliminar las etiquetas HTML
            string patronHTML = "<.*?>";
            string textoSinHTML = Regex.Replace(input, patronHTML, string.Empty);

            return textoSinHTML;
        }

        //public static string RemoveHtml(string htmlString)
        //{
        //    // Reemplazar etiquetas HTML con string vacío
        //    string result = Regex.Replace(htmlString, "<.*?>", string.Empty);

        //    // Reemplazar entidades HTML con sus caracteres equivalentes
        //    result = WebUtility.HtmlDecode(result);

        //    // Eliminar espacios en blanco innecesarios
        //    result = Regex.Replace(result, "\\s+", " ");

        //    return result;
        //}

        public static string execute_request(int page)
        {
            Console.WriteLine("Consultando..." + page);

            // URL de destino
            string url = $"https://medicoscr.hulilabs.com/es/api/search/keywords?page={page}&l_id=1-24010&ro=6492";

            // Crear una solicitud web
            WebRequest request = WebRequest.Create(url);

            // Obtener la respuesta
            WebResponse response = request.GetResponse();

            // Leer el contenido de la respuesta
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string contenido = reader.ReadToEnd();

                // Mostrar el contenido
                return contenido;
            }
             
        }


    }
}
