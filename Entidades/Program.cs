using Entidades.Enumerators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Entidades
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "Gerador de Entidades: Ultimate.";

            do
            {
                Console.WriteLine("Copyright © 2020 Ricardo Lopesoft, Inc. All rights reserved.");
                Console.WriteLine("");

                var gerador = new Gerador();
                gerador.Tabela = new Tabela();

                Console.WriteLine("Informe o nome do sistema:");
                gerador.NomeSistema = Console.ReadLine().Trim();
                Console.Clear();

                Console.WriteLine("Extrair dados?");
                if (Console.ReadLine().Contains("s", StringComparison.OrdinalIgnoreCase))                
                    EscreverEntidadesExistentes(gerador.NomeSistema);               

                Console.Clear();
                foreach (var classe in Classe.ObterListaClasses())
                {
                    Console.WriteLine(classe.Id + " - " + classe.Nome);
                }

                Console.WriteLine("Informe as entidades que deseja gerar: \n(Separado por vigula se mais de uma, ou nenhuma se todas)");
                gerador.ClassesGeradasIds = Console.ReadLine();
                Console.Clear();

                Console.WriteLine("6 = Apagar todas entidades existentes");
                Console.WriteLine("5 = Atualizar todas entidades existentes");
                Console.WriteLine("4 = Criar a partir de entidade existente");
                Console.WriteLine("3 = Apagar entidade");
                Console.WriteLine("2 = Atualizar entidade");
                Console.WriteLine("1 = Criar nova entidade");
                int.TryParse(Console.ReadLine(), out int operacao);
                gerador.Operacao = (Operacao)operacao;
                Console.Clear();
        
                do
                {
                    if (gerador.Operacao == Operacao.CriarDeExistente || gerador.Operacao == Operacao.AtualizarDeExistente || gerador.Operacao == Operacao.ApagarTodasExistentes)
                    {
                        var geradores = ObterEntidades(gerador.NomeSistema, gerador.Operacao, gerador.ClassesGeradasIds);

                        foreach (var geradorr in geradores)
                            new GeradorEntidade().Iniciar(geradorr).Wait();

                        Console.Clear();
                        break;
                    }

                    Console.WriteLine("Informe o nome da entidade:");
                    gerador.Tabela.NomeEntidade = Console.ReadLine().Trim();
                    Console.WriteLine("");

                    if (gerador.Tabela.NomeEntidade == "")
                    {
                        Console.Clear();
                        break;
                    }

                    ObterNomeEntidade(gerador);

                    if (gerador.Operacao != Operacao.Apagar)
                    {
                        var input = string.Empty;
                        var inputList = new List<string>();
                        while (true)
                        {
                            if (string.IsNullOrWhiteSpace(input = Console.ReadLine()))
                                break;

                            if (input.Contains("{ get; set; }"))
                                inputList.Add(input);
                        }

                        ObterInformacoesGerador(inputList, gerador);
                    }
                    else
                    {
                        while (true)
                        {
                            if (string.IsNullOrWhiteSpace(Console.ReadLine()))
                            {
                                Console.Clear();
                                break;
                            }
                        }
                    }

                    Console.Clear();

                    new GeradorEntidade().Iniciar(gerador).Wait();

                    Console.WriteLine("Copyright © 2020 Ricardo Lopesoft, Inc. All rights reserved.");
                    Console.WriteLine("");
                } while (true);

                var classes = new List<int>();
                foreach (var classe in Classe.ObterListaClassesFinais())
                {
                    classes.Add(classe.Id);
                }

                gerador.ClassesGeradasIds = string.Join(",", classes);

                new GeradorEntidade().Iniciar(gerador).Wait();

            } while (true);
        }

        private static void ObterNomeEntidade(Gerador gerador)
        {
            var novoNome = gerador.Tabela.NomeEntidade;
            int de = novoNome.IndexOf("public class ") + "public class ".Length;

            int ate = 0;
            if (novoNome.Contains(":"))
                ate = novoNome.LastIndexOf(" :");
            else
                ate = novoNome.LastIndexOf("");

            gerador.Tabela.NomeEntidade = novoNome.Substring(de, ate - de);
        }

        private static void EscreverEntidadesExistentes(string nomeSistema)
        {
            var caminhoEntidades = new GeradorEntidade().CaminhoSistema + @$"\{nomeSistema}.Domain\Entities\";
            var caminhoMaps = new GeradorEntidade().CaminhoSistema + @$"\{nomeSistema}.Infra.Data\Mappings\";         

            var entidades = new DirectoryInfo(caminhoEntidades).GetFiles("*.cs")
                                                               .Select(x => new { x.Name, x.FullName })
                                                               .ToList();

            var maps = new DirectoryInfo(caminhoMaps).GetFiles("*.cs")
                                                               .Select(x => new { x.Name, x.FullName })
                                                               .ToList();

            entidades.RemoveAll(x => x.Name.Contains("Abstract"));

            var arquivo = string.Empty;
            foreach (var entidadeFile in entidades)
            {
                var entidade = File.ReadAllLines(entidadeFile.FullName).ToList();

                var inicioClasse = entidade.FindIndex(linha => linha.Contains("public class "));
                entidade.RemoveRange(0, inicioClasse);

                entidade.RemoveAt(entidade.Count - 1);

                var entidadeNova = new List<string>();
                entidadeNova.AddRange(entidade);
                foreach (var propriedade in entidade.Where(x => x.Contains("{ get; set; }")))
                {
                    var campoNovo = string.Empty;
                    var campo = ObterInputDentrePublicGetSet(propriedade).Trim();

                    if (!maps.Any(x => x.Name.Contains(entidadeFile.Name.Replace(".cs", ""))))                    
                        continue;      

                    var map = File.ReadAllLines(maps.Where(x => x.Name.Contains(entidadeFile.Name.Replace(".cs", ""))).FirstOrDefault()?.FullName).ToList();

                    var propStartIndex = map.FindIndex(i => i.Contains("builder.Property(x => x." + campo.Split(" ").LastOrDefault()));
                    if (propStartIndex < 0)
                        continue;

                    var propEndIndex = map.Skip(propStartIndex).ToList().FindIndex(i => i.Contains(";"));

                    var propMap = map.GetRange(propStartIndex, propEndIndex + 1).ToList();

                    var required = string.Empty;
                    if (propMap.Any(x => x.Contains("IsRequired")))
                    {
                        required = propMap.Where(x => x.Contains("IsRequired"))
                                          .First()
                                          .Replace(".IsRequired(", "")
                                          .Replace(");", "")
                                          .Trim();

                        if (string.IsNullOrWhiteSpace(required)) required = "true";

                        if (required == "true")
                            continue; 
                        else
                            required = "?";
                    }

                    //var maxLength = string.Empty;
                    //if (propMap.Any(x => x.Contains("HasMaxLength")))
                    //{
                    //    maxLength = propMap.Where(x => x.Contains("HasMaxLength"))
                    //                       .First()
                    //                       .Replace(".HasMaxLength(", "")
                    //                       .Replace(")", "")
                    //                       .Trim();
                    //}

                    campoNovo = campo.Split(" ")[0] + required + " " + campo.Split(" ")[1];

                    entidadeNova[entidadeNova.FindIndex(i => i.Equals(propriedade))] = "    public " + campoNovo + " { get; set; }";
                }

                arquivo += string.Join(Environment.NewLine, entidadeNova) + Environment.NewLine + Environment.NewLine;
            }
        }

        private static List<Gerador> ObterEntidades(string nomeSistema, Operacao operacao, string classesGeradasIds)
        {
            var caminhoEntidades = new GeradorEntidade().CaminhoSistema + @$"\{nomeSistema}.Domain\Entities\";

            var entidades = new DirectoryInfo(caminhoEntidades).GetFiles("*.cs")
                                                               .Select(x => x.FullName)
                                                               .ToList();
            entidades.RemoveAll(x => x.Contains("Abstract"));

            var geradores = new List<Gerador>();
            foreach (var entidadeCaminho in entidades)
            {
                var gerador = new Gerador();
                gerador.Tabela = new Tabela();
                gerador.NomeSistema = nomeSistema;
                gerador.ClassesGeradasIds = classesGeradasIds;

                if (operacao == Operacao.CriarDeExistente) gerador.Operacao = Operacao.Criar;
                else if (operacao == Operacao.ApagarTodasExistentes) gerador.Operacao = Operacao.Apagar;
                else if (operacao == Operacao.AtualizarDeExistente) gerador.Operacao = Operacao.Editar;

                var entidade = File.ReadAllLines(entidadeCaminho).ToList();

                var inicioClasse = entidade.FindIndex(linha => linha.Contains("public class "));
                entidade.RemoveRange(0, inicioClasse);

                var fimClasse = entidade.FindIndex(linha => linha.Trim() == "}");
                entidade.RemoveRange(fimClasse, 1);

                gerador.Tabela.NomeEntidade = entidade[0];

                ObterNomeEntidade(gerador);

                var propList = entidade.Where(x => x.Contains("{ get; set; }")).ToList();
                ObterInformacoesGerador(propList, gerador);

                geradores.Add(gerador);
            }

            return geradores;
        }

        public static void ObterInformacoesGerador(List<string> inputList, Gerador gerador)
        {
            var campoList = new List<string>();
            foreach (var campo in inputList)
            {
                campoList.Add(ObterInputDentrePublicGetSet(campo));
            }

            ObterTipoEntidade(campoList, gerador);

            gerador.Tabela.EhHierarquico = campoList.Where(x => x.Contains("Estrutura")).Any();

            ObterTipoHandler(gerador);            

            RemoverAdicionarCamposGerais(gerador, campoList);

            var campos = Campo.ObterListaCampoDeString(campoList);
            if (campos != null) gerador.Tabela.Campos = campos;
            campos = Campo.ObterListaCampoDeString(campoList);
            if (campos != null) gerador.Tabela.Campos = campos;
        }

        private static void RemoverAdicionarCamposGerais(Gerador gerador, List<string> campoList)
        {
            campoList.RemoveAll(x => x == "{" || x == "}" || string.IsNullOrWhiteSpace(x));
            campoList.RemoveAll(x => x.Contains("Parent") || x.Contains("Pai"));
            campoList.RemoveAll(x => x.Contains("Children") || x.Contains("Filhos"));
            campoList.RemoveAll(x => x.Split(" ").LastOrDefault() == "EmpresaId");
            campoList.RemoveAll(x => x.Split(" ").LastOrDefault() == "Empresa");
            campoList.RemoveAll(x => x.Split(" ").LastOrDefault() == "TenantId");
            campoList.RemoveAll(x => x.Split(" ").LastOrDefault() == "Tenant");
            campoList.RemoveAll(x => x.Split(" ").LastOrDefault() == "Id");
            campoList.RemoveAll(x => x.Split(" ").LastOrDefault() == "Habilitado");

            if (gerador.Tabela.EhHierarquico)
            {
                campoList.Add($"{gerador.Tabela.NomeEntidade}? PaiId");

                if (!campoList.Where(x => x.Contains("Estrutura")).Any())
                    campoList.Add("string Estrutura");

                if (!campoList.Where(x => x.Contains("Descricao")).Any())
                    campoList.Add("string Descricao");

                campoList.Add($"virtual IEnumerable<{gerador.Tabela.NomeEntidade}> Filhos");
            }
        }

        private static void ObterTipoEntidade(List<string> inputList, Gerador gerador)
        {
            if (inputList.Where(x => x.Split(" ").LastOrDefault() == "Tenant").Any())
                gerador.Tabela.TipoEntidade = "Tenant";
            else if (inputList.Where(x => x.Split(" ").LastOrDefault() == "Empresa").Any())
                gerador.Tabela.TipoEntidade = "Company";
            else
                gerador.Tabela.TipoEntidade = "";
        }

        private static void ObterTipoHandler(Gerador gerador)
        {
            if (gerador.Tabela.EhHierarquico)
            {
                switch (gerador.Tabela.TipoEntidade)
                {
                    case "":
                        gerador.Tabela.TipoHandler = $"AbstractMultitenantHieralchicalCommandHandler<{gerador.Tabela.NomeEntidade}>";
                        break;
                    case "Company":
                        gerador.Tabela.TipoHandler = $"AbstractHieralchicalCommandHandler<{gerador.Tabela.NomeEntidade}>";
                        break;
                    case "Tenant":
                        gerador.Tabela.TipoHandler = $"AbstractMulticompanyHieralchicalCommandHandler<{gerador.Tabela.NomeEntidade}>";
                        break;
                }
            }
            else
            {
                gerador.Tabela.TipoHandler = "AbstractCommandHandler";
            }
        }      

        private static string ObterInputDentrePublicGetSet(string input)
        {
            if (input.Contains("public") && input.Contains("{ get; set; }"))
                input = input.Substring(input.IndexOf("public"))
                             .Replace("public", "")
                             .Replace("{ get; set; }", "")
                             .Trim();

            return input;
        }
    }
}