using Mustache;
using System.IO;

namespace ApiGenerator
{
    public abstract class Generated
    {
        public abstract string TemplateName { get; }

        public string GenerateApi ()
        {
            var templateString = File.ReadAllText($@"{Program.TEMPLATES_DIRECTORY_PATH}\{TemplateName}.txt");
            var compiler = new FormatCompiler();
            var generator = compiler.Compile(templateString);

            return generator.Render(this);
        }
    }
}
