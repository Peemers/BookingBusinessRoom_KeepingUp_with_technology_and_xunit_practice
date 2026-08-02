using BusinessRoomBooking.Core.Dtos.Worker.Request;
using BusinessRoomBooking.Core.Dtos.Worker.Response;
using BusinessRoomBooking.Core.Dtos.Worker.Summaries;
using BusinessRoomBooking.Domain;

namespace BusinessRoomBooking.Core.Mappers;

public static class WorkerMapper
{
  public static WorkerResponseDto ToWorkerResponseDto(this Worker worker)
  {
    return new WorkerResponseDto
    {
      Id = worker.Id,
      Email = worker.Email,
      FirstName = worker.FirstName,
      LastName = worker.LastName,
    };
  }

  public static Worker ToWorker(this CreateWorkerRequestDto dto)
  {
    return new Worker
    {
      Id = Guid.NewGuid(),
      Email = dto.Email,
      FirstName = dto.FirstName,
      LastName = dto.LastName,
    };
  }

  public static WorkerSummaryDto ToWorkerSummaryDto(this Worker worker)
  {
    return new WorkerSummaryDto
    {
      Id = worker.Id,
      FirstName = worker.FirstName,
      LastName = worker.LastName,
    };
  }
}