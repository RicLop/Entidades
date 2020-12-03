using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Classe
    {
        public int Id { get; set; }
        public string Caminho { get; set; }
        public string Nome { get; set; }
        public bool TemCampos { get; set; } = false;

        public static Classe Controller { get => new Classe { Id = 1, Caminho = @"\{NomeSistema}.Application\Controllers", Nome = "Controller" }; }

        public static Classe CommandHandlers { get => new Classe { Id = 2, Caminho = @"\{NomeSistema}.Domain\CommandHandlers", Nome = "CommandHandler" }; }
        public static Classe Commands { get => new Classe { Id = 3, Caminho = @"\{NomeSistema}.Domain\Commands", Nome = "Commands", TemCampos = true }; }
        public static Classe CommandValidators { get => new Classe { Id = 4, Caminho = @"\{NomeSistema}.Domain\CommandValidators", Nome = "CommandValidator" }; }
        public static Classe Entities { get => new Classe { Id = 5, Caminho = @"\{NomeSistema}.Domain\Entities", Nome = "Entidade", TemCampos = true }; }

        public static Classe EventHandlers { get => new Classe { Id = 6, Caminho = @"\{NomeSistema}.Domain\EventHandlers", Nome = "EventHandler" }; }
        public static Classe Events { get => new Classe { Id = 7, Caminho = @"\{NomeSistema}.Domain\Events", Nome = "Events", TemCampos = true }; }
        public static Classe RepositoryInterface { get => new Classe { Id = 8, Caminho = @"\{NomeSistema}.Domain\Interfaces", Nome = "IRepository" }; }
        public static Classe ServiceInterfaces { get => new Classe { Id = 9, Caminho = @"\{NomeSistema}.Domain\Interfaces", Nome = "IService" }; }
        public static Classe ViewModels { get => new Classe { Id = 10, Caminho = @"\{NomeSistema}.Domain\ViewModels", Nome = "View", TemCampos = true }; }

        public static Classe Context { get => new Classe { Id = 11, Caminho = @"\{NomeSistema}.Infra.Data\Context", Nome = "NpgsqlContext" }; }
        public static Classe Mappings { get => new Classe { Id = 12, Caminho = @"\{NomeSistema}.Infra.Data\Mappings", Nome = "Map", TemCampos = true }; }
        public static Classe Repository { get => new Classe { Id = 13, Caminho = @"\{NomeSistema}.Infra.Data\Repository", Nome = "Repository" }; }

        public static Classe RepositoryDependency { get => new Classe { Id = 14, Caminho = @"\{NomeSistema}.Infra.CrossCutting\InversionOfControl", Nome = "RepositoryDependency" }; }
        public static Classe ServiceDependency { get => new Classe { Id = 15, Caminho = @"\{NomeSistema}.Infra.CrossCutting\InversionOfControl", Nome = "ServiceDependency" }; }

        public static Classe CommandToEntity { get => new Classe { Id = 16, Caminho = @"\{NomeSistema}.Infra.Shared\Mapper", Nome = "CommandToEntity" }; }
        public static Classe EntityToViewModel { get => new Classe { Id = 17, Caminho = @"\{NomeSistema}.Infra.Shared\Mapper", Nome = "EntityToView" }; }

        public static Classe Services { get => new Classe { Id = 18, Caminho = @"\{NomeSistema}.Service\Services", Nome = "Service" }; }

        public static string ObterValoresFinais(string classeNome, string classesGeradas)
        {
            var classes = classesGeradas.Split(",");

            var stringList = new List<string>();
            switch (classeNome)
            {
                case "CommandToEntity":
                    foreach (var classe in classes)
                    {
                        stringList.Add($"            CreateMap<Criar{classe}Command, {classe}>();" + Environment.NewLine +
                                       $"            CreateMap<Atualizar{classe}Command, {classe}>()" + Environment.NewLine +
                                       "                .ForMember(model => model.Id, cmd => cmd.Ignore());");
                    }

                    return string.Join(Environment.NewLine, stringList);
                    break;

                case "EntityToView":
                    foreach (var classe in classes)
                    {
                        stringList.Add($"            CreateMap<{classe}, {classe}View>();");
                    }

                    return string.Join(Environment.NewLine, stringList);
                    break;

                case "NpgsqlContextDbSet":
                    foreach (var classe in classes)
                    {
                        stringList.Add($"        public DbSet<{classe}> {classe} {{ get; set; }}");
                    }

                    return string.Join(Environment.NewLine, stringList);
                    break;

                case "NpgsqlContextMap":
                    foreach (var classe in classes)
                    {
                        stringList.Add($"            modelBuilder.Entity<{classe}>(new {classe}Map().Configure);");
                    }

                    return string.Join(Environment.NewLine, stringList);
                    break;

                case "RepositoryDependency":
                    foreach (var classe in classes)
                    {
                        stringList.Add($"            services.AddScoped<I{classe}Repository, {classe}Repository>();");
                    }

                    return string.Join(Environment.NewLine, stringList);
                    break;

                case "ServiceDependency":
                    foreach (var classe in classes)
                    {
                        stringList.Add($"            services.AddScoped<I{classe}Service, {classe}Service>();");
                    }

                    return string.Join(Environment.NewLine, stringList);
                    break;

                default:
                    return "";
                    break;
            }
        }

        public static List<Classe> ObterListaClassesTotal()
        {
            return new List<Classe>()
            {
                Classe.Controller,
                Classe.CommandHandlers,
                Classe.Commands,
                Classe.CommandValidators,
                Classe.Entities,
                Classe.EventHandlers,
                Classe.Events,
                Classe.RepositoryInterface,
                Classe.ServiceInterfaces,
                Classe.ViewModels,
                Classe.Mappings,
                Classe.Repository,
                Classe.Services,
                Classe.Context,
                Classe.RepositoryDependency,
                Classe.ServiceDependency,
                Classe.CommandToEntity,
                Classe.EntityToViewModel
            };
        }


        public static List<Classe> ObterListaClasses()
        {
            return new List<Classe>()
            {
                Classe.Controller,
                Classe.CommandHandlers,
                Classe.Commands,
                Classe.CommandValidators,
                Classe.Entities,
                Classe.EventHandlers,
                Classe.Events,
                Classe.RepositoryInterface,
                Classe.ServiceInterfaces,
                Classe.ViewModels,
                Classe.Mappings,
                Classe.Repository,
                Classe.Services
            };
        }

        public static List<Classe> ObterListaClassesFinais()
        {
            return new List<Classe>()
            {
                Classe.Context,
                Classe.RepositoryDependency,
                Classe.ServiceDependency,
                Classe.CommandToEntity,
                Classe.EntityToViewModel
            };
        }

        public static List<Classe> ObterListaClasses(string classesGeradasIds)
        {
            var classesIds = classesGeradasIds.Split(",");

            return Classe.ObterListaClassesTotal().Where(x => classesIds.Contains(x.Id.ToString())).ToList();
        }
    }

    public enum Tipo
    {
        Novo = 1,
        Editar = 2
    }
}