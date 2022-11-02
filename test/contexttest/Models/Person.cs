using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public class Person : System.IEquatable<Person>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }

    //Navigation property Returns the Person Address
    public virtual PersonAddress? Address { get; set; }

    public virtual IList<Book>? Books { get; set; }

    public bool Equals(Person? other)
    {
        if (Name == other?.Name && Surname == other?.Surname) return true;
        return false;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        return Equals((Person)obj);
    }
}