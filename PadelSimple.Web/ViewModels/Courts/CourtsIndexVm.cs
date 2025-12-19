using System;
using System.Collections.Generic;

namespace PadelSimple.Web.ViewModels.Courts;

public class CourtsIndexVm
{
    public DateTime Date { get; set; } = DateTime.Today;
    public string? Start { get; set; }
    public string? End { get; set; }

    public List<CourtRowVm> Courts { get; set; } = new();
}

