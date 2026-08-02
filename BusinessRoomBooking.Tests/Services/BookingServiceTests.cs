using BusinessRoomBooking.Core.Dtos.Booking.Request;
using BusinessRoomBooking.Core.Dtos.Booking.Response;
using BusinessRoomBooking.Core.Exceptions.BookingExceptions;
using BusinessRoomBooking.Core.Exceptions.RoomExceptions;
using BusinessRoomBooking.Core.Exceptions.WorkerExceptions;
using BusinessRoomBooking.Core.Interfaces.Repositories;
using BusinessRoomBooking.Core.Services;
using BusinessRoomBooking.Domain;
using NSubstitute;

namespace BusinessRoomBooking.Tests.Services;

public class BookingServiceTests
{
  [Fact]
  public async Task GetBookingById_ShouldReturnBooking()
  {
    //Arrange

    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Room room = new Room
    {
      Id = roomId,
      Location = "1er étage",
      MaxCapacity = 10,
      Bookings = new List<Booking>(),
      RoomEquipments = new List<RoomEquipment>(),
      Name = "Salle A"
    };

    Worker worker = new Worker
    {
      Id = workerId,
      FirstName = "Mathieu",
      LastName = "Peemeros",
      Email = "Mathieu@SalleA.be",
      Bookings = new List<Booking>()
    };

    Booking booking = new Booking
    {
      Id = Guid.NewGuid(),
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
      NumberOfParticipant = 10,
      RoomId = roomId,
      WorkerId = workerId,
      Room = room,
      Worker = worker
    };

    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    bookingRepository.GetByIdAsync(booking.Id).Returns(booking);

    BookingResponseDto result = await bookingService.GetBookingByIdAsync(booking.Id);

    Assert.Equal(booking.Id, result.Id);
    Assert.Equal(room.Location, result.Room.Location);
    Assert.Equal(worker.FirstName, result.Worker.FirstName);
  }

  [Fact]
  public async Task GetBookingsById_ShouldThrow_WhenBookingNotFound()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Booking booking = new Booking
    {
      Id = Guid.NewGuid(),
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
      NumberOfParticipant = 10,
      RoomId = roomId,
      WorkerId = workerId,
    };

