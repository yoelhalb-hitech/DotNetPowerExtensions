using System.Collections;

namespace SequelPay.DotNetPowerExtensions;

public class FollowThroughEnumerable<T> : IEnumerable<T>
{
    public FollowThroughEnumerable(T startingObject, Func<T, T> nextFunc, Func<T, bool> stopFunc)
    {
        StartingObject = startingObject;
        NextFunc = nextFunc;
        StopFunc = stopFunc;
    }

    public FollowThroughEnumerable(T startingObject, Func<T, T?> autoStopNextFunc)
    {
        StartingObject = startingObject;
        AutoStopNextFunc = autoStopNextFunc;
    }

    public T StartingObject { get; }
    public Func<T, T?>? AutoStopNextFunc { get; }
    public Func<T, T>? NextFunc { get; }
    public Func<T, bool>? StopFunc { get; }

    public IEnumerator<T> GetEnumerator()
    {
        var nextObject = StartingObject;
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
