using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Pyloaded.Reloaded;

public class RxFileWatcher : IDisposable
{
    private readonly FileSystemWatcher _fileWatcher;
    private readonly Subject<Unit> _fileChanged = new();
    private readonly IDisposable _fileChangedSub;
    
    public RxFileWatcher(string file, IScheduler? scheduler = null, TimeSpan? onChangeBuffer = null)
    {
        _fileWatcher = new(Path.GetDirectoryName(file)!, Path.GetFileName(file))
        {
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite,
        };
        
        _fileWatcher.Changed += (_, _) => _fileChanged.OnNext(new());
        _fileChangedSub = _fileChanged
            .Throttle(onChangeBuffer ?? TimeSpan.FromMilliseconds(500))
            .ObserveOn(scheduler ?? Scheduler.Default)
            .Subscribe(_ => Changed?.Invoke(file));
    }

    public Action<string>? Changed { get; set; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        _fileWatcher.Dispose();
        _fileChanged.Dispose();
        _fileChangedSub.Dispose();
    }
}
