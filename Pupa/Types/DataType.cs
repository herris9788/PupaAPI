namespace Pupa.Types
{
    public enum RequestPriority { Low, Normal, High, Urgent }
    public enum RequestStatus { Draft, Submitted, Approved, InProgress, Completed, Cancelled }
    public enum JobStatus { Pending, InProgress, Done, Cancelled }
    public enum FieldType { Text, Number, Dropdown, Json, Boolean }
}
