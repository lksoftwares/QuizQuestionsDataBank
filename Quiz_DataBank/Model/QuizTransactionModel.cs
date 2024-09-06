namespace Quiz_DataBank.Model
{
    public class QuizTransactionModel
    {
        public int? Quiz_ID { get; set; }
        public int? Ques_ID { get; set; }
        public int? User_ID { get; set; }
        public string? Quiz_Date { get; set; }
        public string? Ques_Desc { get; set; }
        public string? Opt_A { get; set; }
        public string? Opt_B { get; set; }
        public string? Opt_C { get; set; }
        public string? Opt_D { get; set; }
        public string? User_Email{ get; set; }



    }
}
