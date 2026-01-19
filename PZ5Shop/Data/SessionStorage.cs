using System;
using System.IO;
using System.Text;

namespace PZ5Shop.Data
{
    public class SessionStorage
    {
        private readonly string _filePath;

        public SessionStorage()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PZ5Shop");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            _filePath = Path.Combine(folder, "session.json");
        }

        public int? LoadUserId()
        {
            if (!File.Exists(_filePath))
            {
                return null;
            }

            var content = File.ReadAllText(_filePath, Encoding.UTF8).Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            var key = "\"UserId\":";
            var index = content.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return null;
            }

            var start = index + key.Length;
            var end = start;
            while (end < content.Length && char.IsDigit(content[end]))
            {
                end++;
            }

            var number = content.Substring(start, end - start);
            if (int.TryParse(number, out var userId))
            {
                return userId > 0 ? userId : (int?)null;
            }

            return null;
        }

        public void SaveUserId(int? userId)
        {
            if (userId == null || userId.Value <= 0)
            {
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                }
                return;
            }

            var content = "{\"UserId\":" + userId.Value + "}";
            File.WriteAllText(_filePath, content, Encoding.UTF8);
        }
    }
}
