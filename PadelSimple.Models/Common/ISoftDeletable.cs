using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PadelSimple.Models.Common
{
    internal interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTimeOffset? DeletedAt { get; set; }
    }
}
