namespace Models;

//1-to-Many relationship with Person, as in a Person can own many books
public class Book
{
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual Person? OwnedBy {get; set;}
}