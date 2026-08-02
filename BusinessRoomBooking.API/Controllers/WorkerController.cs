using BusinessRoomBooking.Core.Dtos.Worker.Request;
using BusinessRoomBooking.Core.Dtos.Worker.Response;
using BusinessRoomBooking.Core.Dtos.Worker.Summaries;
using BusinessRoomBooking.Core.Exceptions.WorkerExceptions;
using BusinessRoomBooking.Core.Interfaces.Repositories;
using BusinessRoomBooking.Core.Interfaces.Services;
using BusinessRoomBooking.Core.Mappers;
using BusinessRoomBooking.Domain;
using Microsoft.AspNetCore.Mvc;

namespace BusinessRoomBooking.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkerController(
  IBaseRepository<Worker> workerRepository,
  IWorkerService workerService) : ControllerBase
{
  [HttpGet("{id:guid}")]
  public async Task<ActionResult<WorkerResponseDto>> GetById(Guid id)
  {
    Worker? worker = await workerRepository.GetByIdAsync(id);
    if (worker is null) throw new WorkerNotFoundException(id);
    return Ok(worker.ToWorkerResponseDto());
  }

  [HttpPost]
  public async Task<ActionResult<WorkerResponseDto>> CreateWorker([FromBody] CreateWorkerRequestDto dto)
  {
    WorkerResponseDto worker = await workerService.CreateWorkerAsync(dto);
    
    return CreatedAtAction(nameof(GetById), new { id = worker.Id }, worker);
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<WorkerSummaryDto>>> GetAll()
  {
    IEnumerable<Worker> workers = await workerRepository.GetAllAsync();
    
    IEnumerable<WorkerSummaryDto> workerSummaries = workers.Select(w => w.ToWorkerSummaryDto());
    
    return Ok(workerSummaries);
  }
}