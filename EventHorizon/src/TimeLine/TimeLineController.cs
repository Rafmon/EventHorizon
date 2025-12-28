using EventHorizon.src.Events;
using EventHorizon.src.TimeLine;
using EventHorizon.src.Util;

public class TimeLineController
{
    private readonly EventManager _eventManager;
    private readonly MemoryController _memoryController;
    private readonly SettingsManager _settingsManager;
    public Queue<TimeLineEvent> TimeLineEvents { get; set; }
    List<TimeLineEvent> UpcomingTimeLineEvents { get; set; }
    public int Duration { get; set; }
    public int currentTime = 0;
    public TimeLinePlayState CurrentPlayState { get; private set; } = TimeLinePlayState.Play;

    public TimeLineController(EventManager eventManager, MemoryController memoryController, SettingsManager settingsManager)
    {
        _eventManager = eventManager;
        _settingsManager = settingsManager;
        _memoryController = memoryController;
        Console.WriteLine("init TimeLineController");

        Duration = _settingsManager.TimelineDuration;
        CurrentPlayState = TimeLinePlayState.Play;

        TimeLineEvents = new Queue<TimeLineEvent>();
        UpcomingTimeLineEvents = new List<TimeLineEvent>();
        GenerateTimeLine();
        Reset();
        Console.WriteLine(" Done init TimeLineController");
        Console.WriteLine("upcoming events amount: " + TimeLineEvents.Count);
        currentTime = Duration / 2;

    }
    public async Task RunTimeLineAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                currentTime += (int)CurrentPlayState;
                Console.WriteLine(currentTime);

                if (currentTime <= Duration)
                {
                    while (TimeLineEvents.Count > 0 && TimeLineEvents.Peek().ExecutionTime <= currentTime)
                    {
                        var timeLineEvent = TimeLineEvents.Dequeue();
                        timeLineEvent.Address.Update(timeLineEvent.IsActive);
                    }
                }
                else
                {
                    Console.WriteLine("Resetting Timeline");
                    Reset();
                }

                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Handle cancellation if necessary
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            Console.WriteLine($"Error in RunTimeLineAsync: {ex.Message}");
        }
    }

    public void GenerateTimeLine()
    {
        var events = _eventManager.Events;
        if (events == null || events.Count == 0)
        {
            Console.WriteLine("no events found skipping event generation");
            return;
        }

        foreach (Event e in events)
        {
            GenerateTimeLineEventsForEvent(UpcomingTimeLineEvents, e);
        }

        UpcomingTimeLineEvents.Sort();
        foreach (TimeLineEvent e in UpcomingTimeLineEvents)
        {
            Console.WriteLine(e.ExecutionTime + " " + e.IsActive + " " + e.ExecutionTime);
        }
    }

    private void GenerateTimeLineEventsForEvent(List<TimeLineEvent> list, Event e)
    {
        list.Add(new TimeLineEvent(true, e.GetEventStartPercent() * (float)Duration, _memoryController.GetMemoryAddressForIndex(e.Address), e.Name));
        list.Add(new TimeLineEvent(false, e.GetEventEndPercent() * (float)Duration, _memoryController.GetMemoryAddressForIndex(e.Address), e.Name));
    }

    private void Reset()
    {
        TimeLineEvents = new Queue<TimeLineEvent>(UpcomingTimeLineEvents);
        UpcomingTimeLineEvents = new List<TimeLineEvent>();
        GenerateTimeLine();
        currentTime = 0;
    }

    public void CompleteReset()
    {
        Duration = _settingsManager.TimelineDuration;
        GenerateTimeLine();//generating the first set of Events so that the old set of precalculated Events also Regenerated.
        Reset();
    }

    public void SetPlayState(TimeLinePlayState playState)
    {
        CurrentPlayState = playState;
    }

    public void SkipToNextEvent()
    {
        if (TimeLineEvents.Count > 0)
        {
            var nextEvent = TimeLineEvents.Peek();
            currentTime = (int)nextEvent.ExecutionTime;
        }
    }

}
