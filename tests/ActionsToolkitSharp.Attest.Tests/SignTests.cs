// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ActionsToolkitSharp.Attest.Tests;

/// <summary>
/// Mirrors the upstream <c>__tests__/sign.test.ts</c> suite. Heavyweight
/// cryptographic flows (Fulcio/Rekor/TSA round-trips) are covered by the
/// upstream <c>Sigstore</c> library's own tests; these tests cover the
/// <see cref="SigstoreSignerFactory.ExtractLeafCertificatePem"/> wiring and
/// the wiring of the abstraction-level <see cref="IAttestSigner"/>.
/// </summary>
public class SignTests
{
    [Fact(DisplayName = "extracts the leaf certificate from a v0.3 bundle (verificationMaterial.certificate.rawBytes)")]
    public void ExtractsCertificateFromV03Bundle()
    {
        var (raw, pem) = MintSelfSignedCertificate();
        var bundle = JsonNode.Parse($$"""
        {
          "verificationMaterial": {
            "certificate": { "rawBytes": "{{Convert.ToBase64String(raw)}}" }
          }
        }
        """)!;

        var actualPem = SigstoreSignerFactory.ExtractLeafCertificatePem(bundle);
        Assert.Equal(NormalizePem(pem), NormalizePem(actualPem));
    }

    [Fact(DisplayName = "extracts the leaf certificate from a v0.1/v0.2 bundle (verificationMaterial.x509CertificateChain.certificates[0])")]
    public void ExtractsCertificateFromV01Bundle()
    {
        var (raw, pem) = MintSelfSignedCertificate();
        var bundle = JsonNode.Parse($$"""
        {
          "verificationMaterial": {
            "x509CertificateChain": {
              "certificates": [
                { "rawBytes": "{{Convert.ToBase64String(raw)}}" }
              ]
            }
          }
        }
        """)!;

        var actualPem = SigstoreSignerFactory.ExtractLeafCertificatePem(bundle);
        Assert.Equal(NormalizePem(pem), NormalizePem(actualPem));
    }

    [Fact(DisplayName = "throws when the bundle has no verificationMaterial")]
    public void ThrowsOnMissingVerificationMaterial()
    {
        var bundle = JsonNode.Parse("""{ "mediaType": "x" }""")!;

        Assert.Throws<InvalidOperationException>(() =>
            SigstoreSignerFactory.ExtractLeafCertificatePem(bundle));
    }

    [Fact(DisplayName = "throws when neither certificate shape is present")]
    public void ThrowsWhenNoCertificateShapePresent()
    {
        var bundle = JsonNode.Parse("""
        { "verificationMaterial": { "tlogEntries": [] } }
        """)!;

        Assert.Throws<InvalidOperationException>(() =>
            SigstoreSignerFactory.ExtractLeafCertificatePem(bundle));
    }

    private static (byte[] DerBytes, string Pem) MintSelfSignedCertificate()
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "CN=ats-attest-test",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        var notBefore = DateTimeOffset.UtcNow.AddMinutes(-1);
        var notAfter = notBefore.AddDays(1);
        using var cert = req.CreateSelfSigned(notBefore, notAfter);
        return (cert.Export(X509ContentType.Cert), cert.ExportCertificatePem());
    }

    private static string NormalizePem(string s) =>
        s.Replace("\r", string.Empty, StringComparison.Ordinal).Trim();
}
