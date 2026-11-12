using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Moonlight.App.Data;
using Moonlight.App.Models;
using SQLite;

namespace Moonlight.App.ViewModels;

public sealed partial class RemindersViewModel : ObservableObject
{
    private readonly SQLiteDatabase _database;

    public RemindersViewModel(SQLiteDatabase database)
    {
        _database = database;
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
            StatusMessage = "Reminder saved.";
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
            StatusMessage = reminder.IsCompleted ? "Reminder completed." : "Reminder marked as not completed.";
        }
        catch (SQLiteException)
        {
            StatusMessage = "That change could not be saved. Please try again.";
            await LoadRemindersCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private async Task DeleteReminderAsync(Reminder reminder)
    {
        try
        {
            await _database.DeleteReminderAsync(reminder);
            Reminders.Remove(reminder);
            StatusMessage = "Reminder deleted.";
        }
        catch (SQLiteException)
        {
            StatusMessage = "Your reminder could not be deleted. Please try again.";
        }
    }

    private bool CanAddReminder() => !IsBusy && !string.IsNullOrWhiteSpace(DraftText);
}
