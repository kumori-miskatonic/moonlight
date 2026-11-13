using Moonlight.App.Models;

namespace Moonlight.App.Services;

public interface IReminderNotificationService
{
    Task<bool> EnsurePermissionAsync();
    Task<bool> AreNotificationsEnabledAsync();
    Task ScheduleAsync(Reminder reminder);
    Task CancelAsync(int reminderId);
}
