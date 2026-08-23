namespace WerkPilot.Application.Billing;
public sealed record DocumentArchiveResult(
    string PdfPath,
    string Sha256,
    string ManifestPath);
