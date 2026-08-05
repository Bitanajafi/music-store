using System;
using System.Collections.Generic;
using System.Text;
using MusicStore.Application.DTOs.Enums;

namespace MusicStore.Application.DTOs.Users
{
    public class UserFilterDto
    {
        public string? Search { get; set; }
        public UserSortType Sort { get; set; }
    }
}
