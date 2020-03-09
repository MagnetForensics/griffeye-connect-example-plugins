using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace FauxFace.Bindings
{
    struct AuthenticationRequest
    {
        public string grant_type { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int client_id { get; set; }
        public string refresh_token { get; set; }
        public string client_secret { get; set; }

        public IEnumerable<KeyValuePair<string, string>> ToKeyValuePair()
        {
            return new Dictionary<string, string>
            {
                { "grant_type", grant_type },
                { "username", username },
                { "password", password },
                { "client_id", $"{client_id}" },
                { "refresh_token", refresh_token },
                { "client_secret", client_secret }
            }.Select(x => x);
        }
    }

    class AuthenticationToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
    }

    enum UserActionScope
    {
        File,
        Workspace,
        Global,
        Entity,
        EntityFile
    };

    enum UserActionType
    {
        ApiCall,
        WebView
    }

    struct UserActionPostRequest
    {
        public string actionName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public UserActionType actionType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public UserActionScope scope { get; set; }
        public IEnumerable<string> path { get; set; }
        public string url { get; set; }
        public string base64Image { get; set; }
    }


    public struct File
    {
        public string FileId { get; set; }
        public long VisualId { get; set; }
        public string Sha1 { get; set; }
        public IEnumerable<string> FileInstanceIds { get; set; }
    }

    public struct UserActionEvent
    {
        public IEnumerable<File> Files { get; set; }
        public IEnumerable<Entity> Entities { get; set; }

        public string Id { get; set; }
        public string ActionName { get; set; }
    }

    public enum Shape
    {
        Rectangle
    }

    public struct Annotation
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Shape Shape { get; set; }
        public float[] Points { get; set; }
    }

    public struct EntityFile
    {
        public int EvidenceId { get; set; }
        public int VisualId { get; set; }
        public int EvidenceFileId { get; set; }
        public string Sha1 { get; set; }
        public Annotation Annotation { get; set; }
        public bool Selected { get; set; }
    }

    public struct Entity
    {
        public int EntityId { get; set; }
        public EntityFile[] EntityFiles { get; set; }
    }
}