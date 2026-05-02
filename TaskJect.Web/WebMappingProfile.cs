using AutoMapper;
using Domain.Database;
using Domain.Enums;
using TaskJect.Web.Enums;
using TaskJect.Web.Models;

namespace TaskJect.Web.Mapping
{
    public class WebMappingProfile : Profile
    {
        public WebMappingProfile()
        {
            CreateMap<AppealStatus, AppealStatusView>().ReverseMap();
            CreateMap<OrganizationRoles, OrganizationRolesView>().ReverseMap();
            CreateMap<Period, PeriodView>().ReverseMap();
            CreateMap<Priority, PriorityView>().ReverseMap();
            CreateMap<QuickFilter, QuickFilterView>().ReverseMap();
            CreateMap<Domain.Enums.TaskStatus, TaskStatusView>().ReverseMap();
            CreateMap<SubscriptionPeriodType, SubscriptionPeriodTypeView>().ReverseMap();

            CreateMap<ApplicationUserLiteDto, ApplicationUserLiteView>()
                .ForMember(dest => dest.RoleInOrganization,
                           opt => opt.MapFrom(src => (OrganizationRolesView)src.RoleInOrganization));

            CreateMap<ApplicationUserLiteView, ApplicationUserLiteDto>()
                .ForMember(dest => dest.RoleInOrganization,
                           opt => opt.MapFrom(src => (OrganizationRoles)src.RoleInOrganization));

            CreateMap<TaskDto, TaskView>().
                ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => (TaskStatusView)src.Status));

            CreateMap<TaskView, TaskDto>().
                ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => (Domain.Enums.TaskStatus)src.Status));
        }
    }
}
