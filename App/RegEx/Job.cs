namespace models
{
    public enum Job
    {
        Detaillant = 0,
        Proffessional = 1,
        WGrossit = 4,
        Grossist = 2,
        Entrepreneur = 8,
        All = Detaillant | Proffessional | WGrossit | Grossist | Entrepreneur
    }
}