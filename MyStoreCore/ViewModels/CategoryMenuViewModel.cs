namespace MyStoreCore.ViewModels
{
    public class CategoryMenuViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<CategoryMenuViewModel> Children { get; set; } = new();
    }
}
