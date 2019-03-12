using NetPrints.Core;
using System.IO;
using System.Runtime.Serialization;

namespace NetPrints.Serialization
{
    public class SerializationHelper
    {
        private static DataContractSerializer classSerializer = new DataContractSerializer(typeof(Class), new DataContractSerializerSettings()
        {
            PreserveObjectReferences = true,
            MaxItemsInObjectGraph = int.MaxValue,
        });

        /// <summary>
        /// Saves a class to a path. The class can be loaded again using LoadClass.
        /// </summary>
        /// <param name="cls">Class to save.</param>
        /// <param name="outputPath">Path to save the class at.</param>
        public static void SaveClass(Class cls, string outputPath)
        {
            using (FileStream fileStream = File.Open(outputPath, FileMode.Create))
            {
                classSerializer.WriteObject(fileStream, cls);
            }
        }

        /// <summary>
        /// Loads a class from a path.
        /// </summary>
        /// <param name="outputPath">Path to load the class from. Throws a FileLoadException if the read object was not a class.</param>
        public static Class LoadClass(string path)
        {
            using (FileStream fileStream = File.OpenRead(path))
            {
                if (classSerializer.ReadObject(fileStream) is Class cls)
                {
                    return cls;
                }
            }

            throw new FileLoadException();
        }
    }
}
