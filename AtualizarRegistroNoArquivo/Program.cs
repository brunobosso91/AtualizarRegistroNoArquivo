using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace AtualizarRegistroWes
{
    class Program
    {
        static void Main(string[] args)
        {

            var visoesWesAntigo = ObterVisoesWesAntigo();

            var startFolder = @"C:\Git\AG\AG_DotNet\WEBAPP\Padrão\Artifacts\Pages\";

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(startFolder);

            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.xml", System.IO.SearchOption.AllDirectories);

            string searchTerm = "mode=\"7\"";
            string wordsToMatch = "mode=\"7\"";


            char[] delims = new[] { '\r', '\n' };

            foreach (var item in visoesWesAntigo)
            {
                var queryMatchingFiles =
                from file in fileList
                where file.Name.Contains(item.Nome.Substring(0, item.Nome.IndexOf('.')))
                let fileText = GetFileText(file.FullName)
                where fileText.Contains(item.WidGet)
                select file.FullName;

                foreach (var arquivo in queryMatchingFiles)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(arquivo.ToString());

                    XmlNodeList LocalCell = doc.GetElementsByTagName("PagePortable");

                    foreach (XmlNode LocalCell_Children in LocalCell)
                    {
                        XmlElement MXPWR = (XmlElement)LocalCell_Children;
                        XmlNodeList MXPWR_List = MXPWR.GetElementsByTagName("XmlAttributes");
                        for (int i = 0; i < MXPWR_List.Count; i++)
                        {
                            if (MXPWR_List[i].InnerText.Contains(item.WidGet))
                                MXPWR_List[i].InnerText = MXPWR_List[i].InnerText.Replace("<CanInsert>false</CanInsert>", "<CanInsert>true</CanInsert>") ;
                        }
                    }



                    doc.Save(arquivo + "2");                    

                }
            }
        }


        public static List<ArquivoOrigem> ObterVisoesWesAntigo()
        {
            string startFolder = @"C:\Git\AG\AG_DotNet\#WEB\Visões\";

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(startFolder);

            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.bvs", System.IO.SearchOption.AllDirectories);

            string searchTerm = "mode=\"7\"";
            char[] delims = new[] { '\r', '\n' };

            var queryMatchingFiles =
                from file in fileList
                where file.Extension == ".bvs"
                let fileText = GetFileText(file.FullName)
                where fileText.Contains(searchTerm)
                select file;
                        
            List<ArquivoOrigem> arquivoOrigem = new List<ArquivoOrigem>();

            foreach (var file in queryMatchingFiles)
            {
                StreamReader sr = new StreamReader(file.FullName);
                var fileSplited = sr.ReadToEnd().Split(delims).Select(z => z).Where(x => x.Contains(searchTerm));


                foreach (var line in fileSplited)
                {
                    var widGet = line.Substring(line.IndexOf("mode=\"7\" vision=\"") + 17);
                    widGet = widGet.Substring(0, widGet.IndexOf("\""));


                    arquivoOrigem.Add(new ArquivoOrigem
                    {
                        Caminho = file.FullName,
                        Nome = file.Name,
                        LinhaEncontrada = line,
                        WidGet = widGet
                    });
                }
            }

            string jsonResult = JsonConvert.SerializeObject(arquivoOrigem);

            using (var tw = new StreamWriter(@"C:\Git\AG\WesConverter.Json", true))
            {
                tw.WriteLine(jsonResult.ToString());
                tw.Close();
            }

            return arquivoOrigem;
        }


        // Read the contents of the file.  
        static string GetFileText(string name)
        {
            string fileContents = String.Empty;

            // If the file has been deleted since we took
            // the snapshot, ignore it and return the empty string.  
            if (System.IO.File.Exists(name))
            {
                fileContents = System.IO.File.ReadAllText(name);
            }
            return fileContents;
        }
    }


    public class ArquivoOrigem
    {
        public string Caminho { get; set; }
        public string Nome { get; set; }
        public string LinhaEncontrada { get; set; }
        public string WidGet { get; set; }
    }
}
