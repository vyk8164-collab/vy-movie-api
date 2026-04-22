using System.Text.RegularExpressions;

namespace ConnectDB.Utils
{
    public static class YouTubeHelper
    {
        // 🔥 Convert link → embed
        public static string? ToEmbedUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            var videoId = GetVideoId(url);
            if (videoId == null) return null;

            return $"https://www.youtube.com/embed/{videoId}";
        }

        // 🔥 LẤY VIDEO ID (QUAN TRỌNG)
        public static string? GetVideoId(string? url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            var regex = new Regex(
                @"(?:youtube\.com\/(?:.*v=|embed\/)|youtu\.be\/)([^&""'>]+)",
                RegexOptions.IgnoreCase
            );

            var match = regex.Match(url);

            return match.Success ? match.Groups[1].Value : null;
        }
    }
}