namespace Quiz_DataBank.Model
{
    public class Quiz_AnsTransactionModel
    {
        public int? Answer_ID { get; set; }
        public int? Ques_ID { get; set; }
        public int? User_ID { get; set; }
        public string? Answer { get; set; }
        public string? Answer_Date { get; set; }
        public string? Correct_Answer { get; set; }
        public string? User_Name  { get; set; }
        public string? User_Email { get; set; }
        public string? Ques_Desc { get; set; }
        public string? Opt_A { get; set; }
        public string? Opt_B { get; set; }
        public string? Opt_C { get; set; }
        public string? Opt_D { get; set; }
        public string? Status { get; set; }
        public int? Topic_ID { get; set; }
        public string? Topic_Name { get; set; }
        public string? Quiz_Date { get; set; }
        public string? Result { get; set; }
        public string? Quiz_Name { get; set; }


    }
}
