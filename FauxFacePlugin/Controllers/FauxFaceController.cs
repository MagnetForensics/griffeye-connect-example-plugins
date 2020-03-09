using FauxFace.Bindings;
using FauxFace.Db;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Griffeye;

namespace FauxFace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FauxFaceController : ControllerBase
    {
        private readonly FileClient _fileClient;
        private readonly FileBookmarkClient _fileBookmarkClient;
        private readonly FileMetadataClient _fileMetadataClient;
        readonly IFauxFaceDb db;

        public FauxFaceController(FileClient fileClient, FileBookmarkClient fileBookmarkClient, FileMetadataClient fileMetadataClient,  IFauxFaceDb db)
        {
            _fileClient = fileClient;
            _fileBookmarkClient = fileBookmarkClient;
            _fileMetadataClient = fileMetadataClient;
            this.db = db;
        }

        [HttpPost]
        public async Task<string> Post([FromBody] UserActionEvent value)
        {
            Console.WriteLine($"Recieved a UserAction event via POST: {JsonConvert.SerializeObject(value)}");

            if (value.ActionName == "FauxFacePlugin View Results")
            {
                return Get();
            }

            foreach (var entity in value.Entities)
            {
                var primaryImageFile = entity.EntityFiles.First(x => x.Selected);
                var data = await _fileClient.GetAsync(primaryImageFile.Sha1);

                using (var stream = new MemoryStream())
                using (Image<Rgba32> image = Image.Load(data.Stream))
                {
                    var rect = new Rectangle(
                        (int) (primaryImageFile.Annotation.Points[0] * image.Width),
                        (int) (primaryImageFile.Annotation.Points[1] * image.Height),
                        (int) (primaryImageFile.Annotation.Points[2] * image.Width),
                        (int) (primaryImageFile.Annotation.Points[3] * image.Height)
                    );

                    image.Mutate(ctx => ctx.Crop(rect));

                    image.Mutate(ctx => ctx.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(64, 64)
                    }));

                    image.Save(stream, new JpegEncoder() {Quality = 90});
                    if (db.TryAdd(stream.ToArray(), entity, out var entry))
                    {
                        foreach (var entityFile in entity.EntityFiles)
                        {
                            await AddBookmark(entry, entityFile);

                            await AddMetadata(entry, entityFile);
                        }
                    }
                }
            }
            return string.Empty;
        }

        private async Task AddMetadata(Entry entry, EntityFile entityFile)
        {
            await _fileMetadataClient.AddMetadataAsync(entityFile.EvidenceId, new FileMetadataCore
            {
                Key = "FauxFace Identity",
                Value = $"{entry.id}-{entry.Subject}"
            });
            var rnd = new Random();
            var year = rnd.Next(1970, 1995);
            var month = rnd.Next(1, 12);
            var day = rnd.Next(1, 28);
            await _fileMetadataClient.AddMetadataAsync(entityFile.EvidenceId, new FileMetadataCore
            {
                Key = "FauxFace Date of Birth/datetime",
                Value = (new DateTime(year, month, day)).ToString("o", CultureInfo.InvariantCulture)
            });
        }

        private async Task AddBookmark(Entry entry, EntityFile entityFile)
        {
            var newBookmark = new FileBookmarkPost
            {
                FileId = entityFile.EvidenceId,
                Comment = "Identified by FauxFace",
                Bookmark = new Bookmark
                {
                    Name = $"{entry.id}-{entry.Subject}",
                    Path = new List<string>() {"FauxFace", entry.Identified ? "Identified" : "Not identified"},
                    Color = entry.Identified ? "#00FF00" : "#FF0000",
                    Description = "Faux"
                }
            };
            try
            {
                await _fileBookmarkClient.AddBookmarkAsync(newBookmark);
            }
            catch (ApiException ex) when (ex.StatusCode == 409)
            {
                //Bookmark already exist. Delete and add again.
                if (ex.Headers.TryGetValue("Location", out var locations))
                {
                    var location = locations.FirstOrDefault();
                    var bookmarkId = location.Split("/").Last();
                    await _fileBookmarkClient.DeleteAsync(int.Parse(bookmarkId));
                    await _fileBookmarkClient.AddBookmarkAsync(newBookmark);
                }
            }
        }


        [HttpGet]
            public string Get()
            {
                Console.WriteLine("Recieved a UserAction via GET, showing results view.");

                Response.ContentType = "text/html";

                var sb = new StringBuilder();

                sb.Append(
                    "<html>" +
                    "<head><style>" +
                    "body {" +
                    "background-color: white;" +
                    "margin-left: 40px;" +
                    "margin-right: 40px;" +
                    "}" +
                    "table { border: 1px solid #555; width: 100%; border-spacing: 0; border-collapse: collapse; }" +
                    "th { height: 30px; background-color: #eee; }" +
                    "tr { height: 64px; border: 1px solid  #555; }" +
                    "td { min-width: 64px; border: 1px solid  #555; text-align: center; vertical-align: middle; }" +
                    ".green { color: green; }" +
                    ".red { color: red; }" +
                    "</style></head>" +
                    "<h1>FauxFace Results</h1>" +
                    "<table>" +
                    "<tr>" +
                    "<th>Queried Image</th>" +
                    "<th>Id</th>" +
                    "<th>Subject</th>" +
                    "<th>Identified</th>" +
                    "</tr>"
                );

                var results = db.GetResults();

                foreach (var entry in results)
                {
                    var imageB64 = Convert.ToBase64String(entry.imageData);
                    string identified = entry.Identified ? "✓" : "✗";
                    string identifiedClass = entry.Identified ? "green" : "red";

                    sb.Append(
                        "<tr>" +
                        $"<td><img src=\"data:image/jpg;base64,{imageB64}\"></td>" +
                        $"<td><a href=\"griffeye://entity/{entry.id}\">{entry.id}</a></td>" +
                        $"<td>{entry.Subject}</td>" +
                        $"<td  class={identifiedClass}>{identified}</td>" +
                        "</tr>"
                    );
                }

                sb.Append(
                    "</table>" +
                    $"<p>{results.Count()} results</p>" +
                    "</html>"
                );

                return sb.ToString();
            }
        }
}