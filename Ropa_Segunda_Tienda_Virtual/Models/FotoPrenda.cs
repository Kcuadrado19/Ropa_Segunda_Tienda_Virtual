//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ropa_Segunda_Tienda_Virtual.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class FotoPrenda
    {
        public int idFoto { get; set; }
        public string FotoPrenda1 { get; set; }
        public int idPrenda { get; set; }
    
        public virtual Prenda Prenda { get; set; }
    }
}
