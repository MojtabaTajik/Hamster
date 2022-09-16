using hamster.DTO;
using hamster.Model;
using hamster.Utils;

namespace hamster.Configuration;

public class AutoMapping : AutoMapper.Profile
{
    public AutoMapping()
    {
        CreateMap<ConfigFile, HamsterConfigDto>();
        CreateMap<BackupOperation, BackupOperationDto>()
            .ForMember(m => m.Command,
                opt => opt.MapFrom(src => 
                    (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    ? TranslateCommand(src.WindowsCommand, src.Name)
                    : TranslateCommand(src.UnixCommand, src.Name)))
            .ForMember(mem => mem.RemoteFileName,
                opt => opt.MapFrom(src =>
                    TranslateVariable(src.RemoteFileName, src.Name, src.PersianDate)))
            .ForMember(mem => mem.BucketName,
                opt => opt.MapFrom(src =>
                    TranslateVariable(src.BucketName, src.Name, src.PersianDate)))
            .ForMember(m => m.BucketName, opt =>
                opt.MapFrom(f => f.BucketName.ToLower()));
    }
    
    private string TranslateVariable(string str, string name, bool persianDate)
    {
        string date = persianDate ? PersianDateUtils.Now() : DateTime.Now.ToString("yyyy.dd.MM-HH.mm.ss");

        return str
            .Replace("$date", date, StringComparison.OrdinalIgnoreCase)
            .Replace("$name", name, StringComparison.OrdinalIgnoreCase);
    }
    
    private string TranslateCommand(string command, string name)
    {
        return command    
            .Replace("$Name", PathUtils.BuildBackupDir(name), StringComparison.OrdinalIgnoreCase)
            .Replace("\"", "\\\"");
    }
}