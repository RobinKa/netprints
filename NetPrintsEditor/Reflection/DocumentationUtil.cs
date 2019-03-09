using Microsoft.CodeAnalysis;
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
        private static XmlDocument DocumentFromString(string xmlString)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            return doc;
        }

        public static string GetMethodSummary(IMethodSymbol methodInfo)
        {
            // TODO
            //XmlDocument doc = DocumentFromString(methodInfo.GetDocumentationCommentXml());

            string documentation = methodInfo.GetDocumentationCommentXml();

            return documentation;
        }

        public static string GetMethodParameterInfo(IParameterSymbol parameter)
        {
            // TODO
            //XmlDocument doc = DocumentFromString(parameter.GetDocumentationCommentXml());
            
            string documentation = parameter.GetDocumentationCommentXml();

            return documentation;
        }

        public static string GetMethodReturnInfo(IMethodSymbol methodInfo)
        {
            // TODO
            string documentation = methodInfo.GetDocumentationCommentXml();

            return documentation;
        }
    }
}
