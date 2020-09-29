using prueba;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ServidorNegocio
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IServicioWCF" in both code and config file together.
    [ServiceContract]
    public interface IServicioWCF
    {
        [OperationContract]
        void DoWork();

        [OperationContract]
        string GuardarCarro(Carro carro, Fabricante fabricante, string parametro2, int parametro3, DateTime parametro4, double parametro5);

    }
}
