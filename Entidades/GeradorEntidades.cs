using Entidades.Enumerators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class GeradorEntidade
    {
        public string Caminho = @"C:\Mestre\Web\Backend\MestreWeb";
        public string NomeEntidade { get; set; }
        public string NomeSistema { get; set; }
        public string CaminhoSistema { get => Caminho; }
        public string CaminhoTemplate { get => @"D:\Mestre\Desenv\ConsoleEntidades\Entidades\Entidades\ClassesTemplates\"; }

        public async Task Iniciar(Gerador gerador)
        {
            NomeSistema = gerador.NomeSistema;
            NomeEntidade = gerador.Tabela.NomeEntidade;
            if (gerador.Operacao == Operacao.Criar)
                CriarEntidade(gerador);
            else if (gerador.Operacao == Operacao.Apagar)
                ApagarEntidade(gerador);
            else if (gerador.Operacao == Operacao.Editar)
            {
                ApagarEntidade(gerador);
                CriarEntidade(gerador);
            }
        }

        private async Task CriarEntidade(Gerador gerador)
        {
            var classes = new List<Classe>();

            if (string.IsNullOrWhiteSpace(gerador.ClassesGeradasIds))
                classes = Classe.ObterListaClasses();
            else
                classes = Classe.ObterListaClasses(gerador.ClassesGeradasIds);

            foreach (var classe in classes)
            {
                if (Classe.ObterListaClassesFinais().Select(x => x.Id).ToList().Contains(classe.Id))
                    GerarArquivoFinal(classe, gerador);
                else
                    GerarArquivo(classe, gerador);
                
            }
        }

        private async Task ApagarEntidade(Gerador gerador)
        {
            var classes = new List<Classe>();

            if (string.IsNullOrWhiteSpace(gerador.ClassesGeradasIds))
                classes = Classe.ObterListaClasses();
            else
                classes = Classe.ObterListaClasses(gerador.ClassesGeradasIds);

            foreach (var classe in classes)
            {
                ApagarArquivo(classe, gerador);
            }
        }

        private async Task ApagarArquivo(Classe classe, Gerador gerador)
        {
            string nomeArquivo = ObterNomeArquivo(classe.Nome, gerador.Tabela.NomeEntidade);

            var caminhoArquivo = Path.Combine(CaminhoSistema + classe.Caminho.Replace("{NomeSistema}", NomeSistema), nomeArquivo);
            if (File.Exists(caminhoArquivo))
            {
                File.Delete(caminhoArquivo);
            }
        }

        private async Task GerarArquivoFinal(Classe classe, Gerador gerador)
        {
            var caminhoEntidades = CaminhoSistema + Classe.Entities.Caminho.Replace("{NomeSistema}", NomeSistema);

            var entidadesCS = new DirectoryInfo(caminhoEntidades).GetFiles("*.cs").Where(x => !x.Name.Contains("Abstract")).ToList();
            var entidades = new List<string>();
            foreach (var entidade in entidadesCS)
            {
                entidades.Add(entidade.Name.Replace(".cs", ""));
            }

            gerador.ClassesGeradasNomes = string.Join(",", entidades);

            string nomeArquivo = classe.Nome + ".cs"; 

            var caminhoArquivo = Path.Combine(CaminhoSistema + classe.Caminho.Replace("{NomeSistema}", NomeSistema), nomeArquivo);

            if (File.Exists(caminhoArquivo))            
                File.Delete(caminhoArquivo);            

            if (!File.Exists(caminhoArquivo))
            {
                using (var novoArquivo = new StreamWriter(caminhoArquivo))
                {
                    novoArquivo.Write(EscreverTemplate(classe.Nome, gerador));
                }
            }
        }

        private async Task GerarArquivo(Classe classe, Gerador gerador)
        {
            string nomeArquivo = ObterNomeArquivo(classe.Nome, gerador.Tabela.NomeEntidade);

            var caminhoArquivo = Path.Combine(CaminhoSistema + classe.Caminho.Replace("{NomeSistema}", NomeSistema), nomeArquivo);
            if (!File.Exists(caminhoArquivo))
            {
                using (var novoArquivo = new StreamWriter(caminhoArquivo))
                {
                    novoArquivo.Write(EscreverTemplate(classe.Nome, gerador));
                }
            }
        }

        private string EscreverTemplate(string classe, Gerador gerador)
        {
            if (classe == Classe.CommandHandlers.Nome && gerador.Tabela.EhHierarquico)
            {
                classe += "Hierarquico";
            }

            using (var template = new StreamReader(CaminhoTemplate + @$"\{classe}.txt"))
            {
                var arquivo = template.ReadToEnd();

                if (arquivo.Contains("{NomeSistema}")) arquivo = arquivo.Replace("{NomeSistema}", NomeSistema);
                if (arquivo.Contains("{NomeEntidade}")) arquivo = arquivo.Replace("{NomeEntidade}", gerador.Tabela.NomeEntidade);
                if (arquivo.Contains("{TipoEntidade}")) arquivo = arquivo.Replace("{TipoEntidade}", gerador.Tabela.TipoEntidade);
                if (arquivo.Contains("{TipoHandler}")) arquivo = arquivo.Replace("{TipoHandler}", gerador.Tabela.TipoHandler);
                if (arquivo.Contains("{Hierarquico}")) arquivo = arquivo.Replace("{Hierarquico}", ObterHierarquico(classe, gerador));
                if (arquivo.Contains("{Campos}")) arquivo = arquivo.Replace("{Campos}", ObterCamposString(gerador.Tabela.Campos, classe));
                if (arquivo.Contains("{Mappers}")) arquivo = arquivo.Replace("{Mappers}", ObterMappersString(gerador.Tabela.Campos, classe));
                if (arquivo.Contains("{Mappings}")) arquivo = arquivo.Replace("{Mappings}", ObterMappingsString(gerador.Tabela.Campos, classe));
                if (arquivo.Contains("{Validators}")) arquivo = arquivo.Replace("{Validators}", ObterValidatorString(gerador.Tabela.Campos, classe));
                if (arquivo.Contains("{EmpresaId}")) arquivo = arquivo.Replace("{EmpresaId}", gerador.Tabela.TipoEntidade == "Company" ? "entidade.EmpresaId = Auth.Empresa.Id;" : "");
                if (arquivo.Contains("{TenantId}")) arquivo = arquivo.Replace("{TenantId}", gerador.Tabela.TipoEntidade == "Company" || gerador.Tabela.TipoEntidade == "Tenant" ? "entidade.TenantId = Auth.Tenant.Id;" : "");
                arquivo = ObterReplacesFinais(classe, gerador.ClassesGeradasNomes, arquivo);

                return arquivo;
            }
        }

        private static string ObterReplacesFinais(string classe, string classesGeradasNomes, string arquivo)
        {
            if (arquivo.Contains("{ComToEnt}")) arquivo = arquivo.Replace("{ComToEnt}", Classe.ObterValoresFinais(classe, classesGeradasNomes));
            if (arquivo.Contains("{EntToView}")) arquivo = arquivo.Replace("{EntToView}", Classe.ObterValoresFinais(classe, classesGeradasNomes));
            if (arquivo.Contains("{RepositoryDep}")) arquivo = arquivo.Replace("{RepositoryDep}", Classe.ObterValoresFinais(classe, classesGeradasNomes));
            if (arquivo.Contains("{ServiceDep}")) arquivo = arquivo.Replace("{ServiceDep}", Classe.ObterValoresFinais(classe, classesGeradasNomes));
            if (arquivo.Contains("{DbSet}")) arquivo = arquivo.Replace("{DbSet}", Classe.ObterValoresFinais(classe + "DbSet", classesGeradasNomes));
            if (arquivo.Contains("{Map}")) arquivo = arquivo.Replace("{Map}", Classe.ObterValoresFinais(classe + "Map", classesGeradasNomes));

            return arquivo;
        }

        private string ObterHierarquico(string classe, Gerador gerador)
        {
            if (classe == Classe.Entities.Nome)
                return gerador.Tabela.EhHierarquico ? ", IHierarchicalEntity" : "";
            else if (classe == Classe.Commands.Nome)
                return gerador.Tabela.EhHierarquico ? ", IHierarchicalCommand" : "";
            else
                return "";
        }

        private string ObterCamposString(List<Campo> campos, string classe)
        {
            var espacos = string.Empty;
            if (classe == Classe.Entities.Nome)
            {
                espacos = "        ";
            }
            else if (classe == Classe.Events.Nome || classe == Classe.Commands.Nome)
            {
                campos = campos.Where(x => !x.Virtual).ToList();
                espacos = "        ";
            }
            else if (classe == Classe.ViewModels.Nome)
            {
                campos = campos.Where(x => !x.Virtual).ToList();
                campos = campos.Where(x => x.Nome != "Habilitado").ToList();
                campos = Campo.ObterCamposDescricao(campos);
                espacos = "        ";
            }

            var campoListStr = new List<string>();
            foreach (var campo in campos)
            {
                campoListStr.Add(campo.ToString(CampoTipo.Propriedade));
            }

            return string.Join($"\r\n{espacos}", campoListStr);
        }

        private string ObterMappingsString(List<Campo> campos, string classe)
        {
            campos = campos.Where(x => !x.Virtual).ToList();

            var campoListStr = new List<string>();
            foreach (var campo in campos)
            {
                campoListStr.Add(campo.ToString(CampoTipo.Map));
            }

            return string.Join($"\r\n", campoListStr);
        }

        private string ObterMappersString(List<Campo> campos, string classe)
        {
            var camposDescricao = campos.Where(x => !x.Virtual && !TiposPadroes.Tipos.Any(x.Tipo.Contains)).ToList();
            camposDescricao = Campo.ObterCamposDescricao(camposDescricao).Where(x => x.Nome.Contains("Descricao")).ToList();

            var campoListStr = new List<string>();
            foreach (var campo in camposDescricao)
            {
                campoListStr.Add(campo.ToString(CampoTipo.Mapper, NomeEntidade));
            }

            return string.Join($"\r\n            ", campoListStr);
        }

        private string ObterValidatorString(List<Campo> campos, string classe)
        {
            campos = campos.Where(x => !x.Virtual).ToList();

            var campoListStr = new List<string>();
            foreach (var campo in campos)
            {
                var campoString = campo.ToString(CampoTipo.Validator);
                if (!string.IsNullOrWhiteSpace(campoString))    
                    campoListStr.Add(campoString);
            }

            return string.Join($"\r\n", campoListStr);
        }        

        private string ObterNomeArquivo(string classeNome, string entidadeNome)
        {
            if (!classeNome.Contains("I"))
                return classeNome == "Entidade" ? $"{entidadeNome}.cs" : $"{entidadeNome}{classeNome}.cs";
            else
                return $"I{entidadeNome}{classeNome.Replace("I", "")}.cs";
        }

        private static List<int> AdicionarLinhasApagadas(int nmrLinha, int quantidade, bool removerAnterior = false)
        {
            var linhas = new List<int>();                      

            for (int i = 0; i < quantidade; i++)           
               linhas.Add(nmrLinha);

            if (removerAnterior)
                linhas.Add(nmrLinha - 1);

            return linhas;
        }
    }
}
