using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Humanizer;
using Newtonsoft.Json;

namespace Mdl2Tool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TemplarianItems items;
            using (TextReader tr = new StreamReader(@"Templarian.json"))
            {
                var json = tr.ReadToEnd();
                items = JsonConvert.DeserializeObject<TemplarianItems>(json);
            }

            var actualItems =
                items.Items.Where(x => !x.Keywords.Contains("duplicate") && x.Name != "name" && x.Name != "unknown")
                    .Distinct(new NameComparer())
                    .ToList();
            var list = actualItems.OrderBy(x => x.Name).ToList();
            foreach (var item in list)
            {
                item.Name = GetName(item.Name);
            }

            var sb = CreateCsFile(list);

            WriteToFile(@"..\..\..\Nuget\Content\Mdl2.cs", sb.ToString());

            sb = CreateXamlFile(list);

            WriteToFile(@"..\..\..\Nuget\Content\Mdl2.xaml", sb.ToString());
        }

        private static StringBuilder CreateXamlFile(List<TemplarianClass> list)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            sb.AppendLine("                    xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">");

            foreach (var item in list)
            {
                sb.AppendLine(string.Format("\t<x:String x:Key=\"{0}\">&#x{1};</x:String>", item.Name, item.Code));
            }

            sb.AppendLine("</ResourceDictionary>");

            return sb;
        }

        private static StringBuilder CreateCsFile(IEnumerable<TemplarianClass> list)
        {
            var sb = new StringBuilder();
            sb.AppendLine("namespace Mdl2Tool");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic class Mdl2");
            sb.AppendLine("\t{");

            foreach (var item in list)
            {
                sb.AppendLine(string.Format("\t\tpublic static string {0} => \"\\u{1}\";", item.Name, item.Code));
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");
            return sb;
        }

        private static string GetName(string name)
        {
            name = name.Replace("-", "_");
            return name.Pascalize();
        }

        private static void WriteToFile(string filename, string content)
        {
            var path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (var sr = new StreamWriter(filename))
            {
                sr.Write(content);
            }
        }
    }

    internal class NameComparer : IEqualityComparer<TemplarianClass>
    {
        #region Other

        public bool Equals(TemplarianClass x, TemplarianClass y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(TemplarianClass obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }
}
