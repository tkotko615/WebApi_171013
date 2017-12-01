using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApi_171013.Controllers
{
    public class MessageController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            string sToken = "5WQvoxc7HKzxSWKCc3O";
            string sCorpID = "wwb2491d1e47ba94f8";
            string sEncodingAESKey = "4CyeXxKsWzkYMxepDmdUHzNYHQoJ6QbAFPVN8OvUG4p";

            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}