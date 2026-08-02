using BusinessRoomBooking.Domain;

namespace BusinessRoomBooking.Core.Interfaces.Repositories;

public interface IWorkerRepository : IBaseRepository<Worker>
{
  public Task<bool> EmailAlreadyExistAsync(string email);
}