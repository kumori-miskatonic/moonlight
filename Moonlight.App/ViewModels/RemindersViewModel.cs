using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Moonlight.App.Data;
using Moonlight.App.Models;

namespace Moonlight.App.ViewModels;
public sealed partial class RemindersViewModel : ObservableObject
{
    private readonly SQLiteDatabase _database;

    public RemindersViewModel(SQLiteDatabase database)
    {
        _database = database;
    }

    // the new note will need to be added as a binding in the xaml
    [ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(SaveNoteButton))]
    private string _newReminderText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Models.Reminder> _reminders = new();

    [ObservableProperty]
    private Models.Reminder? _selectedReminder;

    [RelayCommand]
    private async Task LoadRemindersAsync()
    {
        var reminders = await _database.GetRemindersAsync();
        Reminders = new ObservableCollection<Models.Reminder>(reminders);
    }

    private bool CanSave()
}