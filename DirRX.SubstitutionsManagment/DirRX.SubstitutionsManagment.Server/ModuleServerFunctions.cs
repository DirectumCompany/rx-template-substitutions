using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.SubstitutionsManagment.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Получить замещения для сотрудника, в которых он или сотрудник его подразделения является замещаемым.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="needDepartment">Признак того, что сотрудник является ответственным за настройку замещений для его подразделения.</param>
    /// <returns>Замещения.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<ISubstitution> GetEmployeeSubstitutions(Sungero.Company.IEmployee employee, bool isDepartmentManager)
    {
      return Substitutions.GetAll()
        .Where(s => Users.Equals(s.User, employee) || (
          isDepartmentManager && Sungero.Company.Employees.Is(s.User) && Sungero.Company.Departments.Equals(Sungero.Company.Employees.As(s.User).Department, employee.Department)));
    }
    
    
    /// <summary>
    /// Проверить вхождение пользователя в роль "Cотрудники с правом создавать замещения на себя".
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <returns>Признак вхождение пользователя в роль.</returns>
    [Public, Remote(IsPure = true)]
    public static bool isSubstitutionManager(IUser user)
    {
      var role = Roles.GetAll(r => r.Sid == Constants.Module.RoleGuid.SubstitutionManager).FirstOrDefault();
      return user.IncludedIn(role);
    }
    
    /// <summary>
    /// Проверить вхождение пользователя в роль "Ответственные за настройку замещений для подразделения".
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <returns>Признак вхождение пользователя в роль.</returns>
    [Public, Remote(IsPure = true)]
    public static bool isDepartmentSubstitutionManager(IUser user)
    {
      var role = Roles.GetAll(r => r.Sid == Constants.Module.RoleGuid.DepartmentSubstitutionManager).FirstOrDefault();
      return user.IncludedIn(role);
    }

  }
}