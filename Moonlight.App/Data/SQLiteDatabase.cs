using SQLite;
using Moonlight.App.Models;

namespace Moonlight.App.Data;

public sealed class SQLiteDatabase
{
    private readonly SQLiteAsyncConnection _database;
    private readonly Task _initialization;

    public SQLiteDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _initialization = _database.CreateTableAsync<Reminder>();
    }

    public async Task<List<Reminder>> GetRemindersAsync()
    {
        await _initialization;
        return await _database.Table<Reminder>()
            .OrderBy(reminder => reminder.IsCompleted)
            .ThenBy(reminder => reminder.Due)
            .ToListAsync();
    }

    public async Task<Reminder?> GetReminderAsync(int id)
    {
        await _initialization;
        return await _database.Table<Reminder>().Where(reminder => reminder.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveReminderAsync(Reminder reminder)
    {
        await _initialization;
        return reminder.Id != 0
            ? await _database.UpdateAsync(reminder)
            : await _database.InsertAsync(reminder);
    }

    public async Task<int> DeleteReminderAsync(Reminder reminder)
    {
        await _initialization;
        return await _database.DeleteAsync(reminder);
    }
}
