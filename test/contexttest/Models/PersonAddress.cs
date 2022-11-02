using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

//1-to-1 relationship with Person
public class PersonAddress
{
    [Key, ForeignKey("Person")]
    public int Id { get; set; }
    public string? Address1 {get; set;}
    public string? Address2 {get; set;}
    public string? City {get; set;}
    public string? Zip {get; set;}
    public string? Phone {get; set;}
    public string? Email {get; set;}
    //Navigation property Returns the Person
    public virtual Person? Person {get; set;}
}