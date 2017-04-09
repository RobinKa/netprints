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
        private static Dictionary<string, XmlDocument> cachedDocuments =
            new Dictionary<string, XmlDocument>();

        private static Dictionary<string, string> cachedMethodSummaries =
            new Dictionary<string, string>();

        private static Dictionary<Tuple<string, string>, string> cachedMethodParameterInfos =
            new Dictionary<Tuple<string, string>, string>();

        private static Dictionary<string, string> cachedMethodReturnInfo =
            new Dictionary<string, string>();

        private static string GetMethodInfoKey(MethodInfo methodInfo)
        {
            string key = $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";

            if (methodInfo.GetParameters().Length > 0)
            {
                key += "(";
                key += string.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.FullName));
                key += ")";
            }

            return key;
        }

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
            string key = Path.GetFileNameWithoutExtension(assembly.Location);

            if (cachedDocuments.ContainsKey(key))
            {
                return cachedDocuments[key];
            }

            try
            {
                string docPath = GetAssemblyDocumentationPath(assembly);
                XmlDocument doc = new XmlDocument();
                doc.Load(File.OpenRead(docPath));

                cachedDocuments.Add(key, doc);

                return doc;
            }
            catch
            {
                return null;
            }
        }

        public static string GetMethodSummary(MethodInfo methodInfo)
        {
            string methodKey = GetMethodInfoKey(methodInfo);

            if (cachedMethodSummaries.ContainsKey(methodKey))
            {
                return cachedMethodSummaries[methodKey];
            }

            XmlDocument doc = GetAssemblyDocumentationDocument(methodInfo.DeclaringType.Assembly);
            XmlNodeList nodes = doc.SelectNodes($"doc/members/member[@name='{methodKey}']/summary");

            string documentation = null;

            if(nodes.Count > 0)
            {
                documentation = nodes.Item(0).InnerText;
            }

            cachedMethodSummaries.Add(methodKey, documentation);

            return documentation;
        }

        public static string GetMethodParameterInfo(MethodInfo methodInfo, string parameterName)
        {
            string methodKey = GetMethodInfoKey(methodInfo);
            Tuple<string, string> cacheKey = new Tuple<string, string>(methodKey, parameterName);
            if (cachedMethodParameterInfos.ContainsKey(cacheKey))
            {
                return cachedMethodParameterInfos[cacheKey];
            }

            XmlDocument doc = GetAssemblyDocumentationDocument(methodInfo.DeclaringType.Assembly);

            string searchName = $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";
            if (methodInfo.GetParameters().Length > 0)
            {
                searchName += "(";
                searchName += string.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.FullName));
                searchName += ")";
            }

            XmlNodeList nodes = doc.SelectNodes($"doc/members/member[@name='{searchName}']/param[@name='{parameterName}']");

            string documentation = null;

            if (nodes.Count > 0)
            {
                documentation = nodes.Item(0).InnerText;
            }

            cachedMethodParameterInfos.Add(cacheKey, documentation);

            return documentation;
        }

        public static string GetMethodReturnInfo(MethodInfo methodInfo)
        {
            string methodKey = GetMethodInfoKey(methodInfo);

            if (cachedMethodReturnInfo.ContainsKey(methodKey))
            {
                return cachedMethodReturnInfo[methodKey];
            }

            XmlDocument doc = GetAssemblyDocumentationDocument(methodInfo.DeclaringType.Assembly);

            string searchName = $"M:{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";
            if (methodInfo.GetParameters().Length > 0)
            {
                searchName += "(";
                searchName += string.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.FullName));
                searchName += ")";
            }

            XmlNodeList nodes = doc.SelectNodes($"doc/members/member[@name='{searchName}']/returns");

            string documentation = null;

            if (nodes.Count > 0)
            {
                documentation = nodes.Item(0).InnerText;
            }

            cachedMethodReturnInfo.Add(methodKey, documentation);

            return documentation;
        }
    }
}
