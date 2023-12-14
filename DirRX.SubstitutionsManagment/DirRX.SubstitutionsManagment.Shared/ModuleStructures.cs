using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.SubstitutionsManagment.Structures.Module
{

  /// <summary>
  /// Информация о замещении.
  /// </summary>
  [Public]
  partial class SubstitutionInfo
  {
    public ISubstitution Substitution {get; set;}
    public IUser User {get; set;}
    public IUser Substitute {get; set;}
    public DateTime? StartDate {get; set;}
    public DateTime? EndDate {get; set;}
    public string Comment {get; set;}
  }

}