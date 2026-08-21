using Microsoft.AspNetCore.Hosting;

namespace BabyToddlerEssentials.Services
{
    public interface IImageService
    {
        Task<string> SaveAsync(IFormFile file, string subFolder = "products");
        void Delete(string? webPath);
        bool IsValidImage(IFormFile file);
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;

        // Allowed image types (extension + real file signature)
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private static readonly string[] AllowedContentTypes =
            { "image/jpeg", "image/png", "image/gif", "image/webp" };

        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0 || file.Length > MaxBytes)
                return false;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return false;

            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            // Check the real bytes (magic numbers), so a renamed .exe can't sneak in
            return HasImageSignature(file);
        }

        public async Task<string> SaveAsync(IFormFile file, string subFolder = "products")
        {
            if (!IsValidImage(file))
                throw new InvalidOperationException("Only image files are allowed (jpg, png, gif, webp, max 5 MB).");

            // wwwroot/images/{subFolder}
            var folder = Path.Combine(_env.WebRootPath, "images", subFolder);
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Web-accessible path stored in the DB
            return $"/images/{subFolder}/{fileName}";
        }

        public void Delete(string? webPath)
        {
            if (string.IsNullOrWhiteSpace(webPath))
                return;

            var relative = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, relative);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        // Reads the first bytes and confirms they match a known image format
        private static bool HasImageSignature(IFormFile file)
        {
            try
            {
                using var reader = new BinaryReader(file.OpenReadStream());
                var bytes = reader.ReadBytes(12);
                file.OpenReadStream().Position = 0; // reset for later saving

                // JPEG: FF D8 FF
                if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                    return true;

                // PNG: 89 50 4E 47
                if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                    return true;

                // GIF: 47 49 46 38  ("GIF8")
                if (bytes.Length >= 4 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
                    return true;

                // WEBP: "RIFF"...."WEBP"
                if (bytes.Length >= 12 &&
                    bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                    bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}