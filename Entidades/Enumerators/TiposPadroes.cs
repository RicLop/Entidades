using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.Enumerators
{
    public static class TiposPadroes
    {
        public static List<string> Tipos { get => new List<string>() { TipoPadrao.String.Tipo, TipoPadrao.DateTime.Tipo, TipoPadrao.Int.Tipo, TipoPadrao.Long.Tipo, TipoPadrao.Decimal.Tipo, TipoPadrao.Bool.Tipo }; }
    }

    public class TipoPadrao
    {
        public int Id { get; set; }
        public string Tipo { get; set; }

        public static TipoPadrao String { get => new TipoPadrao() { Id = 1, Tipo = "string" }; }
        public static TipoPadrao DateTime { get => new TipoPadrao() { Id = 2, Tipo = "DateTime" }; }
        public static TipoPadrao Int { get => new TipoPadrao() { Id = 3, Tipo = "int" }; }
        public static TipoPadrao Long { get => new TipoPadrao() { Id = 4, Tipo = "long" }; }
        public static TipoPadrao Decimal { get => new TipoPadrao() { Id = 5, Tipo = "decimal" }; }
        public static TipoPadrao Bool { get => new TipoPadrao() { Id = 6, Tipo = "bool" }; }
        public static TipoPadrao Guid { get => new TipoPadrao() { Id = 7, Tipo = "Guid" }; }
    }
}