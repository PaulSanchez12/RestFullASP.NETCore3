using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Models
{
    public abstract class CourseForManipulationDto
    {
        [Required(ErrorMessage = "You should fill out a title")]
        [MaxLength(100, ErrorMessage = "The tittle shouldn't have more than 100 characters.")]
        public string Title { get; set; }

        [MaxLength(1500, ErrorMessage = "The description shouldn't have more than 100 characters.")]
        public virtual string Description { get; set; }
    }
}
