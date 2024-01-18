using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using Sungero.Workflow;

namespace DirRX.SubstitutionsManagment.Shared
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Сформировать тему уведомления о создании/изменении замещения.
    /// </summary>
    /// <param name="substitution">Замещение.</param>
    /// <returns>Тема.</returns>
    [Public]
    public virtual string FillSubstitutionNotificationSubject(ISubstitution substitution)
    {
      var name = Employees.Is(substitution.User) ?
        Sungero.Parties.PublicFunctions.Person.GetFullName(Employees.As(substitution.User).Person, DeclensionCase.Accusative) :
        string.Empty;
      
      var subject = string.Empty;
      subject = DirRX.SubstitutionsManagment.Resources.SubstitutionNotificationSubjectTextFormat(name);
      if (substitution.StartDate.HasValue)
        subject += DirRX.SubstitutionsManagment.Resources.SubstitutionNotificationStartDateTextFormat(substitution.StartDate.Value.ToShortDateString());
      if (substitution.EndDate.HasValue)
        subject += DirRX.SubstitutionsManagment.Resources.SubstitutionNotificationEndDateTextFormat(substitution.EndDate.Value.ToShortDateString());
      
      return subject.Substring(0, Math.Min(subject.Length, Tasks.Info.Properties.Subject.Length));
    }
  }
}