using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.SubstitutionsManagment.Structures.Module
{

  /// <summary>
  /// Структура данных, содержащая информацию о замещении.
  /// </summary>
  [Public]
  partial class SubstitutionDialogStructure
  {
    public ISubstitution Substitution {get; set;}
    public IUser SubstitutedUser {get; set;}
    public IUser Substitute {get; set;}
    public DateTime? StartDate {get; set;}
    public DateTime? EndDate {get; set;}
    public string Comment {get; set;}
  }

}