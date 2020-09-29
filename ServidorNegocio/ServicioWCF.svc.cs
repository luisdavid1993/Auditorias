using Cenet.ActionsAudit;
using prueba;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ServidorNegocio
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ServicioWCF" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select ServicioWCF.svc or ServicioWCF.svc.cs at the Solution Explorer and start debugging.
    public class ServicioWCF : IServicioWCF
    {
        public void DoWork()
        {
        }

        [Audit2("Carro.Marca.Descripcion", "parametro3")]
        public string GuardarCarro(Carro carro, Fabricante fabricante, string parametro2, int parametro3, DateTime parametro4, double parametro5)
        {


            System.Threading.Thread.Sleep(100);
            Convert.ToInt32("aaa");
            return carro.Marca.Descripcion;
            //throw new Exception("excepcion manual");
        }
    }
}
