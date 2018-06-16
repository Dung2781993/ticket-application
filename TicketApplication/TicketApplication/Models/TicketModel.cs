using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TicketApplication.Models
{
    public class TicketModel
    {
        public long Id { get; set; }

        public long RequesterId { get; set; }

        public long AssigneeNumber { get; set; }
        public String TicketDescription { get; set; }
        public String TicketTitle { get; set; }

        public DateTime CreatedDate { get; set; }
        public String TicketStatus { get; set; }
    }
}