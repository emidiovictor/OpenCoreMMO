using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Engines;

public class RsaPemService
{
    public static byte[] PrivateKeyFromPem(byte[] data)
    {
       
            AsymmetricCipherKeyPair keyPair;

            using (var reader = File.OpenText(@"key.pem"))
            {
                keyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();

                var key = keyPair.Private;
                RsaEngine e = new RsaEngine();
                e.Init(false, key);

                return e.ProcessBlock(data, 0, data.Length);

            }
    }

   
}
