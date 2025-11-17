using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ExportadorDeClases
{
    public class ClassExporter
    {
        public static object ExportClass(Type type)
        {
            return new
            {
                ClassName = type.FullName,
                Namespace = type.Namespace,
                Properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                .Select(p => new { p.Name, Type = p.PropertyType.Name }),
                Fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                             .Select(f => new { f.Name, Type = f.FieldType.Name }),
                Methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                              .Where(m => !m.IsSpecialName)
                              .Select(m => new
                              {
                                  m.Name,
                                  ReturnType = m.ReturnType.Name,
                                  Parameters = m.GetParameters().Select(p => new { p.Name, Type = p.ParameterType.Name })
                              })
            };
        }

        public static void ExportToJson(Type type, string outputPath)
        {
            var data = ExportClass(type);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(outputPath, json);
            Console.WriteLine($"Exportado a: {outputPath}");
        }


        public static List<MemberInfoExport> CollectMembers(Type type)
        {
            var list = new List<MemberInfoExport>();

            list.AddRange(type.GetProperties().Select(p => new MemberInfoExport
            {
                MemberType = "Property",
                Name = p.Name,
                DataType = p.PropertyType.Name
            }));

            list.AddRange(type.GetFields().Select(f => new MemberInfoExport
            {
                MemberType = "Field",
                Name = f.Name,
                DataType = f.FieldType.Name
            }));

            list.AddRange(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                              .Where(m => !m.IsSpecialName)
                              .Select(m => new MemberInfoExport
                              {
                                  MemberType = "Method",
                                  Name = m.Name,
                                  DataType = m.ReturnType.Name,
                                  Parameters = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))
                              }));

            return list;
        }

        public static void ExportToCsv(Type type, string outputPath)
        {
            var members = CollectMembers(type);
            var lines = new List<string> { "MemberType,Name,DataType,Parameters" };

            lines.AddRange(members.Select(m =>
                $"{m.MemberType},{m.Name},{m.DataType},{(m.Parameters ?? "")}"));

            File.WriteAllLines(outputPath, lines);
            Console.WriteLine($"CSV exportado a: {outputPath}");
        }


        public static void ExportToXml(Type type, string outputPath)
        {
            var members = CollectMembers(type);
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<MemberInfoExport>));

            using var writer = new StreamWriter(outputPath);
            serializer.Serialize(writer, members);
            Console.WriteLine($"XML exportado a: {outputPath}");
        }


    }
}
