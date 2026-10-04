namespace TokenGenerator.Models
{
    public class LETQuestion
    {
        public required string Category { get; init; }
        public required string Question { get; init; }
        public required string[] Choices { get; init; }
        public required int CorrectIndex { get; init; }
        public required string Explanation { get; init; }
    }
}
