﻿using AutoMapper;

namespace CourseLibrary.API.Profiles
{
    public class CoursesProfilecs : Profile
    {
        public CoursesProfilecs()
        {
            CreateMap<Entities.Course, Models.CourseDto>();
            CreateMap<Models.CourseForCreationDto, Entities.Course>();
            CreateMap<Models.CourseForUpdateDto, Entities.Course>();
            CreateMap<Entities.Course, Models.CourseForUpdateDto>();
        }
    }
}
