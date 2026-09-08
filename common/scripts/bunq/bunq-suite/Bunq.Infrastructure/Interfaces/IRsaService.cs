using Bunq.Core.Models;

namespace Bunq.Infrastructure.Interfaces;

public interface IRsaService
{
    KeyPair GenerateKeyPair();
    string SignData(string data, string privateKeyPem);
}
