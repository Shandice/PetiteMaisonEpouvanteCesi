using System.ComponentModel.DataAnnotations;

namespace PetiteMaisonEpouvante.Core;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Le nom du produit est obligatoire.")]
    [StringLength(200, ErrorMessage = "Le nom ne doit pas dépasser 200 caractères.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La description est obligatoire.")]
    public string Description { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    [Range(0.01, 100000, ErrorMessage = "Le prix doit être strictement positif.")]
    public decimal Price { get; set; }

    [Range(0, 500, ErrorMessage = "Les frais de port ne peuvent pas être négatifs.")]
    public decimal ShippingCost { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public string SellerId { get; set; } = string.Empty;
    public int Stock { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Le nom de la catégorie est obligatoire.")]
    [StringLength(100, ErrorMessage = "Le nom ne doit pas dépasser 100 caractères.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public List<Product> Products { get; set; } = new();
}

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    [Range(0.01, int.MaxValue)]
    public decimal TotalPrice { get; set; }
    
    public List<OrderItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public class ExchangePost
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    
    public ExchangeType Type { get; set; } = ExchangeType.Exchange;
    public List<string> Tags { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum ExchangeType
{
    Exchange,
    Give,
    Wish
}

public class Magazine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public int IssueNumber { get; set; }
    
    public string? Title { get; set; }
    public string? Description { get; set; }
    
    [Range(0.01, 100)]
    public decimal Price { get; set; }
    
    public string? CoverImageUrl { get; set; }
    public string? PdfUrl { get; set; }
    
    public DateTime PublicationDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Subscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    
    public SubscriptionType Type { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AutoRenew { get; set; } = true;
}

public enum SubscriptionType
{
    DigitalOnly,
    PaperOnly,
    PaperAndDigital
}

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedProductId { get; set; }
    
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum NotificationType
{
    ProductRecommendation,
    ExchangeMatch,
    OrderUpdate,
    NewMagazine,
    SubscriptionRenewal,
    SystemMessage
}

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SenderId { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
}

public class Article
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public string? AuthorId { get; set; }
    public string? FeaturedImageUrl { get; set; }
    
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublished { get; set; } = false;
}