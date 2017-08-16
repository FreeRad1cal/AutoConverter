using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Threading.Tasks;

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

            app.OnExecute(async () =>
            {
                var inputPath = inputPathOption.Value();
                var outputPath = outputPathOption.Value();
                if (inputPath == null || outputPath == null)
                {
                    app.ShowHelp();
                    return 1;
                }


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
                using (var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    var outputSize = (int) input.Length / 2;
                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Seek(outputSize - 1, SeekOrigin.Begin);
                        writer.Write((byte)0);
                        await Task.Delay(5000);
                    }
                }

                return 0;
            });

            app.Execute(args);
        }
    }
}
