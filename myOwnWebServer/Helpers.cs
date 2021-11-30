using System.IO;

namespace myOwnWebServer
{
    public static class Helpers
    {
        public static string CreatePath(this string path, string url)
        {
            return Path.Combine(path, url);
        }
    }
}