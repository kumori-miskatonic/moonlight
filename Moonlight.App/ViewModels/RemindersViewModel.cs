using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Moonlight.App.Data;
using Moonlight.App.Models;
using Moonlight.App.Services;
using SQLite;

namespace Moonlight.App.ViewModels;

public sealed partial class RemindersViewModel : ObservableObject
{
    private readonly SQLiteDatabase _database;
    private readonly IReminderNotificationService _notificationService;
    private readonly ILogger<RemindersViewModel> _logger;

    public RemindersViewModel(
        SQLiteDatabase database,
        IReminderNotificationService notificationService,
        ILogger<RemindersViewModel> logger)
    {
        _database = database;
        _notificationService = notificationService;
        _logger = logger;
        var suggestedDueTime = DateTime.Now.AddHours(1);
        DueDate = suggestedDueTime.Date;
        DueTime = suggestedDueTime.TimeOfDay;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddReminderCommand))]
    private string _draftText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Reminder> _reminders = new();

    [ObservableProperty]
    private DateTime _dueDate;

    [ObservableProperty]
    private TimeSpan _dueTime;

    [ObservableProperty]
    private bool _isImportant;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddReminderCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoadRemindersAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = string.Empty;
            Reminders = new ObservableCollection<Reminder>(await _database.GetRemindersAsync());
            await RefreshScheduledNotificationsAsync();
        }
        catch (SQLiteException)
        {
            StatusMessage = "Your reminders could not be loaded. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAddReminder))]
    private async Task AddReminderAsync()
    {
        if (IsBusy || !CanAddReminder())
        {
            return;
        }

        try
        {
            IsBusy = true;
            var reminder = new Reminder
            {
                Text = DraftText.Trim(),
                Due = DueDate.Date + DueTime,
                IsImportant = IsImportant
            };

            await _database.SaveReminderAsync(reminder);
            Reminders.Add(reminder);
            DraftText = string.Empty;
            IsImportant = false;
            StatusMessage = await ScheduleNewReminderAsync(reminder);
        }
        catch (SQLiteException)
        {
            StatusMessage = "Your reminder could not be saved. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveReminderAsync(Reminder reminder)
    {
        try
        {
            await _database.SaveReminderAsync(reminder);
        }
        catch (SQLiteException exception)
        {
            _logger.LogError(exception, "Could not save reminder {ReminderId}.", reminder.Id);
            StatusMessage = "That reminder change could not be saved. Please try again.";
            await LoadRemindersCommand.ExecuteAsync(null);
            return;
        }

        try
        {
            if (reminder.IsCompleted)
            {
                await _notificationService.CancelAsync(reminder.Id);
                StatusMessage = "Reminder completed.";
            }
            else if (reminder.Due <= DateTime.Now)
            {
                StatusMessage = "Reminder updated. Its time has passed, so no alert was scheduled.";
            }
            else if (!await _notificationService.AreNotificationsEnabledAsync())
            {
                StatusMessage = "Reminder updated, but notifications are off. Enable them in your device settings to receive alerts.";
            }
            else
            {
                await _notificationService.ScheduleAsync(reminder);
                StatusMessage = "Reminder marked as not completed and its alert scheduled.";
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not update notification for reminder {ReminderId}.", reminder.Id);
            StatusMessage = reminder.IsCompleted
                ? "Reminder completed, but its alert could not be canceled."
                : "Reminder updated, but its alert could not be scheduled.";
        }
    }

    [RelayCommand]
    private async Task DeleteReminderAsync(Reminder reminder)
    {
        try
        {
            await _database.DeleteReminderAsync(reminder);
            Reminders.Remove(reminder);
            try
            {
                await _notificationService.CancelAsync(reminder.Id);
                StatusMessage = "Reminder deleted.";
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not cancel notification for deleted reminder {ReminderId}.", reminder.Id);
                StatusMessage = "Reminder deleted, but its alert could not be canceled.";
            }
        }
        catch (SQLiteException)
        {
            StatusMessage = "Your reminder could not be deleted. Please try again.";
        }
    }

    private bool CanAddReminder() => !IsBusy && !string.IsNullOrWhiteSpace(DraftText);

    private async Task<string> ScheduleNewReminderAsync(Reminder reminder)
    {
        if (reminder.Due <= DateTime.Now)
        {
            return "Reminder saved. Its date and time have passed, so no alert was scheduled.";
        }

        try
        {
            if (!await _notificationService.EnsurePermissionAsync())
            {
                return "Reminder saved, but notifications are off. Enable them in your device settings to receive alerts.";
            }

            await _notificationService.ScheduleAsync(reminder);
            return "Reminder saved and alert scheduled.";
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not schedule notification for reminder {ReminderId}.", reminder.Id);
            return "Reminder saved, but its alert could not be scheduled. Please check notification permissions and try again.";
        }
    }

    private async Task RefreshScheduledNotificationsAsync()
    {
        bool notificationsEnabled;
        try
        {
            notificationsEnabled = await _notificationService.AreNotificationsEnabledAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not check notification permissions.");
            StatusMessage = "Your reminders loaded, but notification permission could not be checked.";
            return;
        }

        if (!notificationsEnabled)
        {
            if (Reminders.Any(reminder => !reminder.IsCompleted && reminder.Due > DateTime.Now))
            {
                StatusMessage = "Notifications are off. Enable them in your device settings to receive reminder alerts.";
            }

            return;
        }

        var failedSchedules = 0;
        foreach (var reminder in Reminders)
        {
            try
            {
                if (reminder.IsCompleted || reminder.Due <= DateTime.Now)
                {
                    await _notificationService.CancelAsync(reminder.Id);
                }
                else
                {
                    await _notificationService.ScheduleAsync(reminder);
                }
            }
            catch (Exception exception)
            {
                failedSchedules++;
                _logger.LogError(exception, "Could not refresh notification for reminder {ReminderId}.", reminder.Id);
            }
        }

        if (failedSchedules > 0)
        {
            StatusMessage = $"{failedSchedules} reminder alert(s) could not be scheduled. Check notification permissions.";
        }
    }
}
