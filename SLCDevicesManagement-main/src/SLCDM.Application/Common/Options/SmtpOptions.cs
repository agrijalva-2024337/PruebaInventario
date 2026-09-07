namespace SLCDM.Application.Common.Options;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public bool Enabled { get; set; }

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string FromName { get; set; } = "SLC Devices Management";

    public bool UseStartTls { get; set; } = true;

    public string PublicAppUrl { get; set; } = "http://localhost:5173";
}

public sealed class BrandingOptions
{
    public const string SectionName = "Branding";

    public string LetterheadPath { get; set; } = string.Empty;
}
