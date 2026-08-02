using BusinessRoomBooking.Core.Dtos.Worker.Request;
using BusinessRoomBooking.Core.Dtos.Worker.Response;
using BusinessRoomBooking.Core.Exceptions.WorkerExceptions;
using BusinessRoomBooking.Core.Interfaces.Repositories;
using BusinessRoomBooking.Core.Interfaces.Services;
using BusinessRoomBooking.Core.Mappers;
using BusinessRoomBooking.Domain;

namespace BusinessRoomBooking.Core.Services;

public class WorkerService(IWorkerRepository workerRepository) : IWorkerService
{
  public async Task<WorkerResponseDto> CreateWorkerAsync(CreateWorkerRequestDto dto)
  {
    bool emailAlreadyExist = await workerRepository.EmailAlreadyExistAsync(dto.Email);

    if (emailAlreadyExist)
    {
      throw new WorkerEmailAlreadyExistException(dto.Email);
    }

    Worker worker = dto.ToWorker();

    await workerRepository.AddAsync(worker);
    await workerRepository.SaveChangesAsync();
    
    return worker.ToWorkerResponseDto();
  }
}