    bookingRepository.GetByIdAsync(booking.Id).Returns((Booking?)null);

    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    await Assert.ThrowsAsync<BookingNotFoundException>(() => bookingService.GetBookingByIdAsync(booking.Id));
  }

  [Fact]
  public async Task CancelBookingAsync_ShouldDeleteBooking()
  {
    IRoomRepository roomRepository = Substitute.For<IRoomRepository>();
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Room room = new Room
    {
      Id = roomId,
      Location = "Salle A",
      MaxCapacity = 10,
      Bookings = new List<Booking>(),
      RoomEquipments = new List<RoomEquipment>(),
      Name = "Salle A"
    };

    Worker worker = new Worker
    {
      Id = workerId,
      Email = "test@test.be",
      FirstName = "Test",
      LastName = "Test",
      Bookings = new List<Booking>()
    };


    Booking booking = new Booking
    {
      Id = Guid.NewGuid(),
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
      NumberOfParticipant = 18,
      RoomId = roomId,
      WorkerId = worker.Id,
      Room = room,
      Worker = worker
    };

    bookingRepository.GetByIdAsync(booking.Id).Returns(booking);

    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    await bookingService.CancelBookingAsync(booking.Id);

    await bookingRepository.Received(1).DeleteAsync(booking.Id);
  }

  [Fact]
  public async Task CancelBookingAsync_ShouldThrow_WhenBookingNotFound()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Room room = new Room
    {
      Id = roomId,
      Location = "1er étages",
      MaxCapacity = 10,
      Bookings = new List<Booking>(),
      RoomEquipments = new List<RoomEquipment>(),
      Name = "Salle A"
    };

    Worker worker = new Worker
    {
      Id = workerId,
      Email = "test@test.be",
      FirstName = "Test",
      LastName = "Test",
      Bookings = new List<Booking>()
    };


    Booking booking = new Booking
    {
      Id = Guid.NewGuid(),
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
      NumberOfParticipant = 18,
      RoomId = roomId,
      WorkerId = worker.Id,
      Room = room,
      Worker = worker
    };

    bookingRepository.GetByIdAsync(booking.Id).Returns((Booking?)null);

    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    await Assert.ThrowsAsync<BookingNotFoundException>(() => bookingService.CancelBookingAsync(booking.Id));
  }

  [Fact]
  public async Task CancelBookingAsync_ShouldThrow_WhenBookingIsInThePast()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Room room = new Room
    {
      Id = roomId,
      Location = "1er étages",
      MaxCapacity = 10,
      Bookings = new List<Booking>(),
      RoomEquipments = new List<RoomEquipment>(),
      Name = "Salle A"
    };

    Worker worker = new Worker
    {
      Id = workerId,
      Email = "test@test.be",
      FirstName = "Test",
      LastName = "Test",
      Bookings = new List<Booking>()
    };


    Booking booking = new Booking
    {
      Id = Guid.NewGuid(),
      StartDate = DateTime.UtcNow.AddDays(-1),
      EndDate = DateTime.UtcNow.AddDays(-1).AddHours(1),
      NumberOfParticipant = 18,
      RoomId = roomId,
      WorkerId = worker.Id,
      Room = room,
      Worker = worker
    };

    bookingRepository.GetByIdAsync(booking.Id).Returns(booking);

    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    await Assert.ThrowsAsync<BookingDateAlreadyPassedException>(() => bookingService.CancelBookingAsync(booking.Id));
  }

  [Fact]
  public async Task CreateBookingAsync_ShouldReturnCreatedBooking()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Room room = new Room
    {
      Id = roomId,
      Location = "1er étages",
      MaxCapacity = 10,
      Bookings = new List<Booking>(),
      RoomEquipments = new List<RoomEquipment>(),
      Name = "Salle A"
    };

    Worker worker = new Worker
    {
      Id = workerId,
      Email = "test@test.be",
      FirstName = "Test",
      LastName = "Test",
      Bookings = new List<Booking>()
    };

    CreateBookingRequestDto createBookingRequestDto = new CreateBookingRequestDto
    {
      NumberOfParticipant = 10,
      RoomId = roomId,
      WorkerId = worker.Id,
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1)
    };

    roomRepository.GetByIdAsync(createBookingRequestDto.RoomId).Returns(room);
    workerRepository.GetByIdAsync(createBookingRequestDto.WorkerId).Returns(worker);
    bookingRepository.HasOverlapAsync(createBookingRequestDto.RoomId, createBookingRequestDto.StartDate, createBookingRequestDto.EndDate).Returns(false);

    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    BookingResponseDto result = await bookingService.CreateBookingAsync(createBookingRequestDto);

    Assert.Equal(createBookingRequestDto.NumberOfParticipant, result.NumberOfParticipant);
    Assert.Equal(createBookingRequestDto.StartDate, result.StartDate);
    Assert.Equal(createBookingRequestDto.EndDate, result.EndDate);
    Assert.Equal(createBookingRequestDto.RoomId, result.Room.Id);
    Assert.Equal(createBookingRequestDto.WorkerId, result.Worker.Id);

    await bookingRepository.Received(1).AddAsync(Arg.Any<Booking>());
  }

  [Fact]
  public async Task CreateBookingAsync_ShouldThrow_WhenRoomNotFound()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Worker worker = new Worker
    {
      Email = "test@test.be",
      FirstName = "Test",
      LastName = "Test",
      Bookings = new List<Booking>(),
      Id = workerId
    };

    CreateBookingRequestDto dto = new CreateBookingRequestDto
    {
      NumberOfParticipant = 10,
      RoomId = roomId,
      WorkerId = worker.Id,
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
    };

    roomRepository.GetByIdAsync(roomId).Returns((Room?)null);
    workerRepository.GetByIdAsync(dto.WorkerId).Returns(worker);
    bookingRepository.HasOverlapAsync(dto.RoomId, dto.StartDate, dto.EndDate).Returns(false);

    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    await Assert.ThrowsAsync<RoomNotFoundException>(() => bookingService.CreateBookingAsync(dto));
  }

  [Fact]
  public async Task CreateBookingAsync_ShouldThrow_WhenWorkerNotFound()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Worker worker = new Worker
    {
      Email = "test@test.be",
      FirstName = "Test",
      LastName = "Test",
      Bookings = new List<Booking>(),
      Id = workerId
    };

    Room room = new Room
    {
      Id = roomId,
      Location = "1er étages",
      MaxCapacity = 10,
      Bookings = new List<Booking>(),
      RoomEquipments = new List<RoomEquipment>(),
      Name = "Salle A"
    };

    CreateBookingRequestDto dto = new CreateBookingRequestDto
    {
      NumberOfParticipant = 10,
      RoomId = roomId,
      WorkerId = worker.Id,
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
    };

    roomRepository.GetByIdAsync(roomId).Returns(room);
    workerRepository.GetByIdAsync(dto.WorkerId).Returns((Worker?)null);
    bookingRepository.HasOverlapAsync(dto.RoomId, dto.StartDate, dto.EndDate).Returns(false);

    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    await Assert.ThrowsAsync<WorkerNotFoundException>(() => bookingService.CreateBookingAsync(dto));
  }

  [Fact]
  public async Task CreateBookingAsync_ShouldThrow_WhenBookingOverlapping()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Worker worker = new Worker
    {
      Email = "test@test.be",
      FirstName = "Test",
      LastName = "Test",
      Bookings = new List<Booking>(),
      Id = workerId
    };

    Room room = new Room
    {
      Id = roomId,
      Location = "1er étages",
      MaxCapacity = 10,
      Bookings = new List<Booking>(),
      RoomEquipments = new List<RoomEquipment>(),
      Name = "Salle A"
    };

    CreateBookingRequestDto dto = new CreateBookingRequestDto
    {
      NumberOfParticipant = 10,
      RoomId = roomId,
      WorkerId = worker.Id,
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
    };

    roomRepository.GetByIdAsync(roomId).Returns(room);
    workerRepository.GetByIdAsync(dto.WorkerId).Returns(worker);
    bookingRepository.HasOverlapAsync(dto.RoomId, dto.StartDate, dto.EndDate).Returns(true);

    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    await Assert.ThrowsAsync<BookingOverlapException>(() => bookingService.CreateBookingAsync(dto));
  }
  [Fact]
  public async Task CreateBookingAsync_ShouldThrow_WhenNumberOfParticipantsExceeded()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Room room = new Room
    {
      MaxCapacity = 5,
      Location = "test",
      Id = Guid.NewGuid(),
      Bookings = new List<Booking>(),
      RoomEquipments = new List<RoomEquipment>(),
      Name = "Salle A"
    };

    Worker worker = new Worker
    {
      Id = Guid.NewGuid(),
      FirstName = "Test",
      LastName = "Test",
      Email = "test@test.be",
      Bookings = new List<Booking>(),
    };

    CreateBookingRequestDto requestDto = new CreateBookingRequestDto
    {
      NumberOfParticipant = 10,
      RoomId = room.Id,
      WorkerId = worker.Id,
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
    };
    
    roomRepository.GetByIdAsync(room.Id).Returns(room);
    workerRepository.GetByIdAsync(requestDto.WorkerId).Returns(worker);
    bookingRepository.HasOverlapAsync(requestDto.RoomId, requestDto.StartDate, requestDto.EndDate).Returns(false);
    
    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);
    
    await Assert.ThrowsAsync<NumberOfParticipantExceededException>(() => bookingService.CreateBookingAsync(requestDto));
  }

  [Fact]
  public async Task UpdateBookingAsync_ShouldUpdateBooking()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Room room = new Room
    {
      Id = roomId,
      Location = "1er étages",
      MaxCapacity = 10,
      Bookings = new List<Booking>(),
      RoomEquipments = new List<RoomEquipment>(),
      Name = "Salle A"
    };

    Worker worker = new Worker
    {
      Email = "mail@mail.be",
      FirstName = "Test",
      LastName = "Test",
      Bookings = new List<Booking>(),
      Id = workerId,
    };

    Booking booking = new Booking
    {
      Id = Guid.NewGuid(),
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
      NumberOfParticipant = 10,
      RoomId = room.Id,
      WorkerId = worker.Id,
      Room = room,
      Worker = worker
    };
    UpdateBookingRequestDto dto = new UpdateBookingRequestDto
    {
      NumberOfParticipant = 10,
      StartDate = DateTime.UtcNow.AddDays(2),
      EndDate = DateTime.UtcNow.AddDays(2),
    };

    bookingRepository.GetByIdWithRoomAsync(booking.Id).Returns(booking);
    bookingRepository.HasOverlapAsync(booking.RoomId, dto.StartDate, dto.EndDate, booking.Id).Returns(false);

    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);
    BookingResponseDto result = await bookingService.UpdateBookingDateAsync(booking.Id, dto);

    Assert.Equal(dto.StartDate, result.StartDate);
    Assert.Equal(dto.EndDate, result.EndDate);
    Assert.Equal(dto.NumberOfParticipant, result.NumberOfParticipant);

    await bookingRepository.Received(1).HasOverlapAsync(booking.RoomId, dto.StartDate, dto.EndDate, booking.Id);
    await bookingRepository.Received(1).UpdateAsync(booking);
  }

  [Fact]
  public async Task UpdateBookingAsync_ShouldThrow_WhenBookingNotFound()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Booking booking = new Booking
    {
      Id = Guid.NewGuid(),
      NumberOfParticipant = 10,
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
      RoomId = roomId,
      WorkerId = workerId,
    };

    UpdateBookingRequestDto dto = new UpdateBookingRequestDto
    {
      NumberOfParticipant = 10,
      StartDate = DateTime.UtcNow.AddDays(2),
      EndDate = DateTime.UtcNow.AddDays(2),
    };

    bookingRepository.GetByIdWithRoomAsync(booking.Id).Returns((Booking?)null);

    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    await Assert.ThrowsAsync<BookingNotFoundException>(() => bookingService.UpdateBookingDateAsync(booking.Id, dto));
  }

  [Fact]
  public async Task UpdateBookingAsync_ShouldThrow_WhenBookingDateAlreadyPassed()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Booking booking = new Booking
    {
      Id = Guid.NewGuid(),
      NumberOfParticipant = 10,
      StartDate = DateTime.UtcNow.AddDays(-2),
      EndDate = DateTime.UtcNow.AddDays(-2),
      RoomId = roomId,
      WorkerId = workerId,
    };

    UpdateBookingRequestDto dto = new UpdateBookingRequestDto
    {
      NumberOfParticipant = 10,
      StartDate = DateTime.UtcNow.AddDays(2),
      EndDate = DateTime.UtcNow.AddDays(2),
    };

    bookingRepository.GetByIdWithRoomAsync(booking.Id).Returns(booking);
    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    await Assert.ThrowsAsync<BookingDateAlreadyPassedException>(() => bookingService.UpdateBookingDateAsync(booking.Id, dto));
  }

  [Fact]
  public async Task UpdateBookingDateAsync_ShouldThrow_WhenBookingOverlapping()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Guid roomId = Guid.NewGuid();
    Guid workerId = Guid.NewGuid();

    Booking booking = new Booking
    {
      Id = Guid.NewGuid(),
      NumberOfParticipant = 10,
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
      RoomId = roomId,
      WorkerId = workerId,
    };
    UpdateBookingRequestDto dto = new UpdateBookingRequestDto
    {
      NumberOfParticipant = 10,
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
    };

    bookingRepository.GetByIdWithRoomAsync(booking.Id).Returns(booking);
    bookingRepository.HasOverlapAsync(booking.RoomId, dto.StartDate, dto.EndDate, booking.Id).Returns(true);
    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);

    await Assert.ThrowsAsync<BookingOverlapException>(() => bookingService.UpdateBookingDateAsync(booking.Id, dto));
  }

  [Fact]
  public async Task UpdateBookingDateAsync_ShouldThrow_WhenNumberOfParticipantsExceeded()
  {
    IBookingRepository bookingRepository = Substitute.For<IBookingRepository>();
    IBaseRepository<Room> roomRepository = Substitute.For<IBaseRepository<Room>>();
    IBaseRepository<Worker> workerRepository = Substitute.For<IBaseRepository<Worker>>();

    Worker worker = new Worker
    {
      Id = Guid.NewGuid(),
      FirstName = "John",
      LastName = "Doe",
      Email = "test@test.be",
      Bookings = new List<Booking>()
    };

    Room room = new Room
    {
      Id = Guid.NewGuid(),
      MaxCapacity = 5,
      Location = "test",
      Name = "test",
      Bookings = new List<Booking>(),
      RoomEquipments = new List<RoomEquipment>()
    };

    Booking booking = new Booking
    {
      Id = Guid.NewGuid(),
      NumberOfParticipant = 4,
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
      Worker = worker,
      WorkerId = worker.Id,
      Room = room,
      RoomId = room.Id,
    };

    UpdateBookingRequestDto requestDto = new UpdateBookingRequestDto
    {
      NumberOfParticipant = room.MaxCapacity + 1,
      StartDate = DateTime.UtcNow.AddDays(1),
      EndDate = DateTime.UtcNow.AddDays(1),
    };
    
    bookingRepository.GetByIdWithRoomAsync(booking.Id).Returns(booking);
    bookingRepository.HasOverlapAsync(booking.RoomId, requestDto.StartDate, requestDto.EndDate, booking.Id).Returns(false);
    
    BookingService bookingService = new BookingService(roomRepository, workerRepository, bookingRepository);
    
    await Assert.ThrowsAsync<NumberOfParticipantExceededException>(() => bookingService.UpdateBookingDateAsync(booking.Id, requestDto));
  }
}