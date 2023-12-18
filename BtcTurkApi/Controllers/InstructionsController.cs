using BtcTurkApi.Data;
using BtcTurkApi.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BtcTurkApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructionsController : ControllerBase
    {
        private BtcTurkDbContext _btcTurkDbContext;

        public InstructionsController(BtcTurkDbContext context)
        {
            _btcTurkDbContext = context;
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateInstruction([FromBody] CreateInstrutionRequest request)
        {
            if (request.Amount < 100 || request.Amount > 20000) return BadRequest("Amount is must bigger than 100 and smaller than 20000");
            if (!(request.UserId >= 1)) return BadRequest("UserId must bigger than 0");
            if (request.Date < 0 && request.Date > 28) return BadRequest("Date is must bigger than 0 and smaller than 29");
            if ((await _btcTurkDbContext.Instructions.Where(x => x.UserId == request.UserId && x.IsActive == true).ToListAsync()).Count > 0) return BadRequest("User must have at most 1 active instruction");

            var instruction = new Instruction()
            {
                Amount = request.Amount,
                IsActive = true,
                UserId = request.UserId,
                Date = request.Date,
                Sms = request.Sms,
                Email = request.Email,
                PushNotification = request.PushNotification,
                CreatedDate = DateTime.UtcNow,
            };

            await _btcTurkDbContext.Instructions.AddAsync(instruction);

            await _btcTurkDbContext.SaveChangesAsync();

            //@TODO Hayali bir Http isteği ile bildirim örnekleme?

            return Ok(instruction);
        }

        [HttpGet("{userId:int}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveInstructionByUserId(int userId)
        {
            var result = await _btcTurkDbContext.Instructions.Where(x => x.UserId == userId && x.IsActive == true).FirstOrDefaultAsync();
            if (result == null) return NotFound("This user does not have any instruction");

            return Ok(result);
        }

        [HttpGet("{userId:int}/cancelled-instructions")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCancelledInstructionsByUserId(int userId)
        {
            var result = await _btcTurkDbContext.Instructions.Where(x => x.UserId == userId && x.IsActive == false).ToListAsync();

            if (result.Count == 0) return NotFound("This user does not have any cancelled instruction");

            return Ok(result);
        }

        [HttpPut("{instructionId:int}/update")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateInstruction([FromRoute] int instructionId, [FromBody] UpdateInstrutionRequest request)
        {
            if (request.Amount < 100 || request.Amount > 20000) return BadRequest("Amount is must bigger than 100 and smaller than 20000");
            if (!(request.UserId >= 1)) return BadRequest("UserId must bigger than 0");
            if (request.Date < 0 && request.Date > 28) return BadRequest("Date is must bigger than 0 and smaller than 29");

            var anyActiveInstruction = await _btcTurkDbContext.Instructions.Where(x => x.Id != instructionId && x.UserId == request.UserId && x.IsActive == true).FirstOrDefaultAsync();
            if (request.State == true && anyActiveInstruction != null) return BadRequest("User must have at most 1 active instruction");

            var instruction = await _btcTurkDbContext.Instructions.Where(x => x.Id == instructionId && x.UserId == request.UserId).FirstOrDefaultAsync();
            if (instruction == null) return NotFound("The chosen instruction neither does not exist or not belong to you!");

            instruction.Amount = request.Amount;
            instruction.IsActive = request.State;
            instruction.Date = request.Date;         

            _btcTurkDbContext.Update(instruction);
            _btcTurkDbContext.SaveChanges();

            return Ok(instruction);
        }


        [HttpGet("{instructionId:int}/notifications-record")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotifications([FromRoute] int instructionId)
        {
            var instruction = await _btcTurkDbContext.Instructions.Where(x => x.Id == instructionId).FirstOrDefaultAsync();

            if (instruction == null)
            {
                return NotFound($"There is no instruction about this instruction id: {instructionId}");
            }

            string message = "Notification messages(s) such as ";

            if (instruction.Sms) message += "Sms ";
            if (instruction.Email) message += "Email ";
            if (instruction.PushNotification) message += "Push notification ";

            message += $"for successful instruction with id:{instruction.Id} Date: {instruction.CreatedDate}";

            return Ok(message);
        }

        [HttpGet("{instructionId:int}/notification-channels")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotificationChannels([FromRoute] int instructionId)
        {
            var instruction = await _btcTurkDbContext.Instructions.Where(x => x.Id == instructionId).FirstOrDefaultAsync();

            if (instruction == null)
            {
                return NotFound($"There is no instruction about this instruction id: {instructionId}");
            }

            List<string> notificationChannels = new List<string>();

            if (instruction.Sms) notificationChannels.Add("Sms");
            if (instruction.Email) notificationChannels.Add("Email");
            if (instruction.PushNotification) notificationChannels.Add("PushNotification");

            return notificationChannels.Count > 0
                ? Ok(notificationChannels)
                : NotFound($"There is no notification order about this instruction id: {instructionId}");
        }
    }
}
