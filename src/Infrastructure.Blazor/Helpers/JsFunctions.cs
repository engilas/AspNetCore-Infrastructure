using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Infrastructure.Blazor.Helpers
{
    public static class JsFunctions
    {
        public static void Alert(string message)
        {
            Call("alert", message);
        }

        public static Task<bool> Confirm(string message)
        {
            return CallAsync<bool>("confirm", message);
        }

        public static void LoadScript(string path)
        {
            Call("loadScript", path);
        }

        public static void SetInstance(object instance)
        {
            Call("setInstance", new DotNetObjectRef(instance));
        }

        public static void Eval(string script)
        {
            ((IJSInProcessRuntime)JSRuntime.Current).InvokeAsync<object>("evalJs", script);
        }

        public static T Call<T>(string identifier, params object[] args)
        {
            return ((IJSInProcessRuntime)JSRuntime.Current).Invoke<T>(identifier, args);
        }

        public static void Call(string identifier, params object[] args)
        {
            ((IJSInProcessRuntime)JSRuntime.Current).Invoke<object>(identifier, args);
        }

        public static Task<T> CallAsync<T>(string identifier, params object[] args)
        {
            return JSRuntime.Current.InvokeAsync<T>(identifier, args);
        }

        public static Task CallAsync(string identifier, params object[] args)
        {
            return JSRuntime.Current.InvokeAsync<object>(identifier, args);
        }
    }
}
