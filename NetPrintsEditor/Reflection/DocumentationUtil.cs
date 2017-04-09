using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NetPrintsEditor.Reflection
{
    public static class DocumentationUtil
    {
        private static string GetAssemblyDocumentationPath(Assembly assembly)
        {
            // Try to find the documentation in the assembly's path
            
            string docPath = Path.ChangeExtension(assembly.Location, ".xml");
        
            // Try to find the documentation in the framework doc path

            if (!File.Exists(docPath))
            {
                docPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Reference Assemblies/Microsoft/Framework/.NETFramework/v4.X",
                    $"{Path.GetFileNameWithoutExtension(assembly.Location)}.xml");
            }

            // Try to find the documentation in the current path

            if (!File.Exists(docPath))
            {
                docPath = $"{Path.GetFileNameWithoutExtension(assembly.Location)}.xml";
            }

            return docPath;
        }

        private static XmlDocument GetAssemblyDocumentationDocument(Assembly assembly)
        {
            string docPath = GetAssemblyDocumentationPath(assembly);

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(docPath);
                return doc;
            }
            catch
            {
                return null;
            }
        }

        public static string GetMethodSummary(MethodInfo methodInfo)
        {
            XmlDocument doc = GetAssemblyDocumentationDocument(methodInfo.DeclaringType.Assembly);

            string searchName = $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";
            if(methodInfo.GetParameters().Length > 0)
            {
                searchName += "(";
                searchName += string.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.FullName));
                searchName += ")";
            }

            XmlNodeList nodes = doc.SelectNodes($"doc/members/member[@name='{searchName}']/summary");
            
            if(nodes.Count > 0)
            {
                return nodes.Item(0).InnerText;
            }

            return null;
        }

        public static string GetMethodParameterInfo(MethodInfo methodInfo, string parameterName)
        {
            XmlDocument doc = GetAssemblyDocumentationDocument(methodInfo.DeclaringType.Assembly);

            string searchName = $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";
            if (methodInfo.GetParameters().Length > 0)
            {
                searchName += "(";
                searchName += string.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.FullName));
                searchName += ")";
            }

            XmlNodeList nodes = doc.SelectNodes($"doc/members/member[@name='{searchName}']/param[@name='{parameterName}']");

            if (nodes.Count > 0)
            {
                return nodes.Item(0).InnerText;
            }

            return null;
        }

        public static string GetMethodReturnInfo(MethodInfo methodInfo)
        {
            XmlDocument doc = GetAssemblyDocumentationDocument(methodInfo.DeclaringType.Assembly);

            string searchName = $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";
            if (methodInfo.GetParameters().Length > 0)
            {
                searchName += "(";
                searchName += string.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.FullName));
                searchName += ")";
            }

            XmlNodeList nodes = doc.SelectNodes($"doc/members/member[@name='{searchName}']/returns");

            if (nodes.Count > 0)
            {
                return nodes.Item(0).InnerText;
            }

            return null;
        }
    }
}
