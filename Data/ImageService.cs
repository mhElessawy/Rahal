using static System.Net.Mime.MediaTypeNames;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg; // Add format support

namespace RahalWeb.Data
{
    public class ImageService
    {
        public async Task ResizeAndSaveImage(IFormFile file, string outputPath, int width, int height)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");

            using var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream());
            image.Mutate(x => x.Resize(width, height));

            // Save the resized image
            await image.SaveAsync(outputPath, new JpegEncoder()); // Change encoder if needed (e.g., PngEncoder)
        }
    }
}
