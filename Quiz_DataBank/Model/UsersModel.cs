namespace Quiz_DataBank.Model
{
    public class UsersModel
    {
        public int? User_ID { get; set; }
        public string? User_Name { get; set; }
        public string? User_Email { get; set; }
        public string? User_Password { get; set; }
        public int? Status { get; set; }

        public int? Role_ID { get; set; }
        public string? userRole { get; set; }
        public int? Is_Correct { get; set; }
        public IFormFile? Image { get; set; }
        public int? TotalQuestions { get; set; }
        public double? Avg_Correct { get; set; }

        

    }
}
