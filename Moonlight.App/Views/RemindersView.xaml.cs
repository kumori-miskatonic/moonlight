using Moonlight.App.Models;
using Moonlight.App.ViewModels;

namespace Moonlight.App.Views;

public partial class RemindersView : ContentPage
{
	private readonly RemindersViewModel _viewModel;

	public RemindersView(RemindersViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadRemindersCommand.ExecuteAsync(null);
	}

	private async void OnReminderCheckedChanged(object? sender, CheckedChangedEventArgs e)
	{
		if (sender is CheckBox { BindingContext: Reminder reminder })
		{
			await _viewModel.SaveReminderCommand.ExecuteAsync(reminder);
		}
	}
}