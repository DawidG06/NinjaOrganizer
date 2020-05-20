using AutoMapper;
using NinjaOrganizer.API.Models;
using NinjaOrganizer.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;

namespace NinjaOrganizer.API.Controllers
{
    /// <summary>
    /// This class is responsible for taskboard manipulating.
    /// Authorized access.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("users/{userId}/taskboards")]
    public class TaskboardsController : ControllerBase
    {
        private readonly INinjaOrganizerRepository _ninjaOrganizerRepository;
        private readonly IMapper _mapper;

        public TaskboardsController(INinjaOrganizerRepository ninjaOrganizerRepository,
            IMapper mapper)
        {
            _ninjaOrganizerRepository = ninjaOrganizerRepository ??
                throw new ArgumentNullException(nameof(ninjaOrganizerRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get all taskboards for specific user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetTaskboards(int userId)
        {
            var taskboardsForUser = _ninjaOrganizerRepository.GetTaskboardsForUser(userId);

            foreach(var taskboard in taskboardsForUser)
                taskboard.Cards = _ninjaOrganizerRepository.GetCardsForTaskboard(taskboard.Id).ToList();

            return Ok(_mapper.Map<IEnumerable<TaskboardWithoutCardsDto>>(taskboardsForUser));
        }

        /// <summary>
        /// Get specific taskboard.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includeCards">Allow to return taskboard with cards. This parameter is default false.</param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetTaskboard")]
        public IActionResult GetTaskboard(int id, bool includeCards = false)
        {
            var taskboard = _ninjaOrganizerRepository.GetTaskboard(id, includeCards);
            if (taskboard == null)
            {
                return NotFound();
            }

            if (includeCards)
            {
                taskboard.Cards = _ninjaOrganizerRepository.GetCardsForTaskboard(taskboard.Id).ToList();
                return Ok(_mapper.Map<TaskboardDto>(taskboard));
            }

            return Ok(_mapper.Map<TaskboardWithoutCardsDto>(taskboard));
        }

        /// <summary>
        /// Create taskboard for specific user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="taskboard"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateTaskboard(int userId, [FromBody] TaskboardForCreationDto taskboard)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var finalTaskboard = _mapper.Map<Entities.Taskboard>(taskboard);
            finalTaskboard.UserId = userId;

            _ninjaOrganizerRepository.AddTaskboard(finalTaskboard);
            _ninjaOrganizerRepository.Save();

            var createdTaskboardToReturn = _mapper.Map<Models.TaskboardDto>(finalTaskboard);

            //get this taskboard
            return CreatedAtRoute("GetTaskboard",
                new { id = createdTaskboardToReturn.Id },
                createdTaskboardToReturn);
        }

        /// <summary>
        /// Delete specific taskboard.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult DeleteTaskboard(int id)
        {
            if (!_ninjaOrganizerRepository.TaskboardExists(id))
                return NotFound();

            var taskboardEntity = _ninjaOrganizerRepository.GetTaskboard(id, false);
            if (taskboardEntity == null)
                return NotFound();

            _ninjaOrganizerRepository.DeleteTaskboard(taskboardEntity);
            _ninjaOrganizerRepository.Save();

            return NoContent();
        }

        /// <summary>
        /// Partially update specific taskboard.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="taskboardForUpdate"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateTaskboard(int id, [FromBody] TaskboardDto taskboardForUpdate)
        {
            var taskboard = _ninjaOrganizerRepository.GetTaskboard(id,false);
            if (taskboard == null)
                return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (taskboardForUpdate.Title != null)
                taskboard.Title = taskboardForUpdate.Title;
            if (taskboardForUpdate.Description != null)
                taskboard.Description = taskboardForUpdate.Description;

            _ninjaOrganizerRepository.UpdateTaskboard(id, taskboard);
            _ninjaOrganizerRepository.Save();

            return NoContent();
            //return base.Content("W trakcie implementacji...");
        }

        /// <summary>
        /// Update specific taskboard.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="taskboard"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult UpdateTaskboard(int id,[FromBody] TaskboardForUpdateDto taskboard)
        {
            if (taskboard.Description == taskboard.Title)
            {
                ModelState.AddModelError(
                    "Description",
                    "The provided description should be different from the name.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_ninjaOrganizerRepository.TaskboardExists(id))
            {
                return NotFound();
            }

            var taskboardEntity = _ninjaOrganizerRepository.GetTaskboard(id, false);
            if (taskboardEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(taskboard, taskboardEntity);
            _ninjaOrganizerRepository.UpdateTaskboard(id, taskboardEntity);
            _ninjaOrganizerRepository.Save();

            return NoContent();
        }

    }
}
