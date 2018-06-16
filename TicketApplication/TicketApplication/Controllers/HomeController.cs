using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TicketApplication.Models;

namespace TicketApplication.Controllers
{
    public class HomeController : Controller
    {
        #region Variables
        
        private static IList<TicketModel> ticketList; 

        #endregion
        public ActionResult Index()
        {
            
            var result = RunAsync().GetAwaiter().GetResult();
            if(result == null)
            {
                TempData["WarningMessage"] = "Oh No, Something went wrong!";
                TempData["Error"] = "Error: Couldn't authenticate you";
            }
            ticketList = result;
            return View();
        }

        public static IEnumerable<TicketModel> GetTicketModels()
        {
             return ticketList;
        }


        public static async Task<IList<TicketModel>> RunAsync()
        {
            var ticketArray = await GetTicket().ConfigureAwait(false);
            return ticketArray;
        }

        /// <summary>
        /// Action method to return ticket list to view
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult TicketList_Read([DataSourceRequest] DataSourceRequest request)
        {
            var ticketListResult = GetTicketModels();
            if (ticketListResult == null) return null;
            return Json(ticketListResult.ToDataSourceResult(request));
        }

        /// <summary>
        /// Action method to return ticket info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ActionResult TicketDetailed_Read(int id, [DataSourceRequest] DataSourceRequest request)
        {
            var ticketListResult = GetTicketModels();
            if (ticketListResult == null) return null;
            var ticketListInfo = new List<TicketModel>();
            foreach(TicketModel ticket in ticketListResult)
            {
                if(ticket.Id == id)
                {
                    var ticketInfo = new TicketModel
                    {
                        Id = ticket.Id,
                        TicketDescription = ticket.TicketDescription,
                        AssigneeNumber = ticket.AssigneeNumber,
                        RequesterId = ticket.RequesterId,
                        TicketTitle = ticket.TicketTitle,
                        TicketStatus = ticket.TicketStatus,
                        CreatedDate = ticket.CreatedDate
                    };
                    ticketListInfo.Add(ticketInfo);
                }
            }
            return Json(ticketListInfo.ToDataSourceResult(request)); ;
        }



        /// <summary>
        /// Action method to return ticket info from Get Request
        /// </summary>
        /// <returns></returns>
        public static async Task<IList<TicketModel>> GetTicket()
        {
            var client = new HttpClient();
            var query = "https://helloworld.zendesk.com/api/v2/tickets.json";

            //Passing the authentication values
            var username = ConfigurationManager.AppSettings["username"];
            var password = ConfigurationManager.AppSettings["password"];
            var authentication = Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authentication));

            //Get Json response from Get Request
            var response = await client.GetAsync(query).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;
           
            //Filter Json response 
            var contentString = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(contentString);
            JArray jArray = (JArray)json["tickets"];
            var ticketArray = new List<TicketModel>();
            try
            {
                foreach (JObject item in jArray)
                {
                    var id = item.GetValue("id");
                    var title = item.GetValue("subject");
                    var description = item.GetValue("description");
                    var requesterId = item.GetValue("requester_id");
                    var assigneeId = item.GetValue("assignee_id");
                    var status = item.GetValue("status");
                    var createdDate = item.GetValue("created_at");

                    var ticket = new TicketModel
                    {
                        Id = Convert.ToInt64(id),
                        AssigneeNumber = Convert.ToInt64(assigneeId),
                        RequesterId = Convert.ToInt64(requesterId),
                        TicketDescription = description.ToString(),
                        TicketStatus = status.ToString(),
                        TicketTitle = title.ToString(),
                        CreatedDate = (DateTime)createdDate
                    };

                    ticketArray.Add(ticket);
                }    
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            client.Dispose();

            return ticketArray;
        }
    }
}