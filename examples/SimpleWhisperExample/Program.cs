// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FFMpegCore;
using Whisper.net;
using Whisper.net.Ggml;

public class Program
{
    public static async Task Main(string[] args)
    {
        var ggmlType = GgmlType.Base;
        var modelFileName = "ggml-small.en.bin";
        var wavFileName = "kennedy.wav";

        if (!File.Exists(modelFileName))
        {
            await DownloadModel(modelFileName, ggmlType);
        }

        //using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");
        using var whisperFactory = WhisperFactory.FromPath("ggml-small.en.bin");

        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("en")
            .Build();

        FFMpeg.ExtractAudio("F:\\Downloads\\Video\\TestWhisperCplus\\20220822020254000.mp4", "F:\\Downloads\\Video\\TestWhisperCplus\\20220822020254000.mp3");
        FFMpegArguments
    .FromFileInput("F:\\Downloads\\Video\\TestWhisperCplus\\20220822020254000.mp3")
    .OutputToFile("F:\\Downloads\\Video\\TestWhisperCplus\\20220822020254000.wav", true, options =>
        options.WithAudioSamplingRate(16000))
    .ProcessSynchronously();
        wavFileName = "F:\\Downloads\\Video\\TestWhisperCplus\\20220822020254000.wav";
        using var fileStream = File.OpenRead(wavFileName);
        bool firstLine = true;
        await foreach (var result in processor.ProcessAsync(fileStream))
        {
            var vttTimeStart = TimeSpanToVttTime(result.Start);
            var vttTimeEnd = TimeSpanToVttTime(result.End);
            using var writer = new StreamWriter("F:\\Downloads\\Video\\TestWhisperCplus\\20220822020254000.vtt",true);
            if(firstLine) {
                writer.WriteLine("WEBVTT");
                writer.WriteLine($"");
            }
            writer.WriteLine($"{result.Start:mm\\:ss\\.fff} --> {result.End:mm\\:ss\\.fff}");
            //writer.WriteLine($"{vttTimeStart} --> {vttTimeEnd}");
            writer.WriteLine($"{result.Text}");
            writer.WriteLine($"");
            //Console.WriteLine($"{result.Start}-->{result.End}: {result.Text}");
            firstLine = false;
        }
    }

    private static async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        Console.WriteLine($"Downloading Model {fileName}");
        using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
        using var fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
    }

    static string TimeSpanToVttTime(TimeSpan timeSpan)
    {
        int milliseconds = (int)timeSpan.TotalMilliseconds;
        int hours = timeSpan.Hours;
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;

        string vttTime = $"{hours:D2}:{minutes:D2}:{seconds:D2},{milliseconds:D3}";

        return vttTime;
    }
}
