namespace ARCServer.Domain.Entities
{
    /// <summary>
    /// Single homepage hero clip. Only one active row is used.
    /// </summary>
    public class HeroVideo : BaseEntity
    {
        public string VideoUrl { get; set; } = string.Empty;
    }
}
