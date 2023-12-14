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
      var hasRights = PublicFunctions.Module.Remote.isSubstitutionManager(Employees.Current) ||
        PublicFunctions.Module.Remote.isDepartmentSubstitutionManager(Employees.Current);
      
      if (!hasRights)
        Dialogs.ShowMessage(DirRX.SubstitutionsManagment.Resources.NoRightsErrorMessage, MessageType.Error);
      
      return hasRights;
    }
    
    /// <summary>
    /// Создать замещение.
    /// </summary>
    public virtual void CreateSubstitution()
    {
      if (!CheckUserRights())
        return;
      
      var substitutionStruct = new Structures.Module.SubstitutionDialogStructure();
      var result = ShowSubstitutionInputDialog(false, substitutionStruct);
    }
    
    /// <summary>
    /// Изменить замещение.
    /// </summary>
    public virtual void UpdateSubstitution()
    {
      if (!CheckUserRights())
        return;
      
      var substitutionStruct = new Structures.Module.SubstitutionDialogStructure();
      var result = ShowSubstitutionInputDialog(true, substitutionStruct);
    }
    
    /// <summary>
    /// Показать диалог создания/изменения замещения.
    /// </summary>
    /// <param name="needUpdate">Признак необходимости обновления существующего замещения.</param>
    /// <param name="substitutionInfo">Признак необходимости изменения замещения.</param>
    /// <returns>Результат выполнения диалога.</returns>
    public virtual bool ShowSubstitutionInputDialog(bool needUpdate, Structures.Module.ISubstitutionDialogStructure substitutionStruct)
    {
      var user = Employees.Current;
      var isDepartmentManager = PublicFunctions.Module.Remote.isDepartmentSubstitutionManager(user);
      var dialog = needUpdate ?
        Dialogs.CreateInputDialog(DirRX.SubstitutionsManagment.Resources.UpdateSubstitutionDialogName) :
        Dialogs.CreateInputDialog(DirRX.SubstitutionsManagment.Resources.CreateSubstitutionDialogName);
      
      # region Реквизиты диалога.
      Sungero.Core.INavigationDialogValue<ISubstitution> substitution = null;
      if (needUpdate)
        substitution = dialog.AddSelect(Substitutions.Info.LocalizedName, true, Substitutions.Null)
          .WithLookupMode(LookupMode.Standalone)
          .From(PublicFunctions.Module.Remote.GetEmployeeSubstitutions(user, isDepartmentManager));
      
      var substituted = dialog.AddSelect(DirRX.SubstitutionsManagment.Resources.SubstitutedFieldName, true, Employees.Null)
        .WithLookupMode(LookupMode.Standalone);
      substituted = isDepartmentManager ? substituted.Where(x => Departments.Equals(user.Department, x.Department)) : substituted.From(user);
      
      var substitute = dialog.AddSelect(Substitutions.Info.Properties.Substitute.LocalizedName, true, Employees.Null)
        .WithLookupMode(LookupMode.Standalone);
      
      var startDate = dialog.AddDate(Substitutions.Info.Properties.StartDate.LocalizedName, false);
      var endDate = dialog.AddDate(Substitutions.Info.Properties.EndDate.LocalizedName, false);
      var comment = dialog.AddString(Substitutions.Info.Properties.Comment.LocalizedName, false);
      #endregion
      
      #region Автозаполнение реквизитов диалога по выбранному замещению.
      if (needUpdate)
        substitution.SetOnValueChanged((x) =>
                                       {
                                         if (!Substitutions.Equals(x.NewValue, x.OldValue))
                                         {
                                           substituted.Value = Employees.As(x.NewValue?.User);
                                           substitute.Value = Employees.As(x.NewValue?.Substitute);
                                           startDate.Value = x.NewValue?.StartDate;
                                           endDate.Value = x.NewValue?.EndDate;
                                           comment.Value = x.NewValue?.Comment;
                                         }
                                       });
      #endregion
      
      #region Подтверждение ввода данных.
      if (dialog.Show() == DialogButtons.Ok)
      {
        substitutionStruct.Substitution = substitution?.Value;
        substitutionStruct.User = substituted.Value;
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