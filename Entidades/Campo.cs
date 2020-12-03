using Entidades.Enumerators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;

namespace Entidades
{
    public class Campo : ICloneable
    {
        public string Nome { get; set; }
        public string Tipo { get; set; }
        public bool Opcional { get; set; } = false;
        public bool Virtual { get; set; } = false;
        public bool Filho { get; set; } = false;
        public string FilhoTipo { get; set; }
        public int Tamanho { get; set; } = 0;
        public string ValorPadrao { get; set; } = "";
        public bool EhFK { get; set; } = false;
        public string FKNome { get; set; }
        public string FKTipo { get; set; }

        public Object Clone()
        {
            return this.MemberwiseClone();
        }

        public string ToString(CampoTipo tipo, string nomeEntidade = "")
        {
            if (tipo == CampoTipo.Propriedade)
            {
                var campoStr = string.Empty;

                campoStr = "public ";

                if (Virtual) campoStr += "virtual ";
                campoStr += Opcional ? Tipo + "?" : Tipo;
                campoStr += " " + Nome;
                campoStr += " { get; set; }";

                if (!string.IsNullOrEmpty(ValorPadrao))
                    campoStr += " = " + ValorPadrao + ";";

                return campoStr;
            }
            else if (tipo == CampoTipo.Map)
            {
                Opcional = !Opcional;
                var opicional = $"{Opcional.ToString()[0].ToString().ToLower()}{Opcional.ToString().Substring(1)}";

                if (FKNome == "Pai")
                {
                    return $"            builder.HasOne(x => x.{FKNome})" + Environment.NewLine +
                           $"                   .WithMany(x => x.Filhos)" + Environment.NewLine +
                           $"                   .HasForeignKey(x => x.{Nome});";
                }
                else if (EhFK)
                {
                    return $"            builder.HasOne(x => x.{FKNome})" + Environment.NewLine +
                           $"                   .WithMany()" + Environment.NewLine +
                           $"                   .IsRequired({opicional})" + Environment.NewLine +
                           $"                   .HasForeignKey(x => x.{Nome});";
                }
                else
                {
                    var str = $"            builder.Property(x => x.{Nome})" + Environment.NewLine;
                    if (Tamanho > 0) str += $"                .HasMaxLength({Tamanho})" + Environment.NewLine;
                    return str += $"                   .IsRequired({opicional});";
                }
            }
            else if (tipo == CampoTipo.Validator && !Opcional && !Tipo.Contains("bool", StringComparison.InvariantCultureIgnoreCase) && EstaEntreTiposPadroes(Tipo))
            {
                var must = string.Empty;
                if (Tipo == TipoPadrao.Bool.Tipo)
                    must = ".Must(x => x != null)";
                else if (Tipo == TipoPadrao.String.Tipo)
                    must = ".Must(x => !string.IsNullOrWhiteSpace(x))";
                else if (Tipo == TipoPadrao.DateTime.Tipo)
                    must = ".Must(x => x != null && x != DateTime.MinValue)";
                else if (Tipo == TipoPadrao.Long.Tipo || Tipo == TipoPadrao.Decimal.Tipo || Tipo == TipoPadrao.Decimal.Tipo)
                    must = ".Must(x => x != null && x >= 0)";
                else
                    must = ".NotEmpty()";

                return $"            RuleFor(x => x.{Nome})" + Environment.NewLine +
                       $"               {must}" + Environment.NewLine +
                       $"               .WithMessage(\"\\\"{Nome}\\\" é obrigatório.\");";
            }
            else if (tipo == CampoTipo.Mapper)
            {
                var nomeNormal = Nome.Replace("DescricaoView", "").Replace("Descricao", "");
                var select = "";
                if (nomeNormal.EndsWith("Id"))
                {
                    var nomeSemId = nomeNormal.Replace("Id", "");
                    if (nomeNormal.Trim() == "PaiId")
                        nomeSemId = nomeEntidade;

                    return $@"entidade.{Nome} = mediator.Send(new Obter{nomeSemId}InstanceCommand(x => x.Id == entidade.{nomeNormal}))
                                                        .Result.FirstOrDefault()?.Descricao;";
                }
                else
                {
                    return $"entidade.{Nome} = StringValueAttribute.GetStringValue(entidade.{nomeNormal});";
                }
            }

            return string.Empty;
        }



