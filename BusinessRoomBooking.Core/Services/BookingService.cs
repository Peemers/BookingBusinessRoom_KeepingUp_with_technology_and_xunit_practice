using BusinessRoomBooking.Core.Dtos.Booking.Projections;
using BusinessRoomBooking.Core.Dtos.Booking.Request;
using BusinessRoomBooking.Core.Dtos.Booking.Response;
using BusinessRoomBooking.Core.Exceptions;
using BusinessRoomBooking.Core.Exceptions.BookingExceptions;
using BusinessRoomBooking.Core.Exceptions.RoomExceptions;
using BusinessRoomBooking.Core.Exceptions.WorkerExceptions;
using BusinessRoomBooking.Core.Interfaces.Repositories;
using BusinessRoomBooking.Core.Interfaces.Services;
using BusinessRoomBooking.Core.Mappers;
using BusinessRoomBooking.Domain;

namespace BusinessRoomBooking.Core.Services;

public class BookingService(
  IBaseRepository<Room> roomRepository,
  IBaseRepository<Worker> workerRepository,
  IBookingRepository bookingRepository) : IBookingService
{
  public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto dto)
  {
    Room? room = await roomRepository.GetByIdAsync(dto.RoomId);
    if (room is null)
    {
      throw new RoomNotFoundException(dto.RoomId);
    }

    if (dto.NumberOfParticipant > room.MaxCapacity)
    {
      throw new NumberOfParticipantExceededException(dto.NumberOfParticipant,  room.MaxCapacity);
    }

    Worker? worker = await workerRepository.GetByIdAsync(dto.WorkerId);

    if (worker is null)
    {
      throw new WorkerNotFoundException(dto.WorkerId);
    }

    bool hasOverlap = await bookingRepository.HasOverlapAsync(dto.RoomId, dto.StartDate, dto.EndDate);
    if (hasOverlap)
    {
      throw new BookingOverlapException(dto.RoomId, dto.StartDate, dto.EndDate);
    }

    Booking booking = dto.ToBooking(room, worker);

    await bookingRepository.AddAsync(booking);
    await bookingRepository.SaveChangesAsync();

    return booking.ToBookingResponseDto();
  }

  public async Task<BookingResponseDto> GetBookingByIdAsync(Guid id)
  {
    Booking? booking = await bookingRepository.GetByIdAsync(id);
    if (booking is null)
    {
      throw new BookingNotFoundException(id);
    }

    return booking.ToBookingResponseDto();
  }

  public async Task CancelBookingAsync(Guid id)
  {
    Booking? booking = await bookingRepository.GetByIdAsync(id);
    if (booking is null)
    {
      throw new BookingNotFoundException(id);
    }

    if (booking.StartDate < DateTime.UtcNow)
    {
      throw new BookingDateAlreadyPassedException(booking.StartDate, id);
    }

    await bookingRepository.DeleteAsync(id);
    await bookingRepository.SaveChangesAsync();
  }

  public async Task<BookingResponseDto> UpdateBookingDateAsync(Guid bookingId, UpdateBookingRequestDto dto)
  {
    Booking? booking = await bookingRepository.GetByIdWithRoomAsync(bookingId);
    if (booking is null)
    {
      throw new BookingNotFoundException(bookingId);
    }
    
    if (booking.StartDate < DateTime.UtcNow)
    {
      throw new BookingDateAlreadyPassedException(booking.StartDate, bookingId);
    }

    bool hasOverlap = await bookingRepository.HasOverlapAsync(booking.RoomId, dto.StartDate, dto.EndDate, bookingId);
    if (hasOverlap)
    {
      throw new BookingOverlapException(booking.RoomId, dto.StartDate, dto.EndDate);
    }
    
    if (dto.NumberOfParticipant > booking.Room.MaxCapacity)
    {
      throw new NumberOfParticipantExceededException(dto.NumberOfParticipant,  booking.Room.MaxCapacity);
    }

    booking.UpdateFromDto(dto);

    await bookingRepository.UpdateAsync(booking);
    await bookingRepository.SaveChangesAsync();

    return booking.ToBookingResponseDto();
  }
}