using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceDataUpload.Models
{

    public class CustomerGroupSiteModel
    {
        public string SiteName { get; set; }
        public int SiteId { get; set; }
        public string GroupName { get; set; }
        public int GroupId { get; set; }
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }
    }
}
