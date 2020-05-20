using AutoMapper;
using NinjaOrganizer.API.Models;
using NinjaOrganizer.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NinjaOrganizer.API.Entities;

namespace NinjaOrganizer.API.Controllers
{
    /// <summary>
    /// This class is responsible for cards manipulating.
    /// Authorized access.
    /// </summary>

    [Authorize]
    [ApiController]
    [Route("users/{userId}/taskboards/{taskboardId}/cards")]
    public class CardsController : ControllerBase
    {
        private readonly ILogger<CardsController> _logger;
        private readonly INinjaOrganizerRepository _ninjaOrganizerRepository;
        private readonly IMapper _mapper;

        public CardsController(ILogger<CardsController> logger,
             INinjaOrganizerRepository ninjaOrganizerRepository,
            IMapper mapper)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _ninjaOrganizerRepository = ninjaOrganizerRepository ??
                throw new ArgumentNullException(nameof(ninjaOrganizerRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all cards for specific taskboard.
        /// </summary>
        /// <param name="taskboardId"></param>
        /// <returns>Ok if success.</returns>
        [HttpGet]
        public IActionResult GetCards(int taskboardId)
        {
            try
            {
                if (!_ninjaOrganizerRepository.TaskboardExists(taskboardId))
                {
                    _logger.LogInformation($"Taskboard with id {taskboardId} wasn't found when " +
                        $"accessing cards.");
                    return NotFound();
                }

                var cardsForTaskboard = _ninjaOrganizerRepository.GetCardsForTaskboard(taskboardId);
                return Ok(_mapper.Map<IEnumerable<CardDto>>(cardsForTaskboard));
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting cards for taskboard with id {taskboardId}.", ex);
                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        /// <summary>
        /// Get specific card for specific taskboard.
        /// </summary>
        /// <param name="taskboardId"></param>
        /// <param name="id"></param>
        /// <returns>Ok if success.</returns>
        [HttpGet("{id}", Name = "GetCard")]
        public IActionResult GetCard(int taskboardId, int id)
        {
            if (!_ninjaOrganizerRepository.TaskboardExists(taskboardId))
            {
                return NotFound();
            }

            var card = _ninjaOrganizerRepository.GetCardForTaskboard(taskboardId, id);

            if (card == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CardDto>(card));
        }

        /// <summary>
        /// Create card for specific taskboard.
        /// </summary>
        /// <param name="taskboardId"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateCard(int taskboardId, [FromBody] CardForCreationDto card)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_ninjaOrganizerRepository.TaskboardExists(taskboardId))
            {
                return NotFound();
            }

            var finalCard = _mapper.Map<Entities.Card>(card);
            _ninjaOrganizerRepository.AddCardForTaskboard(taskboardId, finalCard);
            _ninjaOrganizerRepository.Save();

            var createdCardToReturn = _mapper.Map<Models.CardDto>(finalCard);

            // get this card
            return CreatedAtRoute("GetCard",
                new { taskboardId, id = createdCardToReturn.Id },
                createdCardToReturn);
        }

        /// <summary>
        /// Update specific card for specific taskboard.
        /// </summary>
        /// <param name="taskboardId"></param>
        /// <param name="id"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult UpdateCard(int taskboardId, int id, [FromBody] CardForUpdateDto card)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_ninjaOrganizerRepository.TaskboardExists(taskboardId))
            {
                return NotFound();
            }

            var cardEntity = _ninjaOrganizerRepository.GetCardForTaskboard(taskboardId, id);
            if (cardEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(card, cardEntity);

            _ninjaOrganizerRepository.UpdateCard(taskboardId, cardEntity);
            _ninjaOrganizerRepository.Save();

            return NoContent();
        }

        /// <summary>
        /// Partially update specific card for specific taskboard.
        /// </summary>
        /// <param name="taskboardId"></param>
        /// <param name="id"></param>
        /// <param name="cardForUpdate"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateCard(int taskboardId, int id, [FromBody] CardDto cardForUpdate)
        {
            var card = _ninjaOrganizerRepository.GetCardForTaskboard(taskboardId, id);
            if (card == null)
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (cardForUpdate.Title != null)
                card.Title = cardForUpdate.Title;
            if (cardForUpdate.Content != null)
                card.Content = cardForUpdate.Content;
            if (cardForUpdate.State != card.State && (int)cardForUpdate.State != 0)
                if (enumIsOk((int)cardForUpdate.State))
                    card.State = cardForUpdate.State;
                else return BadRequest("Wrong state of State");
            if (cardForUpdate.Priority != card.Priority && (int)cardForUpdate.Priority != 0)
                if (enumIsOk((int)cardForUpdate.Priority)) 
                    card.Priority = cardForUpdate.Priority;
                else return BadRequest("Wrong state of Priority");

            _ninjaOrganizerRepository.UpdateCard(taskboardId, card);
            _ninjaOrganizerRepository.Save();

            return NoContent();
        }

        /// <summary>
        /// Check is state status is correct.
        /// </summary>
        /// <param name="state"></param>
        /// <returns>True if state is correct, else False.</returns>
        private bool enumIsOk(int state)
        {
            if (state <= 0) return false;
            if (state > 3) return false;
            return true;
        }


        /// <summary>
        /// Delete specific card for specific taskboard.
        /// </summary>
        /// <param name="taskboardId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult DeleteCard(int taskboardId, int id)
        {
            if (!_ninjaOrganizerRepository.TaskboardExists(taskboardId))
            {
                return NotFound();
            }

            var cardEntity = _ninjaOrganizerRepository
                .GetCardForTaskboard(taskboardId, id);
            if (cardEntity == null)
            {
                return NotFound();
            }

            _ninjaOrganizerRepository.DeleteCard(cardEntity);
            _ninjaOrganizerRepository.Save();

            return NoContent();
        }
    }
}
