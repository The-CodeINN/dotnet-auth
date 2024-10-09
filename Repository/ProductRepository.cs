using dotnet_auth.Data;
using dotnet_auth.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_auth.Repository;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllProductsAsync(int page, int pageSize, string category);
    Task<IEnumerable<Product>> GetUserProductsAsync(int userId, int page, int pageSize, string category);
    Task<Product> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
}

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync(int page, int pageSize, string category)
    {
        var query = _context.Products
            .Include(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetUserProductsAsync(int userId, int page, int pageSize, string category)
    {
        var query = _context.Products
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Product with ID {id} not found.");

        return product;
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task UpdateProductAsync(Product product)
    {
        var existingProduct = await _context.Products
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == product.Id)
            ?? throw new KeyNotFoundException($"Product with ID {product.Id} not found.");

        // Only update allowed fields
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.Category = product.Category;

        _context.Products.Update(existingProduct);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id)
            ?? throw new KeyNotFoundException($"Product with ID {id} not found.");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}
