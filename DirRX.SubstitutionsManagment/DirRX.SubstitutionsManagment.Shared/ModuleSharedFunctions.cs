using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;

namespace DirRX.SubstitutionsManagment.Shared
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Сформировать тему уведомления о создании/изменении замещения.
    /// </summary>
    /// <returns>Тема.</returns>
    [Public]
    public virtual string FillSubstitutionNotificationSubject(ISubstitution substitution)
    {
      var name = Employees.Is(substitution.User) ?
        Sungero.Parties.PublicFunctions.Person.GetFullName(Employees.As(substitution.User).Person, DeclensionCase.Accusative) :
        substitution.User.ToString();
      
      var subject = DirRX.SubstitutionsManagment.Resources.SubstitutionNotificationSubjectTextFormat(name);
      if (substitution.StartDate.HasValue)
        subject += DirRX.SubstitutionsManagment.Resources.SubstitutionNotificationStartDateTextFormat(substitution.StartDate.Value.ToShortDateString());
      if (substitution.EndDate.HasValue)
        subject += DirRX.SubstitutionsManagment.Resources.SubstitutionNotificationEndDateTextFormat(substitution.EndDate.Value.ToShortDateString());
      
      return subject;
    }

  }
}