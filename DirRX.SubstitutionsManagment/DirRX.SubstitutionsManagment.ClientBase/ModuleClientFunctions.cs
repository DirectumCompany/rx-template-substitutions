using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;

namespace DirRX.SubstitutionsManagment.Client
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Проверить права пользователя на выполнение действий по созданию/изменению замещений.
    /// </summary>
    /// <returns>Результат проверки.</returns>
    public virtual bool CheckUserRights()
    {
      if (Employees.Current == null)
      {
        Dialogs.ShowMessage(DirRX.SubstitutionsManagment.Resources.UserIsSystemErrorMessage, MessageType.Information);
        return false;
      }
      
      var hasRights = PublicFunctions.Module.Remote.IsSubstitutionManager(Employees.Current) ||
        PublicFunctions.Module.Remote.IsDepartmentSubstitutionManager(Employees.Current);
      
      if (!hasRights)
        Dialogs.ShowMessage(DirRX.SubstitutionsManagment.Resources.NoRightsErrorMessage, MessageType.Information);
      
      return hasRights;
    }
    
    /// <summary>
    /// Создать замещение.
    /// </summary>
    public virtual void CreateSubstitution()
    {
      if (!CheckUserRights())
        return;
      
      var isUpdate = false;
      var substitutionStruct = new Structures.Module.SubstitutionDialogStructure();
      var result = ShowSubstitutionInputDialog(substitutionStruct, isUpdate);
      if (result)
        if (Functions.Module.Remote.CheckDoubleSubstitutions(substitutionStruct))
          Dialogs.ShowMessage(DirRX.SubstitutionsManagment.Resources.IsDouble, MessageType.Error);
        else
          Functions.Module.Remote.CreateOrUpdateSubstitution(substitutionStruct, isUpdate);
    }
    
    /// <summary>
    /// Изменить замещение.
    /// </summary>
    public virtual void UpdateSubstitution()
    {
      if (!CheckUserRights())
        return;
      
      var isUpdate = true;
      var substitutionStruct = new Structures.Module.SubstitutionDialogStructure();
      var result = ShowSubstitutionInputDialog(substitutionStruct, isUpdate);
      if (result)
        if (Functions.Module.Remote.CheckDoubleSubstitutions(substitutionStruct))
          Dialogs.ShowMessage(DirRX.SubstitutionsManagment.Resources.IsDouble, MessageType.Error);
        else
          Functions.Module.Remote.CreateOrUpdateSubstitution(substitutionStruct, isUpdate);
    }
    
    /// <summary>
    /// Показать диалог создания/изменения замещения.
    /// </summary>
    /// <param name="substitutionStruct">Структура данных, содержащая информацию о замещении.</param>
    /// <param name="isUpdate">Признак необходимости обновления существующего замещения.</param>
    /// <returns>Результат выполнения диалога.</returns>
    public virtual bool ShowSubstitutionInputDialog(Structures.Module.ISubstitutionDialogStructure substitutionStruct, bool isUpdate)
    {
      var user = Employees.Current;
      var isDepartmentManager = PublicFunctions.Module.Remote.IsDepartmentSubstitutionManager(user);
      var dialog = isUpdate ?
        Dialogs.CreateInputDialog(DirRX.SubstitutionsManagment.Resources.UpdateSubstitutionDialogName) :
        Dialogs.CreateInputDialog(DirRX.SubstitutionsManagment.Resources.CreateSubstitutionDialogName);
      
      #region Реквизиты диалога.
      Sungero.Core.INavigationDialogValue<ISubstitution> substitution = null;
      if (isUpdate)
        substitution = dialog.AddSelect(Substitutions.Info.LocalizedName, true, Substitutions.Null)
          .WithLookupMode(LookupMode.Standalone)
          .From(PublicFunctions.Module.Remote.GetEmployeeSubstitutions(user, isDepartmentManager));
      
      var substitutedEmployee = isDepartmentManager ?
        dialog.AddSelect(DirRX.SubstitutionsManagment.Resources.SubstitutedFieldName, true, Employees.Null).Where(x => Departments.Equals(user.Department, x.Department)) :
        dialog.AddSelect(DirRX.SubstitutionsManagment.Resources.SubstitutedFieldName, true, user).From(user);
      substitutedEmployee = substitutedEmployee.WithLookupMode(LookupMode.Standalone);
      
      var substitute = dialog.AddSelect(Substitutions.Info.Properties.Substitute.LocalizedName, true, Employees.Null)
        .WithLookupMode(LookupMode.Standalone);
      
      var startDate = dialog.AddDate(Substitutions.Info.Properties.StartDate.LocalizedName, false);
      var endDate = dialog.AddDate(Substitutions.Info.Properties.EndDate.LocalizedName, false);
      var comment = dialog.AddString(Substitutions.Info.Properties.Comment.LocalizedName, false);
      #endregion
      
      #region Автозаполнение реквизитов диалога по выбранному замещению.
      if (isUpdate)
        substitution.SetOnValueChanged((x) =>
                                       {
                                         if (!Substitutions.Equals(x.NewValue, x.OldValue))
                                         {
                                           substitutedEmployee.Value = Employees.As(x.NewValue?.User);
                                           substitute.Value = Employees.As(x.NewValue?.Substitute);
                                           startDate.Value = x.NewValue?.StartDate;
                                           endDate.Value = x.NewValue?.EndDate;
                                           comment.Value = x.NewValue?.Comment;
                                         }
                                       });
      #endregion
      
      #region Валидация диалога.
      dialog.SetOnRefresh(er =>
                          {
                            if (startDate.Value >= endDate.Value)
                              er.AddError(DirRX.SubstitutionsManagment.Resources.SubstitutionDialogDateInputErrorMessage, startDate, endDate);
                            if (substitutedEmployee.Value != null && substitute.Value != null && Employees.Equals(substitutedEmployee.Value, substitute.Value))
                              er.AddError(DirRX.SubstitutionsManagment.Resources.SubstitutionReplaceErrorMessage, substitutedEmployee, substitute);
                          });
      #endregion
      
      #region Подтверждение ввода данных.
      if (dialog.Show() == DialogButtons.Ok)
      {
        substitutionStruct.Substitution = substitution?.Value;
        substitutionStruct.SubstitutedUser = substitutedEmployee.Value;
        substitutionStruct.Substitute = substitute.Value;
        substitutionStruct.StartDate = startDate.Value;
        substitutionStruct.EndDate = endDate.Value;
        substitutionStruct.Comment = comment.Value;
        return true;
      }
      return false;
      #endregion
    }

  }
}