using Domain.Database;
using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TaskJect.Web.Controllers
{
	public class PersonalNoteController : Controller
	{
		private readonly IPersonalNoteRepository _personalNoteRepository;
		private readonly IStringLocalizer<ErrorResources> _localizer;

		public PersonalNoteController(
			IPersonalNoteRepository personalNoteRepository, 
			IStringLocalizer<ErrorResources> localizer)
		{
			_personalNoteRepository = personalNoteRepository;
			_localizer = localizer;
		}

		public async Task<IActionResult> Index()
		{
			var userId = this.GetUserId();

			var notes = await _personalNoteRepository.Retrieve(userId);

			return View(notes);
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			var html = await this.RenderViewAsync("_CreateCardNote");

			return Json(new ServerResponse(true)
			{
				Html = html,
			});
		}

		[HttpPost]
		public async Task<IActionResult> CreateNote(PersonalNoteDto model)
		{
			if (string.IsNullOrWhiteSpace(model.Title))
			{
				return Json(new ServerResponse(false)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			model.Id = Guid.NewGuid();
			model.UserId = this.GetUserId();
			model.CreatedAt = DateTime.UtcNow;
			model.UpdatedAt = DateTime.UtcNow;

			var result = await _personalNoteRepository.Insert(model);

			if (!result)
			{
				return Json(new ServerResponse(result)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			var html = await this.RenderViewAsync("_CardNote", model);

			return Json(new ServerResponse(result)
			{
				Html = html,
			});
		}

		[HttpPost]
		public async Task<IActionResult> UpdateNoteTitle(PersonalNoteDto model)
		{
			if (string.IsNullOrWhiteSpace(model.Title))
			{
				return Json(new ServerResponse(false)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			model.UpdatedAt = DateTime.UtcNow;

			var result = await _personalNoteRepository.Update(model);

			if (!result)
			{
				return Json(new ServerResponse(result)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			return Json(new ServerResponse(result)
			{
				UpdatedAt = model.UpdatedAt
			});
		}

		[HttpPost]
		public async Task<IActionResult> UpdateNoteText(PersonalNoteDto model)
		{
			model.UpdatedAt = DateTime.UtcNow;

			var result = await _personalNoteRepository.Update(model);

			if (!result)
			{
				return Json(new ServerResponse(result)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			return Json(new ServerResponse(result)
			{
				UpdatedAt = model.UpdatedAt
			});
		}

		[HttpPost]
		public async Task<IActionResult> DeleteNote(Guid noteId)
		{
			if (Guid.Empty == noteId)
			{
				return Json(new ServerResponse(false)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			var result = await _personalNoteRepository.Delete(noteId);

			if (!result)
			{
				return Json(new ServerResponse(result)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			return Json(new ServerResponse(result));
		}
	}
}
