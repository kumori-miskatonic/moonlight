using Moonlight.App.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace Moonlight.App.Services;

public sealed class ReminderNotificationService : IReminderNotificationService
{
    public async Task<bool> EnsurePermissionAsync()
    {
        if (await LocalNotificationCenter.Current.AreNotificationsEnabled())
        {
            return true;
        }

        return await LocalNotificationCenter.Current.RequestNotificationPermission();
    }

    public Task<bool> AreNotificationsEnabledAsync() =>
        LocalNotificationCenter.Current.AreNotificationsEnabled();

    public Task ScheduleAsync(Reminder reminder)
    {
        var due = DateTime.SpecifyKind(reminder.Due, DateTimeKind.Local);
        var request = new NotificationRequest
        {
            NotificationId = reminder.Id,
            Title = "Moonlight reminder",
            Description = reminder.Text,
            Schedule =
            {
                NotifyTime = new DateTimeOffset(due),
                RepeatType = NotificationRepeat.No
            }
        };

        return LocalNotificationCenter.Current.Show(request);
    }

    public Task CancelAsync(int reminderId)
    {
        LocalNotificationCenter.Current.Cancel(reminderId);
        return Task.CompletedTask;
    }
}
