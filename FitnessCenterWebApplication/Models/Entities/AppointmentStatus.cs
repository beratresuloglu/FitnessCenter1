namespace FitnessCenterWebApplication.Models.Entities
{
    public enum AppointmentStatus
    {
        Pending = 0,      // Onay Bekliyor
        Approved = 1,     // Onaylandı
        Completed = 2,    // Tamamlandı
        Cancelled = 3,    // İptal Edildi
        NoShow = 4        // Gelmedi
    }
}