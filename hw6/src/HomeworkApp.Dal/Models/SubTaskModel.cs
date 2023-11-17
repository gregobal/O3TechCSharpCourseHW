namespace HomeworkApp.Dal.Models;

public record SubTaskModel {
    public required long TaskId { get; init; }
    
    public required string Title { get; init; }
    
    public required int Status { get; init; }
    
    public required long[] ParentTaskIds { get; init; }
}
