using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;

namespace HandbrakeMock
{
    public class HandbrakeMock
    {
        public static void Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: true);

            app.HelpOption("-?|-h|--help");

            var inputPathOption = app.Option("-i|--input", "The input path",
                CommandOptionType.SingleValue);
            var outputPathOption = app.Option("-o|--output", "The output path",
                CommandOptionType.SingleValue);


            app.OnExecute(() =>
            {
                var inputPath = inputPathOption.Value();
                var outputPath = outputPathOption.Value();

                if (!Path.IsPathRooted(inputPath))
                {
                    inputPath = Path.Combine(Directory.GetCurrentDirectory(), inputPath);
                }
                if (!Path.IsPathRooted(outputPath))
                {
                    outputPath = Path.Combine(Directory.GetCurrentDirectory(), outputPath);
                }

                if (!File.Exists(inputPath))
                {
                    throw new ArgumentException("The input path is invalid");
                }

                var input = new FileInfo(inputPath);
                using (var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    var outputSize = (int) input.Length / 2;
                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Seek(outputSize - 2, SeekOrigin.Begin);
                        writer.Write((byte)0);
                    }
                }

                return 0;
            });

            app.Execute(args);
        }
    }
}
