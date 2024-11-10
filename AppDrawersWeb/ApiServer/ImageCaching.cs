using AppDrawers.Shortcuts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace AppDrawers.ApiServer
{
    public static class ImageCaching
    {
        private static Dictionary<string, Image> ImageCache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public static Uri ConvertToDataUri(this Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "Image cannot be null.");
            }

            // Convert the image to a memory stream in the desired format
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);

                // Create the data URI string
                return new Uri($"data:{ContentTypes.Png};base64,{base64String}");
            }
        }

        public static Image GetImage(string iconFilePath, int iconIndex)
        {
            try
            {
                if (iconFilePath.Length < 1)
                {
                    return null;
                }

                var imageCacheKey = $"{iconFilePath}:{iconIndex}";

                if (ImageCache.ContainsKey(imageCacheKey))
                {
                    return ImageCache[imageCacheKey];
                }
                else
                {
                    var iconExtension = Path.GetExtension(iconFilePath).ToLower();

                    if (iconExtension.Equals(".png"))
                    {
                        var image = Image.FromFile(iconFilePath);

                        ImageCache.Add(imageCacheKey, image);

                        return image;
                    }
                    else
                    {
                        var icon = IconExtractor.GetIcon(iconFilePath.ExpandEnvironmentVariables(), iconIndex);

                        if (icon != null && icon.Height > 0 && icon.Width > 0)
                        {
                            var image = icon.ToBitmap();

                            ImageCache.Add(imageCacheKey, image);

                            return image;
                        }
                    }
                }
            }
            catch
            {
                // Ignore
            }

            return null;
        }
    }
}