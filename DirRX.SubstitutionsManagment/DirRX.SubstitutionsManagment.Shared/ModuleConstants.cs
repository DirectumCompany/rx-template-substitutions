using System;
using Sungero.Core;

namespace DirRX.SubstitutionsManagment.Constants
{
  public static class Module
  {
    /// <summary>
    /// Идентификаторы ролей.
    /// </summary>
    [Public]
    public static class RoleGuid
    {
      /// <summary>
      /// Cотрудники с правом создавать замещения на себя.
      /// </summary>
      [Public]
      public static readonly Guid SubstitutionManager = Guid.Parse("1DAC6D4F-9502-44A7-8872-1E8F4449B007");
      /// <summary>
      /// Ответственные за настройку замещений для подразделения.
      /// </summary>
      [Public]
      public static readonly Guid DepartmentSubstitutionManager = Guid.Parse("5D81B15B-CDEC-423F-BC3C-7A1EF4415340");
    }
  }
}