namespace Moonlight.App;

public partial class App : Application
{
	private readonly Views.RemindersView _remindersView;

	public App(Views.RemindersView remindersView)
	{
		InitializeComponent();
		_remindersView = remindersView;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(_remindersView);
	}
}