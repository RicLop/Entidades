using System.Collections.Generic;

namespace Entidades
{
    public class Tabela
    {
        public string NomeEntidade { get; set; }
        public string TipoEntidade { get; set; } = "";
        public string TipoHandler { get; set; }
        public bool EhHierarquico { get; set; }
        public List<Campo> Campos { get; set; }
        public List<Campo> CamposView { get; set; }
    }
}