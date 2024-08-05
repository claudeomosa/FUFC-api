using System.ComponentModel.DataAnnotations;

namespace FUFC.Shared.Models;

public class Referee
{
    [Key]
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}