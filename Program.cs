// Projeto: ImageAverageFilterSkiaSharp
// Framework: .NET 10
//
// Instalação:
// dotnet add package SkiaSharp
//
// O que este projeto faz:
// - Lê todas as imagens da pasta Desktop/imagens
// - Percorre pixel por pixel
// - Calcula a média RGB da vizinhança 3x3
// - Define o novo valor do pixel central
// - Salva em Desktop/processadas

using SkiaSharp;
using System.Xml.Linq;

namespace ImageAverageFilterSkiaSharp;

class Program
{
    static void Main(string[] args)
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        string inputFolder = Path.Combine(desktop, "imagens");
        string outputFolder = Path.Combine(desktop, "processadas");

        Directory.CreateDirectory(outputFolder);

        if (!Directory.Exists(inputFolder))
        {
            Console.WriteLine($"Pasta não encontrada: {inputFolder}");
            return;
        }

        string[] arquivos = Directory.GetFiles(inputFolder)
            .Where(f =>
                f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".nef", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (arquivos.Length == 0)
        {
            Console.WriteLine("Nenhuma imagem encontrada.");
            return;
        }

        foreach (string arquivo in arquivos)
        {
            Console.WriteLine($"Processando: {Path.GetFileName(arquivo)}");

            using SKBitmap bitmap = SKBitmap.Decode(arquivo);

            using SKBitmap resultado = AplicarFiltroMedia(bitmap);

            string nomeArquivo = Path.GetFileNameWithoutExtension(arquivo);
            string extensao = Path.GetExtension(arquivo);

            string outputPath = Path.Combine(
                outputFolder,
                $"{nomeArquivo}_media{extensao}"
            );

            SalvarBitmap(resultado, outputPath, extensao);

            Console.WriteLine($"Imagem salva: {outputPath}");
        }

        Console.WriteLine("Processamento concluído.");
    }

    static SKBitmap AplicarFiltroMedia(SKBitmap original)
    {
        int largura = original.Width;
        int altura = original.Height;

        SKBitmap novaImagem = new SKBitmap(largura, altura);

        // Copia bordas originais
        for (int y = 0; y < altura; y++)
        {
            for (int x = 0; x < largura; x++)
            {
                novaImagem.SetPixel(x, y, original.GetPixel(x, y));
            }
        }

        // Percorre região interna
        for (int y = 1; y < altura - 1; y++)
        {
            for (int x = 1; x < largura - 1; x++)
            {
                int somaR = 0;
                int somaG = 0;
                int somaB = 0;

                // Região 3x3
                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        SKColor vizinho = original.GetPixel(x + kx, y + ky);

                        somaR += vizinho.Red;
                        somaG += vizinho.Green;
                        somaB += vizinho.Blue;
                    }
                }

                byte mediaR = (byte)(somaR / 9);
                byte mediaG = (byte)(somaG / 9);
                byte mediaB = (byte)(somaB / 9);

                SKColor novaCor = new SKColor(
                    mediaR,
                    mediaG,
                    mediaB
                );

                novaImagem.SetPixel(x, y, novaCor);
            }
        }

        return novaImagem;
    }

    static void SalvarBitmap(SKBitmap bitmap, string caminho, string extensao)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);

        SKEncodedImageFormat formato = extensao.ToLower() switch
        {
            ".png" => SKEncodedImageFormat.Png,
            ".bmp" => SKEncodedImageFormat.Bmp,
            _ => SKEncodedImageFormat.Jpeg
        };

        using SKData data = image.Encode(formato, 100);

        using FileStream stream = File.OpenWrite(caminho);

        data.SaveTo(stream);
    }
}