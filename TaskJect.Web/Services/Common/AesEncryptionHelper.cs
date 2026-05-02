using TaskJect.Web.Common;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace TaskJect.Web.Services
{
    public class AesEncryptionHelper
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public AesEncryptionHelper(IOptions<EncryptionOptions> options)
        {
            _key = Convert.FromBase64String(options.Value.Key);
            _iv = Convert.FromBase64String(options.Value.IV);
        }

        public string Encrypt(Guid guid)
        {
            var plainText = guid.ToString();
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        public Guid Decrypt(string encrypted)
        {
            var base64 = encrypted.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            var buffer = Convert.FromBase64String(base64);
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            var plainText = sr.ReadToEnd();
            return Guid.Parse(plainText);
        }
    }

}
