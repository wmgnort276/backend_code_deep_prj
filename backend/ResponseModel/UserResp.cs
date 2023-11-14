namespace backend.ResponseModel
{
    public class UserResp
    {
        public string Id { get; set; }

        public string UerName { get; set; }

        public IList<String> Role { get; set; }

        public int Score { get; set; }
    }
}
