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
    /// Отправки уведомление замещающему сотруднику о создании/изменения замещения.
    /// </summary>
    [Public, Remote]
    public virtual void SendSubstitutionNotification(ISubstitution substitution)
    {
      try
      {
        var subject = PublicFunctions.Module.FillSubstitutionNotificationSubject(substitution);
        var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, substitution.Substitute);
        task.Attachments.Add(substitution);
        task.ActiveText = substitution.Comment;
        task.Save();
        task.Start();
        Logger.DebugFormat("Async Handler - SendSubstitutionNotification. Уведомление пользователю успешно отправлено. Id уведомления: {0}.", task.Id);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("Async Handler - SendSubstitutionNotification. Ошибка при отправке уведомления. Id адресата: {1}. Message: {2}. StackTrace: {3}.", substitution.Substitute.Id, ex.Message, ex.StackTrace);
      }
    }
    
    /// <summary>
    /// Создать/изменить замещение.
    /// </summary>
    /// <param name="substitutionStruct">Структура данных, содержащая информацию о замещении.</param>
    /// <param name="isUpdate">Признак необходимости обновления существующего замещения.</param>
    [Public, Remote(IsPure = true)]
    public virtual void CreateOrUpdateSubstitution(Structures.Module.ISubstitutionDialogStructure substitutionStruct, bool isUpdate)
    {
      var substitution = substitutionStruct.Substitution;
      if (isUpdate && substitution != null && Users.Equals(substitutionStruct.SubstitutedUser, substitution.User) && Users.Equals(substitutionStruct.Substitute, substitution.Substitute) &&
          substitutionStruct.StartDate == substitution.StartDate && substitutionStruct.EndDate == substitution.EndDate && substitutionStruct.Comment == substitution.Comment)
        return;

      var asyncHandler = AsyncHandlers.SubstitutionAsyncHandler.Create();
      if (isUpdate)
        asyncHandler.SubstitutionId = substitution.Id;
      
      asyncHandler.SubstitutedUserId = substitutionStruct.SubstitutedUser.Id;
      asyncHandler.SubstituteId = substitutionStruct.Substitute.Id;
      asyncHandler.StartDate = substitutionStruct.StartDate.GetValueOrDefault();
      asyncHandler.EndDate = substitutionStruct.EndDate.GetValueOrDefault();
      asyncHandler.Comment = substitutionStruct.Comment;
      asyncHandler.isUpdate = isUpdate;
      asyncHandler.ExecuteAsync();
    }
    
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