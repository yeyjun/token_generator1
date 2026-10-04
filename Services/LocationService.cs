namespace TokenGenerator.Services
{
    public static class LocationService
    {
        public static async Task<Location?> GetCurrentLocationAsync()
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return null;

            return await Geolocation.Default.GetLastKnownLocationAsync()
                ?? await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
        }

        public static async Task<string> DescribePlaceAsync(Location location)
        {
            try
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
                var placemark = placemarks?.FirstOrDefault();

                var parts = new[] { placemark?.Locality, placemark?.AdminArea, placemark?.CountryName }
                    .Where(p => !string.IsNullOrWhiteSpace(p));
                var text = string.Join(", ", parts);

                return string.IsNullOrWhiteSpace(text) ? $"{location.Latitude:F4}, {location.Longitude:F4}" : text;
            }
            catch
            {
                return $"{location.Latitude:F4}, {location.Longitude:F4}";
            }
        }
    }
}
