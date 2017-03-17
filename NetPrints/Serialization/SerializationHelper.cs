using NetPrints.Core;
using System.IO;
using System.Runtime.Serialization;

namespace NetPrints.Serialization
{
    public class SerializationHelper
    {
        private static DataContractSerializer classSerializer = new DataContractSerializer(
                typeof(Class), null, int.MaxValue, false, true, new TypeReplacementSurrogate());

        public static void SaveClass(Class cls, string outputPath)
        {
            using (FileStream fileStream = File.Open(outputPath, FileMode.Create))
            {
                classSerializer.WriteObject(fileStream, cls);
            }
        }

        public static Class LoadClass(string path)
        {
            using (FileStream fileStream = File.OpenRead(path))
            {
                if (classSerializer.ReadObject(fileStream) is Class cls)
                {
                    return cls;
                }
            }

            return null;
        }
    }
}
