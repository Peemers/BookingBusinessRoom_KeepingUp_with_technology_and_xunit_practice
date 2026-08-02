using BusinessRoomBooking.Core.Dtos.Worker.Request;
using BusinessRoomBooking.Core.Dtos.Worker.Response;
using BusinessRoomBooking.Core.Exceptions.WorkerExceptions;
using BusinessRoomBooking.Core.Interfaces.Repositories;
using BusinessRoomBooking.Core.Services;
using BusinessRoomBooking.Domain;
using NSubstitute;

namespace BusinessRoomBooking.Tests.Services;

public class WorkerServiceTests
{
  [Fact]
  public async Task CreateWorker_ShouldReturnCreatedWorker()
  {
    //Arrange
    IWorkerRepository workerRepository = Substitute.For<IWorkerRepository>();
    
    CreateWorkerRequestDto dto = new CreateWorkerRequestDto
    {
      FirstName = "Test",
      LastName = "Test",
      Email = "test@test.be",
    };
    
    workerRepository.EmailAlreadyExistAsync(dto.Email).Returns(false);

    WorkerService workerService = new WorkerService(workerRepository);
    
    //Act

    WorkerResponseDto result = await workerService.CreateWorkerAsync(dto);
    
    //Assert
    
    Assert.Equal(dto.Email, result.Email);
    Assert.Equal(dto.FirstName, result.FirstName);
    Assert.Equal(dto.LastName, result.LastName);
    
    await workerRepository.Received(1).AddAsync(Arg.Any<Worker>());
    await workerRepository.Received(1).SaveChangesAsync();
  }
  
  [Fact]
  public async Task CreateWorker_ShouldThrow_WhenWorkerEmailAlreadyExist()
  {
    IWorkerRepository workerRepository = Substitute.For<IWorkerRepository>();

    CreateWorkerRequestDto dto = new CreateWorkerRequestDto
    {
      FirstName = "Test",
      LastName = "Test",
      Email = "test@test.be"
    };
    
    workerRepository.EmailAlreadyExistAsync(dto.Email).Returns(true);
    
    WorkerService workerService = new WorkerService(workerRepository);
    
    await Assert.ThrowsAsync<WorkerEmailAlreadyExistException>(() => workerService.CreateWorkerAsync(dto));
  }
}