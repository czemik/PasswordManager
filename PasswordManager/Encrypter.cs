using Cryptography;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;

using System.Text;
using System.Threading.Tasks;

namespace PasswordManager
{
    internal class Encrypter
    {
        public static string Encrypt(string Key, string Secret)
        {
            using var hashing = SHA256.Create();
            byte[] keyHash = hashing.ComputeHash(Encoding.Unicode.GetBytes(Key));
            string key = Base64UrlEncoder.Encode(keyHash);
            string message = Base64UrlEncoder.Encode(Encoding.Unicode.GetBytes(Secret));
            return Fernet.Encrypt(key, message);
        }

        public static string Decrypt(string Key, string Secret)
        {
            using var hashing = SHA256.Create();
            byte[] keyHash = hashing.ComputeHash(Encoding.Unicode.GetBytes(Key));
            string key = Base64UrlEncoder.Encode(keyHash);
            string encodedSecret = Fernet.Decrypt(key, Secret);
            string message = Encoding.Unicode.GetString(Base64UrlEncoder.DecodeBytes(encodedSecret));
            return message;
        }
    }
}
