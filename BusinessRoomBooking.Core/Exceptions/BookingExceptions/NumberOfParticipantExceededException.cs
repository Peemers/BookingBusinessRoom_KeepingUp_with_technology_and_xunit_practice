namespace BusinessRoomBooking.Core.Exceptions.BookingExceptions;

public class NumberOfParticipantExceededException : Exception
{
  public NumberOfParticipantExceededException(int numberOfParticipant, int maxCapacity)
    : base($"La salle comporte {maxCapacity} places et ne peut contenir {numberOfParticipant} participants.")
  {
  }
}