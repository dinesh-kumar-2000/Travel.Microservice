namespace BookingService.Domain.Entities;

public class LoyaltyTier
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TierName { get; set; } = string.Empty;
    public int MinPoints { get; set; }
    public decimal DiscountPercentage { get; set; }
    public Dictionary<string, object>? Benefits { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoyaltyPoints
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public int CurrentPoints { get; set; }
    public int LifetimePoints { get; set; }
    public string Tier { get; set; } = "bronze";
    public int PointsExpiring { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public void AddPoints(int points)
    {
        CurrentPoints += points;
        LifetimePoints += points;
    }

    public void DeductPoints(int points)
    {
        CurrentPoints = Math.Max(0, CurrentPoints - points);
    }

    public bool HasSufficientPoints(int required)
    {
        return CurrentPoints >= required;
    }
}

public class LoyaltyTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int Points { get; set; }
    public int BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoyaltyReward
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public decimal? ValueAmount { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int? StockQuantity { get; set; }
    public string? RedemptionInstructions { get; set; }
    public string? TermsConditions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsInStock()
    {
        return !StockQuantity.HasValue || StockQuantity.Value > 0;
    }

    public void DecrementStock()
    {
        if (StockQuantity.HasValue && StockQuantity.Value > 0)
        {
            StockQuantity--;
        }
    }
}

public class LoyaltyRedemption
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public Guid RewardId { get; set; }
    public int PointsSpent { get; set; }
    public string Status { get; set; } = "pending";
    public string? RedemptionCode { get; set; }
    public DateTime RedeemedAt { get; set; }
    public DateTime? FulfilledAt { get; set; }

    // Navigation
    public LoyaltyReward? Reward { get; set; }

    public void Fulfill()
    {
        Status = "fulfilled";
        FulfilledAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = "cancelled";
    }
}

