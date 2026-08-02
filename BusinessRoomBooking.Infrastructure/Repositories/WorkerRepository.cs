using BusinessRoomBooking.Core.Interfaces.Repositories;
using BusinessRoomBooking.Domain;
using BusinessRoomBooking.Infrastructure.DataBase.Context;
using Microsoft.EntityFrameworkCore;

namespace BusinessRoomBooking.Infrastructure.Repositories;

public class WorkerRepository(BusinessRoomBookingContext context) : BaseRepository<Worker>(context), IWorkerRepository
{
  public async Task<bool> EmailAlreadyExistAsync(string email)
  {
    IQueryable<Worker> query = DbSet.Where(w => w.Email == email);
    return await query.AnyAsync();
  }
}