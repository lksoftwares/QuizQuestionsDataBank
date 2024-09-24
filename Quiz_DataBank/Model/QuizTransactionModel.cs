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
        public int? Allowed_Time { get; set; }
        public string? Topic_Name { get; set; }
        public string? Quiz_Name { get; set; }
        public int? Total_Questions { get; set; }
        public int? CorrectAnswers { get; set; }
        public int? TotalQuestions { get; set; }
        public double? Score_Percentage { get; set; }
        public bool? isProceed { get; set; }
        public bool? IsAllowed { get; set; }



    }
}
