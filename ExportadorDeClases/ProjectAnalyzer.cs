using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;

namespace ExportadorDeClases
{
    public static class ProjectAnalyzer
    {
        public static async Task<List<string>> GetClassNamesAsync(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("projectPath no puede estar vacío.", nameof(projectPath));

            // Registrar MSBuild disponible para que MSBuildWorkspace pueda inicializar los servicios de lenguaje C#
            if (!MSBuildLocator.IsRegistered)
            {
                try
                {
                    MSBuildLocator.RegisterDefaults();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("No se pudo registrar MSBuild. Asegúrate de tener instalado Visual Studio o .NET SDK/Build Tools compatibles.", ex);
                }
            }

            using var workspace = MSBuildWorkspace.Create();

            // Opcionalmente suscribirse a los diagnostics para más información
            workspace.WorkspaceFailed += (s, e) =>
            {
                // Puedes cambiar esto por logging si lo prefieres
                System.Diagnostics.Debug.WriteLine($"WorkspaceFailed: {e.Diagnostic.Message}");
            };

            Project project;
            try
            {
                project = await workspace.OpenProjectAsync(projectPath);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"No se puede abrir el proyecto '{projectPath}' porque el lenguaje 'C#' no es compatible. Comprueba que MSBuild y los paquetes de Roslyn estén instalados y que el proyecto sea un .csproj válido.");
            }

            var classNames = new List<string>();

            foreach (var document in project.Documents.Where(d => d.FilePath?.EndsWith('.' + "cs") == true))
            {
                var syntaxTree = await document.GetSyntaxTreeAsync();
                if (syntaxTree == null) continue;

                var root = await syntaxTree.GetRootAsync();
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var cls in classes)
                {
                    var ns = cls.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();
                    var fullName = string.IsNullOrEmpty(ns) ? cls.Identifier.Text : $"{ns}.{cls.Identifier.Text}";
                    classNames.Add(fullName);
                }
            }

            return classNames;
        }
    }
}
