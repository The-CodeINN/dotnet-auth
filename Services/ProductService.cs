using dotnet_auth.Models;
using dotnet_auth.Repository;

namespace dotnet_auth.Services;

public interface IProductService
{
    Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(int page = 1, int pageSize = 10, string category = null);
    Task<IEnumerable<ProductResponseDto>> GetUserProductsAsync(int userId, int page = 1, int pageSize = 10, string category = null);
    Task<ProductResponseDto> GetProductByIdAsync(int id);
    Task<ProductResponseDto> CreateProductAsync(CreateProductDto createProductDto, int userId);
    Task<ProductResponseDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto, int userId);
    Task DeleteProductAsync(int id, int userId);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(int page = 1, int pageSize = 10, string category = null)
    {
        var products = await _productRepository.GetAllProductsAsync(page, pageSize, category);
        return products.Select(MapToProductResponseDto);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetUserProductsAsync(int userId, int page = 1, int pageSize = 10, string category = null)
    {
        var products = await _productRepository.GetUserProductsAsync(userId, page, pageSize, category);
        return products.Select(MapToProductResponseDto);
    }

    public async Task<ProductResponseDto> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);
        return product != null ? MapToProductResponseDto(product) : null;
    }

    public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto createProductDto, int userId)
    {
        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            Category = createProductDto.Category,
            UserId = userId
        };

        var createdProduct = await _productRepository.CreateProductAsync(product);
        return MapToProductResponseDto(createdProduct);
    }

    public async Task<ProductResponseDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto, int userId)
    {
        var existingProduct = await _productRepository.GetProductByIdAsync(id);

        if (existingProduct == null || existingProduct.UserId != userId)
        {
            return null;
        }

        existingProduct.Name = updateProductDto.Name;
        existingProduct.Description = updateProductDto.Description;
        existingProduct.Price = updateProductDto.Price;
        existingProduct.Category = updateProductDto.Category;

        await _productRepository.UpdateProductAsync(existingProduct);
        return MapToProductResponseDto(existingProduct);
    }

    public async Task DeleteProductAsync(int id, int userId)
    {
        var product = await _productRepository.GetProductByIdAsync(id);

        if (product != null && product.UserId == userId)
        {
            await _productRepository.DeleteProductAsync(id);
        }
    }

    private static ProductResponseDto MapToProductResponseDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            UserId = product.UserId
        };
    }
}