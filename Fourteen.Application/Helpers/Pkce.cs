using System.Security.Cryptography;
using System.Text;
public static class PkceHelper
{
   public static (string CodeVerifier, string CodeChallenge) GeneratePkce()
   {
       var bytes = new byte[32];
       using (var rng = RandomNumberGenerator.Create())
       {
           rng.GetBytes(bytes);
       }
       var codeVerifier = Base64UrlEncode(bytes);

       using (var sha256 = SHA256.Create())
       {
           var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
           var codeChallenge = Base64UrlEncode(hash);
           return (codeVerifier, codeChallenge);
       }
   }
   private static string Base64UrlEncode(byte[] input)
   {
       return Convert.ToBase64String(input)
           .Replace("+", "-")
           .Replace("/", "_")
           .Replace("=", "");
   }
}