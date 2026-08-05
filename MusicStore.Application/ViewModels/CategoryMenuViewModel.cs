using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.ViewModels
{
    public class CategoryMenuViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public List<CategoryMenuViewModel> Children { get; set; } = new();
    }
}
