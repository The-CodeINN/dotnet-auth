using dotnet_auth.Models;
using dotnet_auth.Repository;

namespace dotnet_auth.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync(int page = 1, int pageSize = 10, string category = null);
    Task<IEnumerable<Product>> GetUserProductsAsync(int userId, int page = 1, int pageSize = 10, string category = null);
    Task<Product> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(Product product);
    Task<Product> UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync(int page = 1, int pageSize = 10, string category = null)
    {
        return await _productRepository.GetAllProductsAsync(page, pageSize, category);
    }

    public async Task<IEnumerable<Product>> GetUserProductsAsync(int userId, int page = 1, int pageSize = 10, string category = null)
    {
        return await _productRepository.GetUserProductsAsync(userId, page, pageSize, category);
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await _productRepository.GetProductByIdAsync(id);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        return await _productRepository.CreateProductAsync(product);
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        await _productRepository.UpdateProductAsync(product);
        return await _productRepository.GetProductByIdAsync(product.Id);
    }

    public async Task DeleteProductAsync(int id)
    {
        await _productRepository.DeleteProductAsync(id);
    }
}