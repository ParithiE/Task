using RL.Data.DataModels.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RL.Data.DataModels
{
    public class ProcedureUserAssignment : IChangeTrackable
    {
        [Key]
        public int ProcedureUserAssignmentId { get; set; }
        public int PlanId { get; set; }
        public Plan Plan { get; set; }

        public int ProcedureId { get; set; }
        public Procedure Procedure { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
