namespace Quiz_DataBank.Model
{
    public class QuestionsModel
    {
        public int? Ques_ID { get; set; }
        public string? Ques_Desc { get; set; }
        public string? Opt_A { get; set; }
        public string? Opt_B { get; set; }
        public string? Opt_C { get; set; }
        public string? Opt_D { get; set; }
        public string? Correct_Answer { get; set; }
        public string? Status { get; set; }
        public int? Topic_ID { get; set; }
        public string? Topic_Name { get; set; }

    }
}
