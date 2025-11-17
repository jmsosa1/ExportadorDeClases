using System;
using System.Collections.Generic;
using System.Text;

namespace ExportadorDeClases
{
    public class MemberInfoExport
    {
        public string MemberType { get; set; } // Property, Field, Method
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Parameters { get; set; } // Solo para métodos
    }
}
