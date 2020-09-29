using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prueba
{

    public class Carro
    {


        public int IdCarro { get; set; }
        public int Modelo { get; set; }

        public EnumColor Color { get; set; }

        public EnumTipoUso TipoUso { get; set; }
     
        public Fabricante Marca { get; set; }
               

        public string Linea { get; set; }
        
        public int IdHiloPrincipal { get; set; }

        public int IdHiloHijo { get; set; }
    }

    public class Fabricante
    {
        public string Id { get; set; }

        public string Descripcion { get; set; }
    }

    public enum EnumTipoUso
    {
        Familiar,
        Individual,
        Carga,
        Pasajeros
    }

    public enum EnumColor
    {
        Rojo,
        Gris,
        Negro,
        Azul
    }

}
