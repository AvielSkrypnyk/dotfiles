using System.Security.Cryptography;
using System.Text;
using Bunq.Core.Models;
using Bunq.Infrastructure.Interfaces;

namespace Bunq.Infrastructure.Security;

public sealed class RsaService : IRsaService
{
    private const int RsaKeySizeBits = 2048;
    public KeyPair GenerateKeyPair()
    {
        using var rsa = RSA.Create(RsaKeySizeBits);

        var publicPem = rsa.ExportSubjectPublicKeyInfoPem();
        var privatePem = rsa.ExportPkcs8PrivateKeyPem();
        
        return new KeyPair(publicPem, privatePem);
    }
    
    public string SignData(string data, string privateKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        
        var dataBytes = Encoding.UTF8.GetBytes(data);
        var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        
        return Convert.ToBase64String(signature);
    }
}
