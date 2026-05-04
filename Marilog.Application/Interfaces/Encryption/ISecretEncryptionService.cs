using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.Interfaces.Encryption
{
    public interface ISecretEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
