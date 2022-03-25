using AutoMapper;
using Prinubes.Common.DatabaseModels;

namespace Prinubes.Common.Helpers

{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserDatabaseModel, UserDisplayDataModel>();
            CreateMap<UserDisplayDataModel, UserDatabaseModel>();
            CreateMap<UserCRUDDataModel, UserDatabaseModel>();
            CreateMap<UserDatabaseModel, UserCRUDDataModel>();
            CreateMap<UserDatabaseModel, UserSimpleDataModel>();

            CreateMap<GroupCRUDDataModel, GroupDatabaseModel>();
            CreateMap<GroupDatabaseModel, GroupCRUDDataModel>();
            CreateMap<GroupDatabaseModel, GroupDisplayDataModel>();
            CreateMap<GroupDatabaseModel, GroupDisplayUsersDataModel>();
            CreateMap<GroupDatabaseModel, GroupKafkaDataModel>();
            CreateMap<GroupKafkaDataModel, GroupDatabaseModel>();


            CreateMap<OrganizationCRUDDataModel, OrganizationDatabaseModel>();
            CreateMap<OrganizationDatabaseModel, OrganizationCRUDDataModel>();
            CreateMap<OrganizationDatabaseModel, OrganizationDisplayDataModel>();
            CreateMap<OrganizationDatabaseModel, OrganizationSimpleDisplayDataModel>();


            CreateMap<CredentialCRUDDataModel, CredentialDatabaseModel>();
            CreateMap<CredentialDatabaseModel, CredentialCRUDDataModel>();
            CreateMap<CredentialDatabaseModel, CredentialDisplayDataModel>();
            CreateMap<CredentialDatabaseModel, CredentialKafkaDataModel>();
            CreateMap<CredentialKafkaDataModel, CredentialDatabaseModel>();

            CreateMap<NetworkPlatformDatabaseModel, NetworkPlatformDisplayDataModel>();
            CreateMap<ComputePlatformDatabaseModel, ComputePlatformDisplayDataModel>();
            CreateMap<LoadBalancerPlatformDatabaseModel, LoadBalancerPlatformDisplayDataModel>();

            //CreateMap<TaggingDatabaseModel, TaggingDisplayDataModel>();

        }
    }
}
