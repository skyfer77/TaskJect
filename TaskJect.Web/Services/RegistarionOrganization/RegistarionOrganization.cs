using Domain.Database;
using Data.DbContexts;
using TaskJect.Web.Enums;
using TaskJect.Web.Models;
using TaskJect.Web.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Data;
using AutoMapper;
using Domain.Enums;

namespace TaskJect.Web.Services
{
	public class RegistarionOrganization : IRegistarionOrganization
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IStringLocalizer<ErrorResources> _localizer;
		private readonly IMapper _mapper;
		public RegistarionOrganization(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
			IStringLocalizer<ErrorResources> localizer , IMapper mapper)
		{
			_context = context;
			_userManager = userManager;
			_localizer = localizer;
            _mapper = mapper;

        }

		public async Task<ServerResponse> RegistarionNewOrganization(RegisterViewModel model)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				var existsOrganization = await _context.Organizations
					.AnyAsync(o => o.Name == model.OrganizationName);

				if (existsOrganization)
				{
					await transaction.RollbackAsync();
					return new ServerResponse(false) 
					{ 
						Errors = new() { _localizer["CreateOrganizationWasSuccessfulAlreadyExists"] } 
					};
				}

				var organization = new Organization
				{
					OrganizationId = Guid.NewGuid(),
					Name = model.OrganizationName,
					RegistrationDate = DateTime.UtcNow
				};

				_context.Organizations.Add(organization);

				var user = new ApplicationUser
				{
					UserName = model.Email,
					Email = model.Email,
					Name = model.FirstName,
					Surname = model.LastName,
					OrganizationCode = organization.OrganizationId.ToString(),
					RegistrationDate = DateTime.UtcNow,
					RoleInOrganization = _mapper.Map<OrganizationRoles>(OrganizationRolesView.TeamLead),
					//Щоб не треба було скидати пароль
					IsNewUser = false,
				};

				var createResult = await _userManager.CreateAsync(user, model.Password);
				if (!createResult.Succeeded)
				{
					await transaction.RollbackAsync();
					return new ServerResponse(false)
					{
						Errors = localizeIdentityErrors(createResult.Errors)
					};
				}

				await _userManager.AddToRoleAsync(user, "TeamLead");

				var plan = new TariffPlanHistory
				{
					OrganizationCode = organization.OrganizationId,
					TariffPlanCode = SD.BasicPlanCode,
					DateFrom = DateTime.UtcNow.Date,
					DateTo = new DateTime(9999, 12, 31, 23, 59, 59)
				};

				_context.TariffPlansHistories.Add(plan);
				await _context.SaveChangesAsync();

				await transaction.CommitAsync();
				return new ServerResponse(true);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return new ServerResponse(false)
				{ 
					Errors = new() { _localizer["CreateOrganizationWasNotSuccessful"] }
				};
			}
		}

		private List<string> localizeIdentityErrors(IEnumerable<IdentityError> errors)
		{
			var messages = new List<string>();

			foreach (var error in errors)
			{
				//UserName і Email однакові 
				if (error.Code == "DuplicateUserName")
				{
					continue;
				}

				messages.Add(error.Description);
			}

			return messages;
		}

	}
}
