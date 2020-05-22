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
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        public IActionResult GetTaskboard(int id, int userId, bool includeCards = false)
        {
            var taskboardsForUser = _ninjaOrganizerRepository.GetTaskboardsForUser(userId);
            var taskboard = taskboardsForUser.FirstOrDefault(t => t.Id == id);

            if (taskboard == null)
                return NotFound();

            if (!_ninjaOrganizerRepository.TaskboardExists(taskboard.Id))
                return NotFound();

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
        public IActionResult DeleteTaskboard(int id, int userId)
        {
            var taskboardsForUser = _ninjaOrganizerRepository.GetTaskboardsForUser(userId);
            var taskboard = taskboardsForUser.FirstOrDefault(t => t.Id == id);
            if (taskboard == null)
                return NotFound();

            if (!_ninjaOrganizerRepository.TaskboardExists(taskboard.Id))
                return NotFound();

            _ninjaOrganizerRepository.DeleteTaskboard(taskboard);
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
        public IActionResult PartiallyUpdateTaskboard(int id, int userId, [FromBody] TaskboardDto taskboardForUpdate)
        {
            var taskboardsForUser = _ninjaOrganizerRepository.GetTaskboardsForUser(userId);
            var taskboard = taskboardsForUser.FirstOrDefault(t => t.Id == id);

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
        public IActionResult UpdateTaskboard(int id, int userId, [FromBody] TaskboardForUpdateDto taskboard)
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

            var taskboardsForUser = _ninjaOrganizerRepository.GetTaskboardsForUser(userId);
            var ent_taskboard = taskboardsForUser.FirstOrDefault(t => t.Id == id);

            if (!_ninjaOrganizerRepository.TaskboardExists(ent_taskboard.Id) || ent_taskboard == null)
            {
                return NotFound();
            }

            _mapper.Map(taskboard, ent_taskboard);
            _ninjaOrganizerRepository.UpdateTaskboard(id, ent_taskboard);
            _ninjaOrganizerRepository.Save();

            return NoContent();
        }

    }
}
