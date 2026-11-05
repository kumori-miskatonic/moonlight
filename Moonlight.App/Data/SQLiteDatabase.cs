using SQLite;
using Moonlight.App.Models;

public sealed class SQLiteDatabase
{
    private readonly SQLiteAsyncConnection _database;

    public SQLiteDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<Reminder>().Wait();
    }

    public Task<List<Reminder>> GetRemindersAsync()
    {
        return _database.Table<Reminder>().ToListAsync();
    }

    public Task<Reminder> GetReminderAsync(int id)
    {
        return _database.Table<Reminder>().Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    public Task<int> SaveReminderAsync(Reminder reminder)
    {
        if (reminder.Id != 0)
        {
            return _database.UpdateAsync(reminder);
        }
        else
        {
            return _database.InsertAsync(reminder);
        }
    }

    public Task<int> DeleteReminderAsync(Reminder reminder)
    {
        return _database.DeleteAsync(reminder);
    }
}

