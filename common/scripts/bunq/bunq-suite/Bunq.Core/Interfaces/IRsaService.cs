using Bunq.Core.Models;

namespace Bunq.Core.Interfaces;

public interface IRsaService
{
    KeyPair GenerateKeyPair();
    string SignData(string data, string privateKeyPem);
    bool VerifyData(string data, string signatureBase64, string publicKeyPem);
}
