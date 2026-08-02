namespace BusinessRoomBooking.Core.Exceptions.WorkerExceptions;

public class WorkerEmailAlreadyExistException : Exception
{
  public  WorkerEmailAlreadyExistException(string email) 
    : base($"L'email {email} existe déjà")
  {
  }
}