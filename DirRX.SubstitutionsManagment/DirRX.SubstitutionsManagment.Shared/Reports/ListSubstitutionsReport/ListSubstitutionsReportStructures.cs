using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.SubstitutionsManagment.Structures.ListSubstitutionsReport
{
  /// <summary>
  /// Структура для отчёта.
  /// </summary>
  partial class TableLine
  {
    public long Id { get; set; }
    
    public string StartDate { get; set; }
    
    public string EndDate { get; set; }
    
    public string Substitute { get; set; }
    
    public string Substitutable { get; set; }
    
    public string Description { get; set; }
    
    public string IsSystemSubstitution { get; set; }
    
    public string IsNeedDelegateStrictAccess { get; set; }
    
    public string ReportSessionId { get; set; }
  }
}