using BusinessRoomBooking.Core.Interfaces.Services;
using BusinessRoomBooking.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessRoomBooking.Core.Extensions;

public static class ServiceExtensions
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    //Services
    services.AddScoped<IBookingService, BookingService>();
    services.AddScoped<IRoomService, RoomService>();
    services.AddScoped<IWorkerService, WorkerService>();
    
    return services;
  }
}