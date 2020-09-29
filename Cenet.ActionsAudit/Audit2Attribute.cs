using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cenet.ActionsAudit
{

    [AttributeUsage(AttributeTargets.Method)]
    public class Audit2Attribute : Attribute
    {


        public string FindReference1 { get; }
        public string FindReference2 { get; }


        public Audit2Attribute()
        {

        }

        public Audit2Attribute(string findReference1 = "", string findReference2 = "") : base()
        {
            this.FindReference1 = findReference1;
            this.FindReference2 = findReference2;
        }


    }
}
