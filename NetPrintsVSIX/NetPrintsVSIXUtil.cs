using NetPrints.Core;
using NetPrints.Serialization;
using NetPrints.Translator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPrintsVSIX
{
    public static class NetPrintsVSIXUtil
    {
        public static void CompileNetPrintsClass(string path, string outputPath)
        {
            CompileNetPrintsClass(SerializationHelper.LoadClass(path), outputPath);
        }

        public static void CompileNetPrintsClass(ClassGraph classGraph, string outputPath)
        {
            ClassTranslator classTranslator = new ClassTranslator();

            string translated = classTranslator.TranslateClass(classGraph);

            File.WriteAllText(outputPath, translated);
        }
    }
}
