using Cenet.ActionsAudit;
using prueba;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditorias
{
    class Program
    {
        static void Main(string[] args)
        {
            /*List<double> lstTiempos = new List<double>();
            DateTime inicioTotal = DateTime.Now;

            for (int i = 0; i <= 1000; i++)
            {
                DateTime inicioParcial = DateTime.Now;
                Negocio obj = new Negocio();
                int r = obj.Add(1, 5);
                DateTime finalParcial = DateTime.Now;
                Console.WriteLine(r);
                lstTiempos.Add((finalParcial - inicioParcial).TotalSeconds);
            }



            DateTime finalTotal = DateTime.Now;

            Console.WriteLine("Tiempo total segundos => " + (finalTotal - inicioTotal).TotalSeconds);

            Console.WriteLine("Tiempo promedio segundos => " + lstTiempos.Sum() / lstTiempos.Count);
            */

            //Console.WriteLine("Main() :" + System.Threading.Thread.CurrentThread.ManagedThreadId);


            
            int i = 0;
            while (i<=0)
            {
              
              
                try
                {

                    Negocio objNegocio = new Negocio();
                    Random r = new Random();

                    Carro objCarro = new Carro()
                    {
                        Color = EnumColor.Rojo,
                        Linea = "Accord",
                        Modelo = 2020,
                        Marca = new Fabricante() { Descripcion = "Hyundai" },
                        TipoUso = EnumTipoUso.Familiar,
                        IdHiloPrincipal = System.Threading.Thread.CurrentThread.ManagedThreadId,
                        IdCarro = r.Next()

                    };

                    // Console.WriteLine($"Hilo : { System.Threading.Thread.CurrentThread.ManagedThreadId} ");
                    /*Task.Factory.StartNew(() =>
                    {
                       

                            objCarro.Marca.Descripcion = Guid.NewGuid().ToString();
                            objCarro.IdHiloHijo = System.Threading.Thread.CurrentThread.ManagedThreadId;

                            Console.WriteLine($"relacion hilo id : {objCarro.Marca.Descripcion} hiloHijo : {objCarro.IdHiloHijo}");

                            objNegocio.GuardarCarro(objCarro, "valor parametro2", 456, DateTime.Now, 7895.44);
                      

                    });*/
                    Task.Factory.StartNew(() =>
                    {


                        objCarro.Marca.Descripcion = Guid.NewGuid().ToString();
                        objCarro.IdHiloHijo = System.Threading.Thread.CurrentThread.ManagedThreadId;

                        Console.WriteLine($"relacion hilo id : {objCarro.Marca.Descripcion} hiloHijo : {objCarro.IdHiloHijo}");

                        objNegocio.GuardarCarroServicio(objCarro, "valor parametro2", 456, DateTime.Now, 7895.44);


                    });



                    /* Task.Factory.StartNew(() =>
                     {
                         objCarro.Marca.Descripcion = Guid.NewGuid().ToString();
                         objCarro.IdHiloHijo = System.Threading.Thread.CurrentThread.ManagedThreadId;

                         Console.WriteLine($"relacion hilo id : {objCarro.Marca.Descripcion} hiloHijo : {objCarro.IdHiloHijo}");

                         objNegocio.GuardarCarro(objCarro, "valor parametro2", 456, DateTime.Now, 7895.44);
                     });
                     Task.Factory.StartNew(() =>
                     {
                         objCarro.Marca.Descripcion = Guid.NewGuid().ToString();
                         objCarro.IdHiloHijo = System.Threading.Thread.CurrentThread.ManagedThreadId;

                         Console.WriteLine($"relacion hilo id : {objCarro.Marca.Descripcion} hiloHijo : {objCarro.IdHiloHijo}");

                         objNegocio.GuardarCarro(objCarro, "valor parametro2", 456, DateTime.Now, 7895.44);
                     });*/

                }
                catch
                { }
                finally
                {
                    i++;
                }
            }
            Console.ReadLine();
        }
    }
}