        public static List<Campo> ObterListaCampoDeString(List<string> inputs)
        {
            var campos = new List<Campo>();

            foreach (var input in inputs)
            {
                var campo = new Campo();
                var campoStr = input;

                if (input.Contains("virtual"))
                {
                    campo.Virtual = true;
                    campoStr = campoStr.Replace("virtual", "");
                }

                if (campoStr.Contains("IEnumerable") || campoStr.Contains("IQueryable"))
                {
                    campo.Filho = true;

                    var de = input.IndexOf("<") + "<".Length;
                    var ate = input.LastIndexOf(">");

                    campo.FilhoTipo = input.Substring(de, ate - de);

                    if (input.Contains("IEnumerable")) campo.Tipo = $"IEnumerable<{campo.FilhoTipo}>";
                    else if (input.Contains("IQueryable")) campo.Tipo = $"IQueryable<{campo.FilhoTipo}>";

                    campo.Nome = campoStr.Replace(campo.Tipo, "").Trim();

                    campos.Add(campo);
                    continue;
                }

                if (campoStr.Contains("?"))
                {
                    campo.Opcional = true;
                    campoStr = campoStr.Replace("?", "");
                }

                if (campoStr.Contains("="))
                {
                    campo.ValorPadrao = input.Substring(input.IndexOf("=") + "=".Length)
                                             .Replace(";", "")
                                             .Trim();
                    campoStr = campoStr.Replace("= " + campo.ValorPadrao + ";", "");
                }

                if (campoStr.Any(c => char.IsDigit(c)))
                {
                    campo.Tamanho = Convert.ToInt32(input.Where(c => char.IsDigit(c)).ToString());
                    campoStr = campoStr.Replace(campo.Tamanho.ToString(), "");
                }            

                var props = campoStr.Split(" ").Where(x => !string.IsNullOrEmpty(x)).ToArray();

                if (props.Length == 2 && (!props[1].EndsWith("Id") ||
                    props[1].EndsWith("Id") && props[0] == "Guid" ||
                    props[1].Contains("Detalhe")))
                {
                    campo.Tipo = props[0];
                    campo.Nome = props[1];
                }
                else if (props.Length == 1)
                {
                    campo.Nome = props[0];
                    campo.Tipo = "Guid";
                    campo.FKNome = campo.Nome.Substring(0, campo.Nome.LastIndexOf("Id"));
                    campo.FKTipo = props[0].Substring(0, props[0].LastIndexOf("Id"));
                    campo.EhFK = true;
                }
                else
                {
                    campo.Nome = props[1];
                    campo.Tipo = "Guid";
                    campo.FKNome = campo.Nome.Substring(0, campo.Nome.LastIndexOf("Id"));
                    campo.FKTipo = props[0];
                    campo.EhFK = true;
                }

                if (campo.EhFK)
                {
                    var campoVirtual = campo.Clone() as Campo;
                    campoVirtual.Nome = campo.FKNome;
                    campoVirtual.Tipo = campo.FKTipo.Replace("?", "");
                    campoVirtual.FKTipo = null;
                    campoVirtual.EhFK = false;
                    campoVirtual.Virtual = true;
                    campos.Add(campoVirtual);
                }

                campos.Add(campo);
            }

            return campos;
        }

        public static List<Campo> ObterCamposDescricao(List<Campo> campos)
        {
            var novaLista = new List<Campo>();
            foreach (var campo in campos)
            {
                if (campo.Tipo.Contains("Guid") || !EstaEntreTiposPadroes(campo.Tipo))
                {
                    novaLista.Add(campo);

                    var novoCampo = campo.Clone() as Campo;
                    novoCampo.Nome += "Descricao";
                    novoCampo.Tipo = "string";
                    novaLista.Add(novoCampo);
                }
                else                
                    novaLista.Add(campo);                
            }

            return novaLista;
        }

        public static bool EstaEntreTiposPadroes(string tipo)
        {
            return TiposPadroes.Tipos.Any(x => x.Contains(tipo, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}