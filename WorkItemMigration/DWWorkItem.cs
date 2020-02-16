using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WorkItemMigration
{
    public class DWWorkItem : WorkItem
    {
        public int DWId { get; set; }

        
    }
}
