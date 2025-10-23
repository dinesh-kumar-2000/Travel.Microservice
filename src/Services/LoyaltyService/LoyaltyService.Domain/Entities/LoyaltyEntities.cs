using SharedKernel.Models;

namespace LoyaltyService.Domain.Entities;

public class LoyaltyTier : TenantEntity<string>
{
    public string TierName { get; private set; } = string.Empty;
    public int MinPoints { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public Dictionary<string, object>? Benefits { get; private set; }

    private LoyaltyTier() { }

    public static LoyaltyTier Create(string id, string tenantId, string tierName, int minPoints, decimal discountPercentage, Dictionary<string, object>? benefits = null)
    {
        return new LoyaltyTier
        {
            Id = id,
            TenantId = tenantId,
            TierName = tierName,
            MinPoints = minPoints,
            DiscountPercentage = discountPercentage,
            Benefits = benefits,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateTier(string tierName, int minPoints, decimal discountPercentage, Dictionary<string, object>? benefits = null)
    {
        TierName = tierName;
        MinPoints = minPoints;
        DiscountPercentage = discountPercentage;
        Benefits = benefits;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class LoyaltyPoints : TenantEntity<string>
{
    public string UserId { get; private set; } = string.Empty;
    public int CurrentPoints { get; private set; }
    public int LifetimePoints { get; private set; }
    public string Tier { get; private set; } = "bronze";
    public int PointsExpiring { get; private set; }
    public DateTime? ExpiryDate { get; private set; }

    private LoyaltyPoints() { }

    public static LoyaltyPoints Create(string id, string tenantId, string userId)
    {
        return new LoyaltyPoints
        {
            Id = id,
            TenantId = tenantId,
            UserId = userId,
            CurrentPoints = 0,
            LifetimePoints = 0,
            Tier = "bronze",
            PointsExpiring = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddPoints(int points)
    {
        CurrentPoints += points;
        LifetimePoints += points;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeductPoints(int points)
    {
        CurrentPoints = Math.Max(0, CurrentPoints - points);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasSufficientPoints(int required)
    {
        return CurrentPoints >= required;
    }

    public void UpdateTier(string tier)
    {
        Tier = tier;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetExpiringPoints(int points, DateTime? expiryDate)
    {
        PointsExpiring = points;
        ExpiryDate = expiryDate;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class LoyaltyTransaction : TenantEntity<string>
{
    public string UserId { get; private set; } = string.Empty;
    public string TransactionType { get; private set; } = string.Empty;
    public int Points { get; private set; }
    public int BalanceAfter { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ReferenceType { get; private set; }
    public string? ReferenceId { get; private set; }

    private LoyaltyTransaction() { }

    public static LoyaltyTransaction Create(string id, string tenantId, string userId, string transactionType, int points, int balanceAfter, string description, string? referenceType = null, string? referenceId = null)
    {
        return new LoyaltyTransaction
        {
            Id = id,
            TenantId = tenantId,
            UserId = userId,
            TransactionType = transactionType,
            Points = points,
            BalanceAfter = balanceAfter,
            Description = description,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class LoyaltyReward : TenantEntity<string>
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public int PointsCost { get; private set; }
    public decimal? ValueAmount { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public int? StockQuantity { get; private set; }
    public string? RedemptionInstructions { get; private set; }
    public string? TermsConditions { get; private set; }

    private LoyaltyReward() { }

    public static LoyaltyReward Create(string id, string tenantId, string title, string? description, string category, int pointsCost, decimal? valueAmount = null, string? imageUrl = null, int? stockQuantity = null, string? redemptionInstructions = null, string? termsConditions = null)
    {
        return new LoyaltyReward
        {
            Id = id,
            TenantId = tenantId,
            Title = title,
            Description = description,
            Category = category,
            PointsCost = pointsCost,
            ValueAmount = valueAmount,
            ImageUrl = imageUrl,
            StockQuantity = stockQuantity,
            RedemptionInstructions = redemptionInstructions,
            TermsConditions = termsConditions,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateReward(string title, string? description, string category, int pointsCost, decimal? valueAmount = null, string? imageUrl = null, int? stockQuantity = null, string? redemptionInstructions = null, string? termsConditions = null)
    {
        Title = title;
        Description = description;
        Category = category;
        PointsCost = pointsCost;
        ValueAmount = valueAmount;
        ImageUrl = imageUrl;
        StockQuantity = stockQuantity;
        RedemptionInstructions = redemptionInstructions;
        TermsConditions = termsConditions;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsInStock()
    {
        return !StockQuantity.HasValue || StockQuantity.Value > 0;
    }

    public void DecrementStock()
    {
        if (StockQuantity.HasValue && StockQuantity.Value > 0)
        {
            StockQuantity--;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class LoyaltyRedemption : TenantEntity<string>
{
    public string UserId { get; private set; } = string.Empty;
    public string RewardId { get; private set; } = string.Empty;
    public int PointsSpent { get; private set; }
    public string Status { get; private set; } = "pending";
    public string? RedemptionCode { get; private set; }
    public DateTime RedeemedAt { get; private set; }
    public DateTime? FulfilledAt { get; private set; }

    private LoyaltyRedemption() { }

    public static LoyaltyRedemption Create(string id, string tenantId, string userId, string rewardId, int pointsSpent, string? redemptionCode = null)
    {
        return new LoyaltyRedemption
        {
            Id = id,
            TenantId = tenantId,
            UserId = userId,
            RewardId = rewardId,
            PointsSpent = pointsSpent,
            RedemptionCode = redemptionCode ?? GenerateRedemptionCode(),
            RedeemedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Fulfill()
    {
        Status = "fulfilled";
        FulfilledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = "cancelled";
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateRedemptionCode()
    {
        return $"RED{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}
