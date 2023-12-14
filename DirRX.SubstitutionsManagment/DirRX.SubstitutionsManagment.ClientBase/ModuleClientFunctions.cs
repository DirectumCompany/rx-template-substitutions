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
      if (!PublicFunctions.Module.Remote.isSubstitutionManager(Employees.Current) &&
          !PublicFunctions.Module.Remote.isDepartmentSubstitutionManager(Employees.Current))
      {
        Dialogs.ShowMessage(DirRX.SubstitutionsManagment.Resources.NoRightsErrorMessage, MessageType.Error);
        return false;
      }
      return true;
    }
    
    /// <summary>
    /// Создать замещение.
    /// </summary>
    public virtual void CreateSubstitution()
    {
      if (!CheckUserRights())
        return;
      
      var substitutionInfo = new Structures.Module.SubstitutionInfo();
      var result = ShowSubstitutionInputDialog(false, substitutionInfo);
    }
    
    /// <summary>
    /// Изменить замещение.
    /// </summary>
    public virtual void UpdateSubstitution()
    {
      if (!CheckUserRights())
        return;
      
      var substitutionInfo = new Structures.Module.SubstitutionInfo();
      var result = ShowSubstitutionInputDialog(true, substitutionInfo);
    }
    
    /// <summary>
    /// Показать диалог создания/изменения замещения.
    /// </summary>
    /// <param name="needUpdate">Признак необходимости обновления существующего замещения.</param>
    /// <param name="substitutionInfo">Признак необходимости изменения замещения.</param>
    /// <returns>Результат выполнения диалога.</returns>
    public virtual bool ShowSubstitutionInputDialog(bool needUpdate, Structures.Module.ISubstitutionInfo substitutionInfo)
    {
      var user = Employees.Current;
      var isDepartmentManager = PublicFunctions.Module.Remote.isDepartmentSubstitutionManager(user);
      var dialog = needUpdate ?
        Dialogs.CreateInputDialog(DirRX.SubstitutionsManagment.Resources.UpdateSubstitutionDialogName) :
        Dialogs.CreateInputDialog(DirRX.SubstitutionsManagment.Resources.CreateSubstitutionDialogName);
      
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
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        substitutionInfo.Substitution = substitution?.Value;
        substitutionInfo.User = substituted.Value;
        substitutionInfo.Substitute = substitute.Value;
        substitutionInfo.StartDate = startDate.Value;
        substitutionInfo.EndDate = endDate.Value;
        substitutionInfo.Comment = comment.Value;
        return true;
      }
      
      return false;
    }

  }
}