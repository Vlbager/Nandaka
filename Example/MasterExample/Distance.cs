namespace Example.MasterExample
{
    public readonly struct Distance
    {
        private const int MillimetersInCentimeter = 100;
        
        private readonly int _distanceInMillimeters;

        public int GetCm => _distanceInMillimeters / MillimetersInCentimeter;
        public int GetMm => _distanceInMillimeters;

        public Distance(int distanceInMillimeters)
        {
            _distanceInMillimeters = distanceInMillimeters;
        }

        public override string ToString()
        {
            return $"{_distanceInMillimeters} mm";
        }
    }
}