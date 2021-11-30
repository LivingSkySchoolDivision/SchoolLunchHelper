using System;

namespace LSSD.Lunch
{
    public class LoggedTransaction
    {
        public string InputStudentID { get; set; }
        public string StudentName { get; set; }
        public Guid TransactionGUID { get; set; }
        public bool UploadSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}
