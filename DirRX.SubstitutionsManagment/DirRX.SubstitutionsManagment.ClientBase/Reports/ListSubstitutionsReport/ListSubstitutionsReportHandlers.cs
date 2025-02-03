using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.SubstitutionsManagment
{
  partial class ListSubstitutionsReportClientHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Client.BeforeExecuteEventArgs e)
    {
      var dialog = Dialogs.CreateInputDialog(DirRX.SubstitutionsManagment.Reports.Resources.ListSubstitutionsReport.BeingReplacedReplacement);
      var substituteDialog = dialog.AddSelect(DirRX.SubstitutionsManagment.Reports.Resources.ListSubstitutionsReport.Substitute, true, Sungero.Company.Employees.Null);
      var substitutableDialog = dialog.AddSelect(DirRX.SubstitutionsManagment.Reports.Resources.ListSubstitutionsReport.Substitutable, true, Sungero.Company.Employees.Null);
      
      substituteDialog.SetOnValueChanged(x =>
                                         {
                                           substitutableDialog.IsRequired = !(x.NewValue != null);
                                           
                                           if (x.NewValue != null)
                                             substitutableDialog.Value = null;
                                           
                                         }
                                        );
      
      substitutableDialog.SetOnValueChanged(x =>
                                            {
                                              substituteDialog.IsRequired = !(x.NewValue != null);
                                              
                                              if (x.NewValue != null)
                                                substituteDialog.Value = null;
                                            }
                                           );
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        ListSubstitutionsReport.Substitute = substituteDialog?.Value;
        ListSubstitutionsReport.Substitutable = substitutableDialog?.Value;
      }
    }
  }
}