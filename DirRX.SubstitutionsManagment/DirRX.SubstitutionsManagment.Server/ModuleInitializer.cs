using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace DirRX.SubstitutionsManagment.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateRoles();
      CreateReportsTables();
      GrantRights();
    }
    
    public static void GrantRights()
    {
      var allUsers = Roles.AllUsers;
      
      if (allUsers != null)
      {
        Reports.AccessRights.Grant(Reports.GetListSubstitutionsReport().Info, allUsers, DefaultReportAccessRightsTypes.Execute);
      }
    }
    
    public static void CreateReportsTables()
    {
      var listSubstitutionsReportTableName = SubstitutionsManagment.Constants.ListSubstitutionsReport.SourceTableName;
      Sungero.Docflow.PublicFunctions.Module.DropReportTempTables(new[] { listSubstitutionsReportTableName });

      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.ListSubstitutionsReport.CreateListSubstitutions, new[] { listSubstitutionsReportTableName });
    }
    
    public static void CreateRoles()
    {
      // Cотрудники с правом создавать замещения на себя.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.SubstitutionsManagment.Resources.SubstitutionManagerRoleName,
                                                                      DirRX.SubstitutionsManagment.Resources.SubstitutionManagerRoleDescription,
                                                                      Constants.Module.RoleGuid.SubstitutionManager);
      // Ответственные за настройку замещений для подразделения.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.SubstitutionsManagment.Resources.DepartmentSubstitutionManagerRoleName,
                                                                      DirRX.SubstitutionsManagment.Resources.DepartmentSubstitutionManagerRoleDescription,
                                                                      Constants.Module.RoleGuid.DepartmentSubstitutionManager);
    }
    
    /// <summary>
    /// Видимость модуля в проводнике.
    /// </summary>
    /// <returns>Признак видимости модуля.</returns>
    public override bool IsModuleVisible()
    {
      return true;
    }
  }
}
