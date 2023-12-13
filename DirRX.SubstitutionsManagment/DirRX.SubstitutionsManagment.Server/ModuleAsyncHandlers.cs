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
      var logPrefix = "SubstitutionAsyncHandler";
      Logger.DebugFormat("{0}. SubstitutedUserId {1}, SubstituteId {2}, IsUpdate {3}, SubstitutionId {4}, RetryIteration {5}. Начало выполнения асинхронного обработчика.",
                         logPrefix, args.SubstitutedUserId, args.SubstituteId, args.IsUpdate, args.SubstitutionId, args.RetryIteration);
      try
      {
        #region Получение данных из БД.
        var substitutedUser = Users.GetAll(x => x.Id == args.SubstitutedUserId).FirstOrDefault();
        var substitute = Users.GetAll(x => x.Id == args.SubstituteId).FirstOrDefault();
        var substitution = args.IsUpdate ? Substitutions.GetAll(x => x.Id == args.SubstitutionId).FirstOrDefault() : null;
        var startDate = new Nullable<DateTime>();
        var endDate = new Nullable<DateTime>();
        startDate = args.StartDate != DateTime.MinValue ? args.StartDate : (DateTime?)null;
        endDate = args.EndDate != DateTime.MinValue ? args.EndDate : (DateTime?)null;
        #endregion
        
        #region Проверки.
        if (args.IsUpdate && substitution == null)
        {
          Logger.DebugFormat("{0}. SubstitutionId {1}. Замещение - не найдено. Окончание выполнения АО.", logPrefix, args.SubstitutionId);
          return;
        }
        if (substitutedUser == null || substitute == null)
        {
          Logger.DebugFormat("{0}. SubstitutedUserId {1}, SubstituteId {2}. Замещаемый или замещающий пользователь - не найден. Окончание выполнения АО.",
                             logPrefix, args.SubstitutedUserId, args.SubstituteId);
          return;
        }
        #endregion
        
        #region Создание/обновление замещения.
        var isLocked = false;
        try
        {
          substitution = substitution ?? Substitutions.Create();
          isLocked = Locks.TryLock(substitution);
          if (isLocked)
          {
            substitution.User = substitutedUser;
            substitution.Substitute = substitute;
            substitution.StartDate = startDate;
            substitution.EndDate = endDate;
            substitution.Comment = args.Comment;
            substitution.Save();
            Logger.DebugFormat("{0}. SubstitutionId {1}, SubstitutedUserId {2}, SubstituteId {3}. Создано/обновлено замещение.",
                               logPrefix, substitution.Id, substitutedUser.Id, substitute.Id);
            PublicFunctions.Module.Remote.SendSubstitutionNotification(substitution, logPrefix);
          }
          else
          {
            Logger.DebugFormat("{0}. SubstitutionId {1}. Не удалось установить блокировку на замещение.", logPrefix, substitution.Id);
            args.Retry = true;
          }
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. SubstitutedUserId {1}, SubstituteId {2}, IsUpdate {3}, SubstitutionId {4}, RetryIteration {5}. Ошибка при создании/обновлении замещения: {6}, StackTrace: {7}.",
                             logPrefix, args.SubstitutedUserId, args.SubstituteId, args.IsUpdate, args.SubstitutionId, args.RetryIteration, ex.Message, ex.StackTrace);
        }
        finally
        {
          if (isLocked)
            Locks.Unlock(substitution);
        }
        #endregion
        
      }
      catch(Exception ex)
      {
        Logger.ErrorFormat("{0}. SubstitutedUserId {1}, SubstituteId {2}, IsUpdate {3}, substitutionId {4}, RetryIteration {5}. Ошибка при выполнении АО: {6}, StackTrace: {7}.",
                           logPrefix, args.SubstitutedUserId, args.SubstituteId, args.IsUpdate, args.SubstitutionId, args.RetryIteration, ex.Message, ex.StackTrace);
      }
      
      #region Перезапуск события.
      if (args.Retry)
      {
        Logger.DebugFormat("{0}. SubstitutionAsyncHandler. SubstitutedUserId {1}, SubstituteId {2}, IsUpdate {3}, substitutionId {4}, RetryIteration {5}. Асинхронный обработчик будет запущен повторно.",
                           logPrefix, args.SubstitutedUserId, args.SubstituteId, args.IsUpdate, args.SubstitutionId, args.RetryIteration);
      }
      else
        Logger.DebugFormat("{0}. SubstitutedUserId {1}, SubstituteId {2}, IsUpdate {3}, substitutionId {4}, RetryIteration {5}. Окончание выполнения асинхронного обработчика.",
                           logPrefix, args.SubstitutedUserId, args.SubstituteId, args.IsUpdate, args.SubstitutionId, args.RetryIteration);
      #endregion
    }
  }
}