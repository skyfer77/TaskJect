using AutoMapper;

namespace Domain.Mapper
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<Team, TeamDto>(). ;
                config.CreateMap<Membership, MembershipDto>();
                config.CreateMap<Organization, OrganizationDto>();
                config.CreateMap<Project, ProjectDto>();
                config.CreateMap<Domain.Database.Task, TaskDto>();
                config.CreateMap<TariffPlan, TariffPlanDto>();
                config.CreateMap<TariffPlanHistory, TariffPlanHistoryDto>();
                config.CreateMap<OrganizationAppeal, OrganizationAppealDto>();
                config.CreateMap<ApplicationUserLite, ApplicationUserLiteDto>();
                config.CreateMap<ProjectUserPermission, ProjectUserPermissionDto>();
                config.CreateMap<OrganizationFiles, OrganizationFilesDto>();
                config.CreateMap<PersonalTodo, PersonalTodoDto>();
                config.CreateMap<PersonalTodoTask, PersonalTodoTaskDto>();
                config.CreateMap<PersonalNote, PersonalNoteDto>();
                config.CreateMap<Notification, NotificationDto>();

                config.CreateMap<ApplicationUser, ApplicationUserDto>();


            });
            return mappingConfig;
        }
    }
}
