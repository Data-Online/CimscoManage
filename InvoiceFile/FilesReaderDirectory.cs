using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceFile
{
    public class FilesReaderDirectory
    {

        public static List<string> GetListOfDirectories(string _directory)
        {
            List<string> _list = Directory.GetDirectories(_directory).ToList();
            List<string> _returnlist = new List<string>();
            foreach (var s in _list)
            {
                _returnlist.Add(Path.GetFileName(s));
            }
            //List<string> _list = Directory.GetDirectories(_directory, "*", System.IO.SearchOption.AllDirectories).ToList();
            return _returnlist;
        }

        public static List<string> GetFilesFromDirectories(string _directory)
        {
            List<string> _list = Directory.GetFiles(_directory).ToList();
            List<string> _returnlist = new List<string>();
            foreach (var s in _list)
            {
                _returnlist.Add(Path.GetFileName(s));
            }
            //List<string> _list = Directory.GetDirectories(_directory, "*", System.IO.SearchOption.AllDirectories).ToList();
            return _returnlist;
        }


        public static List<string> GetNameInList(List<string> list)
        {
            List<string> _returnlist = new List<string>();
            foreach (var s in list)
            {
                _returnlist.Add(Path.GetFileName(s));
            }
            //List<string> _list = Directory.GetDirectories(_directory, "*", System.IO.SearchOption.AllDirectories).ToList();
            return _returnlist;
        }
    }
}
