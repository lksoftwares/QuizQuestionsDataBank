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
    }
}
