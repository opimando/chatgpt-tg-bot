using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bot.DbModel.Models;

[Table("User", Schema = "Gpt")]
public class User
{
    [Key] public string Id { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public bool HasAccess { get; set; }
    public BotAnswerType AnswerType { get; set; } = BotAnswerType.ByPart;
    public UserType UserType { get; set; } = UserType.User;
    public DateTime CreateTime { get; set; }
    public DateTime LastAccessTime { get; set; }
}