namespace FitnessCenter1.Models
{
    public class MemberProfile
    {
        public int ProfileID { get; set; }
        public int UserID { get; set; }
        public double Height  { get; set; }
        public double Weight  { get; set; }
        public String BodyType  { get; set; }
        public String Goal  { get; set; } // hedef örn: kilo vermek
        public string PhotoFilePath { get; set; } // yüklenecek dosyanın sunucudaki dosya yolu
        public string AISuggestion{ get; set; } // yapay zeka önerisi
    }
}
