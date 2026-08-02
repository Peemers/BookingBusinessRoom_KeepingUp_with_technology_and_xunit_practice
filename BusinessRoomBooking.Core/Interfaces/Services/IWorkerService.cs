using BusinessRoomBooking.Core.Dtos.Worker.Request;
using BusinessRoomBooking.Core.Dtos.Worker.Response;

namespace BusinessRoomBooking.Core.Interfaces.Services;

public interface IWorkerService
{
  public Task<WorkerResponseDto> CreateWorkerAsync(CreateWorkerRequestDto dto);
}