#:sdk MauiReactor.FileBased.Sdk@0.1.0-preview.1

public class CounterState
{
    public int Count { get; set; }
}

public class App : Component<CounterState>
{
    public override VisualNode Render()
        => ContentPage(
            VStack(
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
