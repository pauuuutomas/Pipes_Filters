using System;
using System.Drawing;
using System.IO;
using CompAndDel.Pipes;
using CompAndDel.Filters;
using Ucu.Poo.Cognitive;
using Ucu.Poo.Twitter;

namespace CompAndDel
{
    class Program
    {
        static void Main(string[] args)
        {
            // Ejercicio 1
            PictureProvider provider = new PictureProvider();
            IPicture pictureLuke = provider.GetPicture("luke.jpg");

            PipeNull pipeNull = new PipeNull();
            PipeSerial pipeSerialGrayScale = new PipeSerial(new FilterGreyscale(), pipeNull);
            PipeSerial pipeSerialFinal = new PipeSerial(new FilterNegative(), pipeSerialGrayScale);
            
            IPicture resultFinal = pipeSerialFinal.Send(pictureLuke);
            provider.SavePicture(resultFinal, @"lukeFinal.jpg");

            // Ejercicio 2
            IPicture resultPipeSerial1 = pipeSerialGrayScale.Send(pictureLuke);
            provider.SavePicture(resultPipeSerial1, @"lukePipeSerial1.jpg");

            IPicture resultPipeSerial2 = pipeSerialFinal.Send(pictureLuke);
            provider.SavePicture(resultPipeSerial2, @"lukePipeSerial2.jpg");

            // Ejercicio 3
            var twitter = new TwitterImage();
            Console.WriteLine(twitter.PublishToTwitter("Luke Filter Greyscale", @"lukePipeSerial1.jpg"));
            Console.WriteLine(twitter.PublishToTwitter("Luke Filter Greyscale + FilterNegative", @"lukePipeSerial2.jpg"));

            // Ejercicio 4
            FilterConditional("luke.jpg"); // (Encuentra una cara) Le aplica un filtro de GrayScale y BlurConvolution. La guarda en PC. Posteriormente la sube a Twitter.
            FilterConditional("beer.jpg"); // (No encuentra una cara) Le aplica un filtro de GrayScale y Negative. La guarda en PC.
        }

        static void FilterConditional(string picturePath)
        {
            string pictureName = Path.GetFileNameWithoutExtension(picturePath);
            
            PictureProvider provider = new PictureProvider();
            IPicture pictureLuke = provider.GetPicture(picturePath);
            
            PipeNull pipeNull = new PipeNull();
            PipeSerial pipeSerialGrayScale = new PipeSerial(new FilterGreyscale(), pipeNull);
            IPicture pictureLukeGrayScale = pipeSerialGrayScale.Send(pictureLuke);
            
            provider.SavePicture(pictureLukeGrayScale, $"{pictureName}GrayScale.jpg");
            
            CognitiveFace cog = new CognitiveFace(true, Color.GreenYellow);
            cog.Recognize($"{pictureName}GrayScale.jpg");
            if (cog.FaceFound)
            {
                PipeSerial pipeSerialBlurConvolution = new PipeSerial(new FilterBlurConvolution(), pipeSerialGrayScale);
                IPicture resultBlurConvolution = pipeSerialBlurConvolution.Send(pictureLukeGrayScale);
                provider.SavePicture(resultBlurConvolution, $"{pictureName}BlurConvolution.jpg");
                
                var twitter = new TwitterImage();
                Console.WriteLine(twitter.PublishToTwitter($"{pictureName} Filter GrayScale + BlurConvolution With Conditional Pipe With Bifurcation", $"{pictureName}BlurConvolution.jpg"));
            }
            else
            {
                PipeSerial pipeSerialNegative = new PipeSerial(new FilterNegative(), pipeSerialGrayScale);
                IPicture pictureLukeNegative = pipeSerialNegative.Send(pictureLukeGrayScale);
                provider.SavePicture(pictureLukeNegative, $"{pictureName}Negative.jpg");
            }
        }
    }
}
