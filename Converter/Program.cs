using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using System;
using System.IO;

namespace Converter
{
    class Arguments
    {
        public string InputFilename { get; set; }
        public string OutputType { get; set; }
        public int? Height { get; set; }
        public int Optimisation { get; set; }
    }

    class Program
    {
        static (Arguments arguments, bool cancelConverter) Parse(string[] args)
        {
            var parser = new CommandLineParser.Core.FluentCommandLineParser<Arguments>();
            parser.Setup(a => a.InputFilename).As('i', "input").WithDescription("input file - any square grayscale image file").Required();
            parser.Setup(a => a.OutputType).As('o', "output").WithDescription("output type - [df|distancefield]").Required();
            parser.Setup(a => a.Height).As('h', "height").WithDescription("height of the heightfield, by default 1/4th the width of the image");
            parser.Setup(a => a.Optimisation).As("optimize").WithDescription("optimize the performance. creates lower resolution distance field").SetDefault(0);
            parser.SetupHelp("?", "help").Callback(text => Console.WriteLine(text));
            var results = parser.Parse(args);
            if (results.HasErrors)
            {
                Console.WriteLine(results.ErrorText);
                return (null, true);
            }
            if (results.HelpCalled)
            {
                return (null, true);
            }
            return (parser.Object, false);
        }

        static ProgressBar Progress(string message) => new ProgressBar(100, message, new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.DarkYellow,
            ForegroundColorDone = ConsoleColor.DarkGreen,
            BackgroundColor = ConsoleColor.DarkGray,
            BackgroundCharacter = '░'
        });

        static void Main(string[] args)
        {
            var (arguments, cancelConverter) = Parse(args);
            if (cancelConverter)
            {
                return;
            }

            switch(arguments.OutputType)
            {
                case "distancefield":
                case "df":
                    CreateDistanceField(arguments);
                    break;
            }
        }

        private static void CreateDistanceField(Arguments arguments)
        {
            using (var image = Image.Load(arguments.InputFilename))
            {
                if (image.Width != image.Height)
                {
                    Console.WriteLine("image has to be square");
                    throw new Exception("image has to be square");
                }
                if (arguments.Optimisation > 0)
                {
                    var speedboostFactor = (int)Math.Pow(2, arguments.Optimisation);
                    image.Mutate(ctx => ctx.Resize(image.Width / speedboostFactor, image.Height / speedboostFactor));
                }
                var size = image.Width;
                var height = arguments.Height ?? size / 16;

                var heightmap = Array2D.Create(size, size, (x, y) => image[x, y].R / (float)byte.MaxValue);

                DistanceField distanceField;
                using (var progress = Progress("create volume"))
                {
                    distanceField = DistanceField.FromHeightmap(heightmap, height, percentage => progress.Tick());
                }

                var outname = Path.ChangeExtension(arguments.InputFilename, "distancefield");
                using (var file = File.Create(outname))
                {
                    distanceField.SaveTo(file);
                }
            }
        }
    }
}
