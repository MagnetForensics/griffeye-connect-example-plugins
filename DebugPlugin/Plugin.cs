using Griffeye;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConnectDebugPlugin
{
    public class Plugin
    {
        public async Task Initialize()
        {
            var httpClient = new System.Net.Http.HttpClient();
            var client = new Client(httpClient);
            var t = await client.Oauth2TokenPostAsync(string.Empty, Grant_type.Password, "root", "password", "debug_plugin", string.Empty, string.Empty);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer { t.Access_token}");

            const string ActionNamePrefix = "DebugPlugin ";
            var userActionClient = new UserActionClient(httpClient);
            foreach (var userAction in await userActionClient.GetAllUserActionsAsync())
            {
                if (userAction.ActionName.StartsWith(ActionNamePrefix))
                {
                    await userActionClient.DeleteAsync(userAction.Id);
                }
            }

            await userActionClient.AddUserActionAsync(
                new UserActionPost()
                {
                    ActionName = $"{ActionNamePrefix}Dashboard Home Tab",
                    ActionType = UserActionPostActionType.WebView,
                    Scope = UserActionPostScope.Global,
                    Path = new List<string> { "HOME", "Dashboard Home Tab", "Action" },
                    Url = "http://localhost:8780/plugin2/debug/dashboardhome"
                });

            await userActionClient.AddUserActionAsync(
                new UserActionPost()
                {
                    ActionName = $"{ActionNamePrefix}Dashboard New Tab",
                    ActionType = UserActionPostActionType.WebView,
                    Scope = UserActionPostScope.Global,
                    Path = new List<string> { "Dashboard Tab", "Dashboard new Tab", "Action" },
                    Url = "http://localhost:8780/plugin2/debug/dashboardnew"
                });

            await userActionClient.AddUserActionAsync(
                new UserActionPost()
                {
                    ActionName = $"{ActionNamePrefix}Workspace view tab - ribbon group information",
                    ActionType = UserActionPostActionType.WebView,
                    Scope = UserActionPostScope.Workspace,
                    Path = new List<string> { "VIEW", "Information", "Action" },
                    Url = "http://localhost:8780/plugin2/debug/workspaceview"
                });

            await userActionClient.AddUserActionAsync(
                new UserActionPost()
                {
                    ActionName = $"{ActionNamePrefix}Workspace New Tab",
                    ActionType = UserActionPostActionType.WebView,
                    Scope = UserActionPostScope.Workspace,
                    Path = new List<string> { "Workspace Tab", "Workspace new Tab", "Action" },
                    Url = "http://localhost:8780/plugin2/debug/workspacenew"
                });

            await userActionClient.AddUserActionAsync(
                new UserActionPost()
                {
                    ActionName = $"{ActionNamePrefix}File Context Menu",
                    ActionType = UserActionPostActionType.WebView,
                    Scope = UserActionPostScope.File,
                    Path = new List<string> { "Debug Plugin", "File Context Menu", "Action" },
                    Url = "http://localhost:8780/plugin2/debug/filemenu"
                });

            await userActionClient.AddUserActionAsync(
                new UserActionPost()
                {
                    ActionName = $"{ActionNamePrefix}File Context Menu with Sub Menu",
                    ActionType = UserActionPostActionType.WebView,
                    Scope = UserActionPostScope.File,
                    Path = new List<string> { "Debug Plugin", "File Context Menu", "Sub Menu", "Action" },
                    Url = "http://localhost:8780/plugin2/debug/filesubmenu"
                });

            await userActionClient.AddUserActionAsync(
                new UserActionPost()
                {
                    ActionName = $"{ActionNamePrefix}Entity Context Menu",
                    ActionType = UserActionPostActionType.WebView,
                    Scope = UserActionPostScope.Entity,
                    Path = new List<string> {"Debug Plugin", "Entity Context Menu", "Action"},
                    Url = "http://localhost:8780/plugin2/debug/entitymenu"
                });

            await userActionClient.AddUserActionAsync(
                new UserActionPost()
                {
                    ActionName = $"{ActionNamePrefix}Entity File Context Menu",
                    ActionType = UserActionPostActionType.WebView,
                    Scope = UserActionPostScope.EntityFile,
                    Path = new List<string> {"Debug Plugin", "Entity File Context Menu", "Action"},
                    Url = "http://localhost:8780/plugin2/debug/entityfilemenu"
                });
        }
    }
}
