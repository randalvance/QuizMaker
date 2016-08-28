﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizMaker.Models
{
    public class ApplicationSetting
    {
        public Guid ApplicationSettingId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Value { get; set; }
        [Required]
        public ApplicationSettingValueType ApplicationSettingValueType { get; set; }
    }
}
