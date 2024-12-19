using AutoMapper;
using PlanWise.Application.DTOs;
using PlanWise.Domain.Entities;

namespace PlanWise.Application.Mappings;

public class DomainToMappingUser
{
    public static MapperConfiguration RegisterMaps()
    {
        var mappingConfiguration = new MapperConfiguration(config =>
        {
            config.CreateMap<UserVO, User>();
        });

        return mappingConfiguration;
    }
}
