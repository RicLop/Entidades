using Entidades.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Gerador
    {        
        public string NomeSistema { get; set; }
        public string ClassesGeradasIds { get; set; }
        public string ClassesGeradasNomes { get; set; }
        public Operacao Operacao { get; set; }
        public Tabela Tabela { get; set; }
    }
}