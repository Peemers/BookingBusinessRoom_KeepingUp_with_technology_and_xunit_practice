namespace BusinessRoomBooking.Core.Dtos.Worker.Summaries;

public record WorkerSummaryDto
{
  public required Guid Id { get; init; }
  public required string FirstName { get; init; }
  public required string LastName { get; init; }
}