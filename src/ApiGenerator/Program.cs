using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using SagaNetwork;
using System.Diagnostics;

namespace ApiGenerator
{
    public class Program
    {
        public static string TEMPLATES_DIRECTORY_PATH => $@"{Environment.CurrentDirectory}\Templates";
        public static string GENERATED_DIRECTORY_PATH => $@"{AppDomain.CurrentDomain.BaseDirectory}_Generated";
        public static string GENERATED_FILE_NAME => @"GeneratedApi.h";

        public static void Main (string[] args)
        {
            var sagaNetworkTypes = Assembly.GetAssembly(typeof(SagaNetwork.Models.Player)).GetTypes();
            var reflectedClasses = sagaNetworkTypes.Where(type => (type.IsClass || type.IsEnum) &&
                type.GetCustomAttributes<GenerateApiAttribute>(true).Any()).ToList();
            var reflectedStatusNames = new List<string>();
            foreach (var prop in typeof(JStatus).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static))
                reflectedStatusNames.Add(prop.Name);

            var uClasses = new List<GeneratedUClass>();
            foreach (var reflectedClass in reflectedClasses)
                uClasses.Add(new GeneratedUClass(reflectedClass));
            SortDependencies(ref uClasses);

            Console.WriteLine($"\nReflected {reflectedClasses.Count} classes: " + string.Join(", ", uClasses.Select(c => c.Name)));
            Console.WriteLine($"\nReflected {reflectedStatusNames.Count} statuses: " + string.Join(", ", reflectedStatusNames));
            Console.WriteLine(Environment.NewLine + $@"Press any key to generate API file {GENERATED_DIRECTORY_PATH}\{GENERATED_FILE_NAME}");
            Console.ReadKey();

            var generatedString = File.ReadAllText($@"{TEMPLATES_DIRECTORY_PATH}\Header.txt") + Environment.NewLine;
            generatedString += new GeneratedStatuses(reflectedStatusNames).GenerateApi() + Environment.NewLine;
            foreach (var uClass in uClasses) generatedString += uClass.GenerateApi() + Environment.NewLine;

            var generatedDir = Directory.CreateDirectory(GENERATED_DIRECTORY_PATH);
            File.WriteAllText($@"{generatedDir.FullName}\{GENERATED_FILE_NAME}", generatedString);

            Console.WriteLine("\nAPI file successfully generated. Press any key to open directory with the generated file and exit...");
            Console.ReadKey();

            Process.Start(GENERATED_DIRECTORY_PATH);
        }

        private static void SortDependencies (ref List<GeneratedUClass> outClasses)
        {
            var uClasses = outClasses;

            bool isDependenciesSorted = false;
            while (!isDependenciesSorted)
            {
                isDependenciesSorted = true;
                for (int i = 0; i < uClasses.Count - 1; i++)
                {
                    // If the current class is dependent on any of the classes declared after it.
                    var dependentClass = uClasses.GetRange(i + 1, uClasses.Count - i - 1).Find(c => uClasses[i].DependsOn(c));
                    if (dependentClass != null)
                    {
                        isDependenciesSorted = false;
                        var dependentClassIndex = uClasses.IndexOf(dependentClass);
                        var tmp = uClasses[i];
                        uClasses[i] = uClasses[dependentClassIndex];
                        uClasses[dependentClassIndex] = tmp;
                        break;
                    }
                }
            }

            outClasses = uClasses;
        }
    }
}
