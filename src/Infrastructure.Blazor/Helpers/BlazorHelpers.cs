using System.Threading.Tasks;

namespace Infrastructure.Blazor.Helpers
{
    public static class BlazorHelpers
    {
        public static async Task<bool> ConfirmDelete()
        {
            return await JsFunctions.Confirm("Are you sure you want to delete?");
        }
    }
}
