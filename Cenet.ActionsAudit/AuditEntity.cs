using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cenet.ActionsAudit
{
    internal class AuditEntity
    {

        public string NombreMetodo { get; set; }
        public Dictionary<string, object> Parametros { get; set; }

        public double TiempoEjecucion { get; set; }
        public bool Excepcion { get; set; }

        public Dictionary<string, string> ReferenciasBusqueda { get; set; }

        public int idHiloParamParaBorrar { get; set; }

    }
}

