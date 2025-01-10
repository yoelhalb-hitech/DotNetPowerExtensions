using System.Collections;

namespace SequelPay.DotNetPowerExtensions.Collections;

public class FollowThroughEnumerable<T> : IEnumerable<T>
{
    public FollowThroughEnumerable(T startObject, Func<T, T> nextFunc, Func<T, bool> stopFunc)
    {
        StartObject = startObject ?? throw new ArgumentNullException(nameof(startObject));
        NextFunc = nextFunc ?? throw new ArgumentNullException(nameof(nextFunc));
        StopFunc = stopFunc ?? throw new ArgumentNullException(nameof(stopFunc));
    }

    public FollowThroughEnumerable(T startObject, Func<T, T?> autoStopNextFunc)
    {
        StartObject = startObject ?? throw new ArgumentNullException(nameof(startObject));
        AutoStopNextFunc = autoStopNextFunc ?? throw new ArgumentNullException(nameof(autoStopNextFunc));
    }

    public T StartObject { get; }
    public Func<T, T?>? AutoStopNextFunc { get; }
    public Func<T, T>? NextFunc { get; }
    public Func<T, bool>? StopFunc { get; }

    public IEnumerator<T> GetEnumerator()
    {
        var nextObject = StartObject;
        while (nextObject is not null)
        {
            yield return nextObject;

            if (AutoStopNextFunc is not null)
            {
                nextObject = AutoStopNextFunc(nextObject);
                if (nextObject is null) break;
            }
            else
            {
                if (StopFunc is not null && StopFunc(nextObject)) break;
                nextObject = NextFunc!(nextObject);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
