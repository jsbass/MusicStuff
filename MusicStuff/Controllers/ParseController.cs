using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using MidiParser.Helpers;
using MusicStuff.Models.Midi;
using Newtonsoft.Json;

namespace MidiParser.Controllers
{
    public class ParseController : ApiController
    {
        [HttpPost]
        [Route("parse/midi")]
        public async Task<IHttpActionResult> PostMidi()
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (file != null && file.ContentLength > 0)
            {
                using (var stream = file.InputStream)
                using (var reader = new BigEndianBinaryReader(stream))
                {
                    try
                    {
                        var midi = new MidiObject(reader);

                        return Ok(midi);
                    }
                    catch (MidiObject.MidiParseException e)
                    {
                        return Content(HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(new
                        {
                            Partial = e.Partial,
                            Message = e.Message,
                            Type = e.GetType(),
                            StackTrace = e.StackTrace,
                            Position = $"{stream.Position} of {stream.Length}"
                        }));
                    }
                }
            }

            return Content(HttpStatusCode.BadRequest, "File is either missing or empty");
        }

        [HttpPost]
        [Route("parse/musicXml")]
        public async Task<IHttpActionResult> PostXml()
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            if (file != null && file.ContentLength > 0)
            {

            }
            return Ok();
        }
    }
}
