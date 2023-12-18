using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.SubstitutionsManagment.Server
{
  public class ModuleAsyncHandlers
  {
    /// <summary>
    /// Cоздание/обновление замещения.
    /// </summary>
    /// <param name="args">Аргументы АО.</param>
    public virtual void SubstitutionAsyncHandler(DirRX.SubstitutionsManagment.Server.AsyncHandlerInvokeArgs.SubstitutionAsyncHandlerInvokeArgs args)
    {
      Logger.DebugFormat("AsyncHandler - SubstitutionAsyncHandler. RetryIteration: {0}. Начало выполнения асинхронного обработчика.", args.RetryIteration);
      
      try
      {
        #region Получение данных из БД.
        var substitutedUser = Users.Get(args.SubstitutedUserId);
        var substitute = Users.Get(args.SubstituteId);
        var substitution = args.isUpdate ? Substitutions.Get(args.SubstitutionId) : null;
        var startDate = new Nullable<DateTime>();
        var endDate = new Nullable<DateTime>();
        startDate = args.StartDate != DateTime.MinValue ? args.StartDate : (DateTime?)null;
        endDate = args.EndDate != DateTime.MinValue ? args.EndDate : (DateTime?)null;
        #endregion
        
        #region Проверки.
        if (args.isUpdate && substitution == null)
        {
          Logger.ErrorFormat("AsyncHandler - SubstitutionAsyncHandler. Замещение - не найдено. Id замещения: {0}. Окончание выполнения АО.", args.SubstitutionId);
          return;
        }
        if (substitution != null && Locks.GetLockInfo(substitution).IsLocked)
        {
          Logger.ErrorFormat("AsyncHandler - SubstitutionAsyncHandler. Замещение - заблокированно. Id замещения: {0}.", substitution.Id);
          args.Retry = true;
          return;
        }
        if (substitutedUser == null || substitute == null)
        {
          Logger.ErrorFormat("AsyncHandler - SubstitutionAsyncHandler. Замещаемый или замещающий пользователь - не найден. Id замещаемого: {0}, Id замещающего: {1}. Окончание выполнения АО.",
                             args.SubstitutedUserId, args.SubstituteId);
          return;
        }
        #endregion
        
        #region Создание/обновление замещения.
        if (args.isUpdate)
          Logger.DebugFormat("AsyncHandler - SubstitutionAsyncHandler. Обновление замещения. Id замещения: {0}, Id замещаемого пользователя: {1}, Id замещающего: {2}.",
                             substitution.Id, substitutedUser.Id, substitute.Id);
        else
          Logger.DebugFormat("AsyncHandler - SubstitutionAsyncHandler. Создание замещения. Id замещаемого пользователя: {0}, Id замещающего: {1}.",
                             substitutedUser.Id, substitute.Id);
        Transactions.Execute(() =>
                             {
                               substitution = substitution ?? Substitutions.Create();
                               substitution.User = substitutedUser;
                               substitution.Substitute = substitute;
                               substitution.StartDate = startDate;
                               substitution.EndDate = endDate;
                               substitution.Comment = args.Comment;
                               try
                               {
                                 substitution.Save();
                                 Logger.DebugFormat("AsyncHandler - SubstitutionAsyncHandler. Создано/обновлено замещение. Id замещения: {0}, Id замещаемого пользователя: {1}",
                                                    substitution.Id, substitution.User.Id);
                                 PublicFunctions.Module.Remote.SendSubstitutionNotification(substitution);
                               }
                               catch (Exception ex)
                               {
                                 Logger.ErrorFormat("AsyncHandler - SubstitutionAsyncHandler. Ошибка при создании/обновлении замещения: {0}. StackTrace: {1}.",
                                                    ex.Message, ex.StackTrace);
                               }
                             });
        #endregion
        
      }
      catch(Exception ex)
      {
        Logger.ErrorFormat("AsyncHandler - SubstitutionAsyncHandler. Ошибка при выполнении АО: {0}. StackTrace: {1}.", ex.Message, ex.StackTrace);
      }
      
      #region Перезапуск события.
      if (args.Retry)
      {
        Logger.DebugFormat("AsyncHandler - SubstitutionAsyncHandler. RetryIteration: {0}. Асинхронный обработчик будет запущен повторно.", args.RetryIteration);
      }
      #endregion
      
      Logger.DebugFormat("AsyncHandler - SubstitutionAsyncHandler. RetryIteration: {0}. Окончание выполнения асинхронного обработчика.", args.RetryIteration);
    }
  }
}