using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public class PersonWithoutKey
{
    public int id { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
}