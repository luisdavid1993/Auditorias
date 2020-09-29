using Cenet.ActionsAudit;
using prueba;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditorias
{
    public class Negocio
    {
        [Audit]
        internal int Add(int a, int b)
        {
          
            for (int i = 0; i <= 1000; i++)
            {
                Console.WriteLine(i);
            }
            
            return a + b;
        }

        //[Audit("Carro.Marca.Descripcion", "parametro2",AttributeReplace =true)]
        internal void GuardarCarro(Carro carro, string parametro2, int parametro3, DateTime parametro4, double parametro5)
        {
            using (CenetAudit audit = new CenetAudit("Carro.Marca.Descripcion", "parametro2", ()=>carro, () => parametro2, () => parametro3, () => parametro4, () => parametro5))
            {
                audit.Execute(() =>
                {
                    int cero = 0;

                    System.Threading.Thread.Sleep(100);

                    //Console.WriteLine($"Hilo GuardarCarro: { System.Threading.Thread.CurrentThread.ManagedThreadId} "); 

                   //  var ind = parametro3 / cero;

                    /*  Exception ex2 = new Exception("excepcion interna 2");
                      Exception ex1 = new Exception("excepcion interna 1",ex2);
                      throw new Exception("cualquier excepcion",ex1);*/
                    //  Console.WriteLine("Se guardo el carro con exito");
                });   
            }
        }

        internal void GuardarCarroServicio(Carro carro, string parametro2, int parametro3, DateTime parametro4, double parametro5)
        {
            ProxyServicio.ServicioWCFClient proxy = new ProxyServicio.ServicioWCFClient();


            Fabricante fabricante = new Fabricante() { Descripcion = "mazda", Id = "FBR-789456" };

            Console.WriteLine(proxy.GuardarCarro(carro,fabricante ,parametro2, parametro3, parametro4, parametro5));


        }



    }
}
