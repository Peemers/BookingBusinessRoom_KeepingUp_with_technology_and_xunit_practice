using BusinessRoomBooking.Core.Interfaces.Repositories;
using BusinessRoomBooking.Infrastructure.DataBase.Context;
using BusinessRoomBooking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessRoomBooking.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    //DbContext
    services.AddDbContext<BusinessRoomBookingContext>(options =>
      options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    
    //Repo
    services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
    services.AddScoped<IBookingRepository, BookingRepository>();
    services.AddScoped<IRoomRepository, RoomRepository>();
    services.AddScoped<IRoomEquipmentRepository, RoomEquipmentRepository>();
    services.AddScoped<IWorkerRepository, WorkerRepository>();
    
    return services;
  }
}