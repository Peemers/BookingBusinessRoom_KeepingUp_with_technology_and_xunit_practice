using System.Net;
using System.Text.Json;
using BusinessRoomBooking.Core.Exceptions.BookingExceptions;
using BusinessRoomBooking.Core.Exceptions.EquipmentExceptions;
using BusinessRoomBooking.Core.Exceptions.RoomExceptions;
using BusinessRoomBooking.Core.Exceptions.WorkerExceptions;

namespace BusinessRoomBooking.MiddleWare;

public class ExceptionMiddleware(RequestDelegate next)
{
  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await next(context);
    }
    catch (Exception ex)
    {
      await HandleExceptionAsync(context, ex);
    }
  }
  private static Task HandleExceptionAsync(HttpContext context, Exception ex)
  {
    (int statusCode, string message) = ex switch
    {
      RoomNotFoundException => ((int)HttpStatusCode.NotFound, ex.Message),
      WorkerNotFoundException => ((int)HttpStatusCode.NotFound, ex.Message),
      BookingNotFoundException => ((int)HttpStatusCode.NotFound, ex.Message),
      BookingOverlapException => ((int)HttpStatusCode.Conflict, ex.Message),
      EquipmentNotFoundException => ((int)HttpStatusCode.NotFound, ex.Message),
      EquipmentAlreadyAssignedException => ((int)HttpStatusCode.Conflict, ex.Message),
      BookingDateAlreadyPassedException => ((int)HttpStatusCode.Conflict, ex.Message),
      WorkerEmailAlreadyExistException => ((int)HttpStatusCode.Conflict, ex.Message),
      NumberOfParticipantExceededException => ((int)HttpStatusCode.BadRequest, ex.Message),
      _ => ((int)HttpStatusCode.InternalServerError, "Une erreur interne est survenue.")
    };
    context.Response.StatusCode = statusCode;
    context.Response.ContentType = "application/json";

    var errorResponse = new { message };
    string json = JsonSerializer.Serialize(errorResponse);
    return context.Response.WriteAsync(json);
  }
}