﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizMaster.Models
{
    public class QuizGroup
    {
        public QuizGroup()
        {
            Quizes = new List<Quiz>();
        }

        public Guid QuizGroupId { get; set; }
        [Required]
        public Guid QuizCategoryId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Code { get; set; }
        public string Description { get; set; }
        public string ArticleContentHtml { get; set; }

        public QuizCategory QuizCategory { get; set; }
        public List<Quiz> Quizes { get; set; }
    }
}
