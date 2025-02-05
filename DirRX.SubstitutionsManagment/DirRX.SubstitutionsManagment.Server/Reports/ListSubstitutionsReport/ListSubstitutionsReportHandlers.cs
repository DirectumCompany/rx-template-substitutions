using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.SubstitutionsManagment
{
  partial class ListSubstitutionsReportServerHandlers
  {

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.ListSubstitutionsReport.SourceTableName, ListSubstitutionsReport.ReportSessionId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      IQueryable<Sungero.CoreEntities.ISubstitution> substitutions = null;
      
      var dataTable = new List<SubstitutionsManagment.Structures.ListSubstitutionsReport.TableLine>();
      
      if (ListSubstitutionsReport.Substitute != null)
        substitutions = Sungero.CoreEntities.Substitutions.GetAll(s => Equals(s.Substitute, ListSubstitutionsReport.Substitute) &&
                                                                  s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active);
      if (ListSubstitutionsReport.Substitutable != null)
        substitutions = Sungero.CoreEntities.Substitutions.GetAll(s => Equals(s.User, ListSubstitutionsReport.Substitutable) &&
                                                                  s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active);
      
      var reportSessionId = System.Guid.NewGuid().ToString();
      ListSubstitutionsReport.ReportSessionId = reportSessionId;
      
      foreach (var substitution in substitutions)
      {
        var tableLine = SubstitutionsManagment.Structures.ListSubstitutionsReport.TableLine.Create();
        tableLine.Id = substitution.Id;
        tableLine.StartDate = substitution.StartDate != null ? substitution.StartDate.Value.ToString() : string.Empty;
        tableLine.EndDate = substitution.EndDate != null ? substitution.EndDate.Value.ToString() : string.Empty;
        tableLine.Substitute = Sungero.Company.Employees.As(substitution.Substitute).Name;
        tableLine.Substitutable = Sungero.Company.Employees.As(substitution.User).Name;
        tableLine.Description = substitution.Comment;
        
        tableLine.IsSystemSubstitution = substitution.IsSystem == true ?
          DirRX.SubstitutionsManagment.Reports.Resources.ListSubstitutionsReport.Yes :
          DirRX.SubstitutionsManagment.Reports.Resources.ListSubstitutionsReport.No;
        
        tableLine.IsNeedDelegateStrictAccess = substitution.DelegateStrictRights ?
          DirRX.SubstitutionsManagment.Reports.Resources.ListSubstitutionsReport.Yes :
          DirRX.SubstitutionsManagment.Reports.Resources.ListSubstitutionsReport.No;
        tableLine.ReportSessionId = reportSessionId;
        
        dataTable.Add(tableLine);
      }
      
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.ListSubstitutionsReport.SourceTableName, dataTable);
    }
  }
}