#:sdk MauiReactor.FileBased.Sdk@0.1.0-preview.2

public class CounterState
{
    public int Count { get; set; }
}

public class App : Component<CounterState>
{
    public override VisualNode Render()
        => ContentPage(
            VStack(
                Image("dotnet_bot.png")
                    .HeightRequest(185)
                    .AutomationId("dotnet-bot"),
                Label("Hello, MauiReactor!").FontSize(28),
                Label("One C# source file. Native .NET MAUI controls."),
                Label($"Count: {State.Count}")
                    .FontSize(24)
                    .AutomationId("count"),
                Button("Increment", () => SetState(s => s.Count++))
                    .AutomationId("increment")
            )
            .Spacing(16)
            .Padding(32)
            .VCenter()
        );
}
