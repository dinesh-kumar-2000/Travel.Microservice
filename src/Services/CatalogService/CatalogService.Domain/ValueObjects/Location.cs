namespace CatalogService.Domain.ValueObjects;

public class Location
{
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }

    private Location()
    {
        Address = string.Empty;
        City = string.Empty;
        Country = string.Empty;
    }

    public Location(decimal latitude, decimal longitude, string address, string city, string country)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));

        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        if (string.IsNullOrEmpty(address))
            throw new ArgumentException("Address cannot be null or empty", nameof(address));

        if (string.IsNullOrEmpty(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));

        if (string.IsNullOrEmpty(country))
            throw new ArgumentException("Country cannot be null or empty", nameof(country));

        Latitude = latitude;
        Longitude = longitude;
        Address = address;
        City = city;
        Country = country;
    }

    public double DistanceTo(Location other)
    {
        const double earthRadius = 6371; // Earth's radius in kilometers
        
        var lat1Rad = (double)Latitude * Math.PI / 180;
        var lat2Rad = (double)other.Latitude * Math.PI / 180;
        var deltaLatRad = ((double)other.Latitude - (double)Latitude) * Math.PI / 180;
        var deltaLonRad = ((double)other.Longitude - (double)Longitude) * Math.PI / 180;

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return earthRadius * c;
    }

    public override string ToString() => $"{Address}, {City}, {Country}";
    public override bool Equals(object? obj) => obj is Location location && 
        Latitude == location.Latitude && 
        Longitude == location.Longitude && 
        Address == location.Address && 
        City == location.City && 
        Country == location.Country;
    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude, Address, City, Country);
}
