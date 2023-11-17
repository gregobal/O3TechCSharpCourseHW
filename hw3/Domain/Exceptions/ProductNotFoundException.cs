namespace Domain.Exceptions;

public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(int id) :
        base($"Product with Id = {id} not found.")
    {
    }
}