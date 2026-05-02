using TaskJect.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace TaskJect.Web.Models.Account
{
	public class ChangePasswordModel
	{
		[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
			ErrorMessageResourceType = typeof(ErrorResources))]
		[DataType(DataType.Password)]
		[Display(Name = "CurrentPassword", ResourceType = typeof(SharedResources))]
		public string CurrentPassword { get; set; }

		[Required(ErrorMessageResourceName = "RequiredAttribute_ValidationError",
			ErrorMessageResourceType = typeof(ErrorResources))]
		[DataType(DataType.Password)]
		[Display(Name = "NewPassword", ResourceType = typeof(SharedResources))]
		public string NewPassword { get; set; }
	}
}
