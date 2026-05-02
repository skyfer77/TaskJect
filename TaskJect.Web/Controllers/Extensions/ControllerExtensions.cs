using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TaskJect.Web.Controllers
{
	//TODO: Використовувати в усіх контроллерах і додати ще загалні речі 
	public static class ControllerExtensions
	{
		/// <summary>
		/// Рендерить Razor Partial View в HTML-рядок із переданою моделлю.
		/// </summary>
		/// <param name="controller">Контролер, який викликає метод.</param>
		/// <param name="viewName">Назва часткового представлення (Partial View), яке потрібно рендерити (без розширення).</param>
		/// <param name="model">Модель, яка передається у представлення.</param>
		/// <returns>HTML-рядок, згенерований із часткового представлення, або порожній рядок, якщо представлення не знайдено.</returns>
		public static async Task<string> RenderViewAsync(this Controller controller, string viewName, object model)
		{
			if (model != null)
			{
				controller.ViewData.Model = model;
			}

			using var writer = new StringWriter();
			var viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

			var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);

			if (viewResult.View == null)
			{
				return string.Empty;
			}

			var viewContext = new ViewContext(
				controller.ControllerContext,
				viewResult.View,
				controller.ViewData,
				controller.TempData,
				writer,
				new HtmlHelperOptions()
			);

			await viewResult.View.RenderAsync(viewContext);
			return writer.GetStringBuilder().ToString();
		}

		/// <summary>
		/// Рендерить Razor Partial View в HTML-рядок без передавання моделі.
		/// </summary>
		/// <param name="controller">Контролер, який викликає метод.</param>
		/// <param name="viewName">Назва часткового представлення (Partial View), яке потрібно рендерити (без розширення).</param>
		/// <returns>HTML-рядок, згенерований із часткового представлення, або порожній рядок, якщо представлення не знайдено.</returns>
		public static Task<string> RenderViewAsync(this Controller controller, string viewName)
		{
			return controller.RenderViewAsync(viewName, null);
		}

		/// <summary>
		/// Отримує ID поточного користувача з Claims
		/// </summary>
		/// /// <param name="controller">Контролер, з якого викликається метод.</param>
		/// <returns>ID користувача як рядок або null, якщо не знайдено.</returns>
		public static string? GetUserId(this Controller controller)
		{
			return controller.User?.FindFirstValue(ClaimTypes.NameIdentifier);
		}

		/// <summary>
		/// Отримує код організації поточного користувача з Claims.
		/// </summary>
		/// <param name="controller">Контролер, з якого викликається метод.</param>
		/// <returns>Код організації як рядок або null, якщо не знайдено.</returns>
		public static string? GetOrganizationCode(this Controller controller)
		{
			return controller.User?.Claims.FirstOrDefault(c => c.Type == "organization_code")?.Value;
		}

		/// <summary>
		/// Перенаправляє на сторінку помилки з переданим заголовком і повідомленням.
		/// Дозволяє вказати контролер і дію для редіректу.
		/// </summary>
		/// <param name="controller">Контролер, у якому викликається метод.</param>
		/// <param name="errorTitle">Заголовок помилки.</param>
		/// <param name="errorMessage">Повідомлення помилки.</param>
		/// <param name="actionName">Ім'я дії, за замовчуванням "Error".</param>
		/// <param name="controllerName">Ім'я контролера, за замовчуванням null.</param>
		/// <returns>Редірект на вказану дію і контролер.</returns>
		public static ActionResult RedirectToErrorPage(this Controller controller, string errorTitle, string errorMessage, string actionName = "Error", string controllerName = null)
		{
			controller.TempData["ErrorTitle"] = errorTitle;
			controller.TempData["ErrorMessage"] = errorMessage;

			controllerName ??= controller.ControllerContext.RouteData.Values["controller"]?.ToString() ?? "Home";

			return controller.RedirectToAction(actionName, controllerName);
		}

		/// <summary>
		/// Отримує культуру користувача з його клеймів.
		/// Якщо користувач не аутентифікований або клейм не знайдено — повертає англійську "en" за замовчуванням.
		/// </summary>
		/// <param name="controller">Контролер, з якого витягуємо користувача</param>
		/// <returns>Назва культури у форматі "uk", "en" тощо</returns>
		public static string GetUserCulture(this Controller controller)
		{
			return controller.User?.Identity?.IsAuthenticated == true
				? controller.User.Claims.FirstOrDefault(c => c.Type == "culture")?.Value ?? "en"
				: "en";
		}
	}
}
