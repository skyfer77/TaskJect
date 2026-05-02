using Domain.Database;
using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using AutoMapper;

namespace TaskJect.Web.Controllers
{
	public class PersonalTodoController : Controller
	{
		private readonly IPersonalTodoRepository _personalTodoRepository;
		private readonly IPersonalTodoTaskRepository _personalTodoTaskRepository;
		private readonly ITaskRepository _taskRepository;
		private readonly IOrganizationFilesRepository _organizationFilesRepository;
		private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IStringLocalizer<ErrorResources> _localizer;
        private readonly ITariffPlanRepository _tariffPlanRepository;
        private readonly ITariffPlanHistoryRepository _tariffPlanHistoryRepository;
        private readonly IMapper _mapper;
		public PersonalTodoController(IPersonalTodoRepository personalTodoRepository, 
			IPersonalTodoTaskRepository personalTodoTaskRepository, 
			ITaskRepository taskRepository,
			IOrganizationFilesRepository organizationFilesRepository, 
			IApplicationUserRepository applicationUserRepository, 
			IStringLocalizer<ErrorResources> localizer,
            IOrganizationRepository organizationRepository,
            ITariffPlanRepository tariffPlanRepository, ITariffPlanHistoryRepository tariffPlanHistoryRepository,
            IMapper mapper)
		{
			_personalTodoRepository = personalTodoRepository;
			_personalTodoTaskRepository = personalTodoTaskRepository;
			_taskRepository = taskRepository;
			_organizationFilesRepository = organizationFilesRepository;
			_applicationUserRepository = applicationUserRepository;
            _organizationRepository = organizationRepository;
            _localizer = localizer;
            _tariffPlanRepository = tariffPlanRepository;
            _tariffPlanHistoryRepository = tariffPlanHistoryRepository;
            _mapper = mapper;

        }

		#region ToDo
		public async Task<IActionResult> Index()
		{
			var userId = this.GetUserId();
			var organizationCode = this.GetOrganizationCode();

			var personalTodos = await _personalTodoRepository.Retrieve(userId);

			var tasks = await _taskRepository.RetriveByUser(userId, organizationCode, 10);

			var viewModel = new TodoPageViewModel
			{
				PersonalTodos = personalTodos,
				Tasks = tasks
			};

			return View(viewModel);
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			var html = await this.RenderViewAsync("_CreateCardTodo");

			return Json(new ServerResponse(true)
			{
				Html = html,
			});
		}

		[HttpPost]
		public async Task<IActionResult> CreateTodo(PersonalTodoDto model)
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
			
			var result = await _personalTodoRepository.Insert(model);

			if (!result)
			{
				return Json(new ServerResponse(result) 
				{ 
					Message = _localizer["YourOperationWasNotSuccessful"] 
				});
			}

			var html = await this.RenderViewAsync("_CardTodo", model);

			return Json(new ServerResponse(result)
			{
				Html = html,
			});
		}

		[HttpPost]
		public async Task<IActionResult> UpdateTodo(PersonalTodoDto model)
		{
			if (string.IsNullOrWhiteSpace(model.Title))
			{
				return Json(new ServerResponse(false)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			model.UpdatedAt = DateTime.UtcNow;

			var result = await _personalTodoRepository.Update(model);

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
		public async Task<IActionResult> DeleteTodo(Guid todoId)
		{
			if (Guid.Empty == todoId)
			{
				return Json(new ServerResponse(false)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			var result = await _personalTodoRepository.Delete(todoId);

			if (!result)
			{
				return Json(new ServerResponse(result)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			return Json(new ServerResponse(result));
		}

		[HttpPost]
		public async Task<IActionResult> CreateTodoTask(PersonalTodoTaskDto model)
		{
			if (string.IsNullOrWhiteSpace(model.Text) && model.TodoId == Guid.Empty)
			{
				return Json(new ServerResponse(false)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			model.Id = Guid.NewGuid();
			model.CreatedAt = DateTime.UtcNow;
			model.IsDone = false;

			var result = await _personalTodoTaskRepository.Insert(model);

			if (!result)
			{
				return Json(new ServerResponse(result)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			var updatedAt = await _personalTodoRepository.SetUpdatedAtNow(model.TodoId);

			var html = await this.RenderViewAsync("_LiTodoTaskPartial", model);

			return Json(new ServerResponse(result)
			{
				Html = html,
				UpdatedAt = updatedAt,
			});
		}

		[HttpPost]
		public async Task<IActionResult> ToggleTaskStatus(PersonalTodoTaskDto model)
		{
			var result = await _personalTodoTaskRepository.ToggleStatus(model.Id, model.IsDone);
			if (!result)
			{
				return Json(new ServerResponse(result)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			var updatedAt = await _personalTodoRepository.SetUpdatedAtNow(model.TodoId);

			return Json(new ServerResponse(result)
			{
				UpdatedAt = updatedAt,
			});
		}

		[HttpPost]
		public async Task<IActionResult> UpdateTodoTask(PersonalTodoTaskDto model)
		{
			if (string.IsNullOrWhiteSpace(model.Text))
			{
				return Json(new ServerResponse(false)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			var result = await _personalTodoTaskRepository.Update(model);

			if (!result)
			{
				return Json(new ServerResponse(result)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			var updatedAt = await _personalTodoRepository.SetUpdatedAtNow(model.TodoId);

			return Json(new ServerResponse(result)
			{
				UpdatedAt = updatedAt,
			});
		}

		[HttpPost]
		public async Task<IActionResult> DeleteTodoTask(Guid taskId, Guid todoId)
		{
			if (Guid.Empty == taskId)
			{
				return Json(new ServerResponse(false)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			var result = await _personalTodoTaskRepository.Delete(taskId);

			if (!result)
			{
				return Json(new ServerResponse(result)
				{
					Message = _localizer["YourOperationWasNotSuccessful"]
				});
			}

			var updatedAt = await _personalTodoRepository.SetUpdatedAtNow(todoId);

			return Json(new ServerResponse(result)
			{
				UpdatedAt = updatedAt,
			});
		}

		#endregion

		#region User task


		[HttpPost]
		public async Task<ActionResult> OverviewTask(Guid id)
		{
			var userId = this.GetUserId();
			var organizationCode = this.GetOrganizationCode();
            var organizationId = Guid.Parse(organizationCode);
            var task = await _taskRepository.Retrieve(id, organizationCode);
			if (task == null)
			{
				return this.RedirectToErrorPage(_localizer["tasksByUserLoadErrorTitle"], _localizer["tasksByUserLoadErrorMessage"]);
			}

			var user = await _applicationUserRepository.GetUserById(userId, organizationCode);
			if (user == null)
			{
				return this.RedirectToErrorPage(_localizer["NoFoundUsersTitle"], _localizer["NoFoundUsersMessage"]);
			}
			task.User = user;
            var activePlan = await _tariffPlanHistoryRepository.RetrieveActive(organizationId);
            var currentPlan = await _tariffPlanRepository.Retrieve(activePlan.TariffPlanCode);
            ViewBag.HasGitHubIntegration = currentPlan.HasGitHubIntegration;

            var taskFiles = await _organizationFilesRepository.RetrieveLightTaskFile(id);

            var installationId = await _organizationRepository.FindGitHubInstallationId(Guid.Parse(organizationCode));
            var overviewTask = new OverviewTaskModel()
			{
				Task = _mapper.Map<TaskView>(task),
				OrganizationFiles = taskFiles.ToList(),
                GitHubIntegration = installationId != null
            };

			return PartialView("_OverviewTask", overviewTask);
		}
		#endregion
	}
}
