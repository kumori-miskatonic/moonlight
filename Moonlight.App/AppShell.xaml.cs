namespace Moonlight.App;

public partial class AppShell : Shell
{
	public AppShell(Views.RemindersView remindersView)
	{
		InitializeComponent();
		Items.Add(new ShellContent
		{
			Title = "Reminders",
			Route = "Reminders",
			Content = remindersView
		});
	}
}
