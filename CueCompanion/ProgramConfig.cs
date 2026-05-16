using YamlDotNet.Serialization;

namespace CueCompanion;

public sealed class ProgramConfig
{
    [YamlMember(Alias = "urls")]
    public string[] Urls { get; set; } = [];

    [YamlMember(Alias = "certificatePath")]
    public string? CertificatePath { get; set; }

    [YamlMember(Alias = "certificatePassword")]
    public string? CertificatePassword { get; set; }

    [YamlMember(Alias = "databasePath")]
    public string DatabasePath { get; set; } = "data.db";
}