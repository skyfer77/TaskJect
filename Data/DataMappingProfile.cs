using AutoMapper;
using Domain.Database;

namespace Data.Mapper
{
    public class DataMappingProfile : Profile
    {
        public DataMappingProfile()
        {
            CreateMap<Team, TeamDto>().ReverseMap();
            CreateMap<Membership, MembershipDto>().ReverseMap();
            CreateMap<Organization, OrganizationDto>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();

            CreateMap<Domain.Database.Task, TaskDto>().ReverseMap();
            CreateMap<TariffPlan, TariffPlanDto>().ReverseMap();
            CreateMap<TariffPlanHistory, TariffPlanHistoryDto>().ReverseMap();
            CreateMap<OrganizationAppeal, OrganizationAppealDto>().ReverseMap();

            CreateMap<ApplicationUserLite, ApplicationUserLiteDto>().ReverseMap();
            CreateMap<ProjectUserPermission, ProjectUserPermissionDto>().ReverseMap();
            CreateMap<OrganizationFiles, OrganizationFilesDto>().ReverseMap();

            CreateMap<PersonalTodo, PersonalTodoDto>().ReverseMap();
            CreateMap<PersonalTodoTask, PersonalTodoTaskDto>().ReverseMap();
            CreateMap<PersonalNote, PersonalNoteDto>().ReverseMap();

            CreateMap<Notification, NotificationDto>().ReverseMap();
            CreateMap<ApplicationUser, ApplicationUserDto>().ReverseMap();

            CreateMap<PaymentWayForPay, PaymentWayForPayDto>().ReverseMap();
            CreateMap<PaymentInvoice, PaymentInvoiceDto>().ReverseMap();
        }
    }
}
