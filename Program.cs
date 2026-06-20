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

namespace ImageAverageFilterSkiaSharp;

class Program
{
    static void Main(string[] args)
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        string inputFolder = Path.Combine(desktop, "Aniversário_3_anos_Maria Alice\\Tratadas\\teste");
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

            Console.WriteLine($"{bitmap.Width} x {bitmap.Height}");

            using SKBitmap resultado = CorrigirManchaSensor(
                                        bitmap,
                                        centroX: 1488,
                                        centroY: 780,
                                        raio: 18);

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

    static SKBitmap CorrigirManchaSensorEscalado(SKBitmap original)
    {
        double xRel = 492.0 / 992.0;
        double yRel = 258.0 / 1488.0;

        int x = (int)(original.Width * xRel);
        int y = (int)(original.Height * yRel);

        int raio = Math.Max(10, original.Width / 600);

        return CorrigirManchaSensor(
            original,
            x,
            y,
            raio);
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
    static SKBitmap CorrigirManchaSensor(
    SKBitmap original,
    int centroX,
    int centroY,
    int raio)
    {
        int largura = original.Width;
        int altura = original.Height;

        SKBitmap nova = new SKBitmap(largura, altura);

        for (int y = 0; y < altura; y++)
        {
            for (int x = 0; x < largura; x++)
            {
                nova.SetPixel(x, y, original.GetPixel(x, y));
            }
        }

        List<byte> reds = new();
        List<byte> greens = new();
        List<byte> blues = new();

        // coleta pixels ao redor da mancha
        for (int y = centroY - raio * 3; y <= centroY + raio * 3; y++)
        {
            for (int x = centroX - raio * 3; x <= centroX + raio * 3; x++)
            {
                if (x < 0 || x >= largura ||
                    y < 0 || y >= altura)
                    continue;

                double dist = Math.Sqrt(
                    (x - centroX) * (x - centroX) +
                    (y - centroY) * (y - centroY));

                if (dist < raio + 5)
                    continue;

                if (dist > raio * 3)
                    continue;

                var cor = original.GetPixel(x, y);

                reds.Add(cor.Red);
                greens.Add(cor.Green);
                blues.Add(cor.Blue);
            }
        }

        reds.Sort();
        greens.Sort();
        blues.Sort();

        byte r = reds[reds.Count / 2];
        byte g = greens[greens.Count / 2];
        byte b = blues[blues.Count / 2];

        // preenche a mancha
        for (int y = centroY - raio; y <= centroY + raio; y++)
        {
            for (int x = centroX - raio; x <= centroX + raio; x++)
            {
                if (x < 0 || x >= largura ||
                    y < 0 || y >= altura)
                    continue;

                double dist = Math.Sqrt(
                    (x - centroX) * (x - centroX) +
                    (y - centroY) * (y - centroY));

                if (dist <= raio)
                {
                    nova.SetPixel(
                        x,
                        y,
                        new SKColor(r, g, b));
                }
            }
        }

        return nova;
    }
}
