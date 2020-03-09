using Griffeye;
using System.Threading.Tasks;

namespace FauxFace
{
    public class Plugin
    {
        private readonly UserActionClient _userActionClient;

        readonly string BASE64_ICON_PNG =
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgAQMAAABJtOi3AAAABlBMVEUAAAAAAAClZ7nPAAAAAXRSTlMAQObYZgAA" +
            "ACNJREFUCNdjYCAM+P/b/4EQfAwWDLgJ/n/2PyAEfnVoBF4AALCjDTXMe2y+AAAAAElFTkSuQmCC";

        public Plugin(UserActionClient userActionClient)
        {
            this._userActionClient = userActionClient;
        }
        public async Task Initialize()
        {

            const string ActionNamePrefix = "FauxFacePlugin ";
            foreach (var userAction in await _userActionClient.GetAllUserActionsAsync())
            {
                if (userAction.ActionName.StartsWith(ActionNamePrefix))
                {
                    await _userActionClient.DeleteAsync(userAction.Id);
                }
            }

            await _userActionClient.AddUserActionAsync(
                new UserActionPost()
                {
                    ActionName = $"{ActionNamePrefix}Query",
                    ActionType = UserActionPostActionType.ApiCall,
                    Base64Image = BASE64_ICON_PNG,
                    Scope = UserActionPostScope.Entity,
                    Path = new string[] { "FauxFace", "Enqueue file(s) for identification" },
                    Url = "http://localhost:8780/plugin1/api/fauxface"
                });

            await _userActionClient.AddUserActionAsync(
                new UserActionPost()
                {
                    ActionName = $"{ActionNamePrefix}View Results",
                    ActionType = UserActionPostActionType.WebView,
                    Base64Image = BASE64_ICON_PNG,
                    Scope = UserActionPostScope.Global,
                    Path = new string[] { "HOME", "FauxFace", "View Results" },
                    Url = "http://localhost:8780/plugin1/api/fauxface"
                });

        }
    }
}
