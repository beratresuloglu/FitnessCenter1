using System.ComponentModel.DataAnnotations;

namespace FitnessCenter1.Models
{
    public class FitnessCenter
    {
        public int FitnessCenterID { get; set; }
        public String FitnessCenterName { get; set; }
        public String FitnessCenterAdress { get; set; }
        public string FitnessCenterWorkingHours {  get; set; }

        public FitnessCenter()
        {
            
        }
    }
}
