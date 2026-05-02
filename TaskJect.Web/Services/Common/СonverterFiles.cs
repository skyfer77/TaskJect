using TaskJect.Web.Models;
using Domain.Database;
namespace TaskJect.Web.Services
{
	public static class СonverterFiles
	{
		public static async Task<OrganizationFilesDto> ConverterFilesToOrganizationFilesDtoAsync(FileConversionRequest request)
		{
			var orgFile = new OrganizationFilesDto();

			if (request.File != null)
			{
				using var ms = new MemoryStream();
				await request.File.CopyToAsync(ms);

				orgFile = new OrganizationFilesDto
				{
					Id = Guid.NewGuid(),
					FileName = request.File.FileName,
					ContentType = request.File.ContentType,
					Content = ms.ToArray(),
					Size = request.File.Length,
					TaskId = request.TaskId,
					ProjectId = request.ProjectId,
					OrganizationCode = Guid.Parse(request.OrganizationCode),
					DateUploaded = DateTime.UtcNow
				};
			}

			return orgFile;
		}
	}
}
