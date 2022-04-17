using System;
using System.Collections;
using System.Diagnostics;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;

namespace ExileCore.Shared
{
    public class Coroutine
    {
        private IEnumerator _enumerator;

        private Coroutine(string name, IPlugin owner)
        {
            Name = name ?? MathHepler.GetRandomWord(13);

            OwnerName = owner == null ? "Free" : owner.InternalName;
        }

        public Coroutine(Action action, IYieldBase condition, IPlugin owner, string name = null, bool infinity = true,
            bool autoStart = true) : this(name, owner)
        {
            Running = autoStart;
            Started = DateTime.Now;
            Owner = owner;
            switch (condition)
            {
              case WaitTime time:
                TimeoutForAction = time.Milliseconds.ToString();
                break;
              case WaitRender render:
                TimeoutForAction = render.HowManyRenderCountWait.ToString();
                break;
              case WaitRandom random:
                TimeoutForAction = random.Timeout;
                break;
              case WaitFunction _:
                TimeoutForAction = "Function -1";
                break;
              default:
                TimeoutForAction = TimeoutForAction;
                break;
            }

            Action = action;
            Condition = condition;

            if (infinity)
            {
                IEnumerator CoroutineAction(Action a)
                {
                    yield return YieldBase.RealWork;

                    while (true)
                    {
                        try
                        {
                            a?.Invoke();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Coroutine {Name} in {OwnerName} error -> {e}");
                        }

                        Ticks++;
                        yield return Condition.GetEnumerator();
                    }
                }

                _enumerator = CoroutineAction(action);
            }
            else
            {
                IEnumerator CoroutineAction(Action a)
                {
                    yield return Condition.GetEnumerator();
                    a?.Invoke();
                    Ticks++;
                }

                _enumerator = CoroutineAction(action);
            }
        }

        public Coroutine(Action action, int waitMilliseconds, IPlugin owner, string name = null, bool autoStart = true) : this(
            action, new WaitTime(waitMilliseconds), owner, name, autoStart)
        {
        }

        public Coroutine(IEnumerator enumerator, IPlugin owner, string name = null, bool autoStart = true) : this(name, owner)
        {
            Running = autoStart;
            Started = DateTime.Now;
            TimeoutForAction = "Not simple -1";
            _enumerator = enumerator;
            Owner = owner;
        }

        public bool IsDone { get; private set; }
        public string Name { get; set; }
        public IPlugin Owner { get; private set; }
        public string OwnerName { get; }
        public bool Running { get; private set; }
        public bool AutoResume { get; set; } = true;
        public string TimeoutForAction { get; private set; }
        public long Ticks { get; private set; } = -1;
        public CoroutinePriority Priority { get; set; } = CoroutinePriority.Normal;
        public DateTime Started { get; set; }
        public Action Action { get; private set; }
        public IYieldBase Condition { get; private set; }
        public bool ThisIsSimple => Action != null;
        public bool NextIterRealWork { get; set; }
        public bool SyncModWork { get; set; }
        public event Action OnAutoRestart;
        public event EventHandler WhenDone;

        [Obsolete("Coroutine.UpdateCondtion(...) => Use UpdateCondition()", false)]
        public void UpdateCondtion(IYieldBase condition)
        {
            UpdateCondition(condition);
        }
        public void UpdateCondition(IYieldBase condition)
        {
            switch (condition)
            {
                case WaitTime time:
                    TimeoutForAction = time.Milliseconds.ToString();
                    break;
                case WaitRender render:
                    TimeoutForAction = render.HowManyRenderCountWait.ToString();
                    break;
                case WaitFunction _:
                    TimeoutForAction = "Function";
                    break;
            }

            Condition = condition;
        }

        public IEnumerator GetEnumerator()
        {
            return _enumerator;
        }

        public void UpdateTicks(uint tick)
        {
            Ticks = tick;
        }

        public void Resume()
        {
            Running = true;
        }

        public void AutoRestart()
        {
            OnAutoRestart?.Invoke();
        }

        public void Pause(bool force = false)
        {
            if (Priority == CoroutinePriority.Critical && !force) return;
            Running = false;
        }

        public bool Done(bool force = false)
        {
            if (Priority == CoroutinePriority.Critical) return false;
            Running = false;
            IsDone = true;
            WhenDone?.Invoke(this, EventArgs.Empty);
            return IsDone;
        }

        public void UpdateAction(Action action)
        {
            if (Action != null) Action = action;
        }

        public void UpdateAction(IEnumerator action)
        {
            if (_enumerator != null) _enumerator = action;
        }

        public bool MoveNext()
        {
            return MoveNext(_enumerator);
        }

        private bool MoveNext(IEnumerator enumerator)
        {
            if (IsDone) return false;

            bool moveNext;

            if (enumerator.Current is IEnumerator e && MoveNext(e))
                moveNext = true;
            else
            {
                moveNext = enumerator.MoveNext();
                NextIterRealWork = enumerator.Current == YieldBase.RealWork;
            }

            return moveNext;
        }
    }

    public class WaitRandom : YieldBase
    {
        private static readonly Random rnd = new Random();
        private readonly int _maxWait;
        private readonly int _minWait;

        public WaitRandom(int minWait, int maxWait)
        {
            _minWait = minWait;
            _maxWait = maxWait;
            Current = GetEnumerator();
        }

        public string Timeout => $"{_minWait}-{_maxWait}";

        public sealed override IEnumerator GetEnumerator()
        {
            var wait = sw.ElapsedMilliseconds + rnd.Next(_minWait, _maxWait);

            while (sw.ElapsedMilliseconds < wait)
            {
                yield return null;
            }

            yield return RealWork;
        }
    }

    public class WaitRender : YieldBase
    {
        public WaitRender(long howManyRenderCountWait = 1)
        {
            HowManyRenderCountWait = howManyRenderCountWait;
            Current = GetEnumerator();
        }

        public static int FrameCount { get; private set; }
        public long HowManyRenderCountWait { get; }

        public static void Frame()
        {
            FrameCount++;
        }

        public sealed override IEnumerator GetEnumerator()
        {
            var wait = FrameCount + HowManyRenderCountWait;

            while (FrameCount < wait)
            {
                yield return null;
            }

            yield return RealWork;
        }
    }

    public class WaitFunction : YieldBase
    {
        private readonly Func<bool> fn;

        public WaitFunction(Func<bool> fn)
        {
            this.fn = fn;
            Current = GetEnumerator();
        }

        public sealed override IEnumerator GetEnumerator()
        {
            while (fn())
            {
                yield return null;
            }

            yield return RealWork;
        }
    }
    /// <summary>
    /// Takes a function and lets the Coroutine wait until the given function returns true or the specified time threshold is exceeded.
    /// Using this over WaitFunction is recommended.
    /// </summary>
    public class WaitFunctionTimed : YieldBase
    {
        private readonly Func<bool> fn;
        public int Milliseconds { get; }
        public bool StopCode { get; }
        public string ErrorMessage { get; }

        public WaitFunctionTimed(Func<bool> fn, bool stopCode = false, int maxWait = 1000, string errorMessage = "")
        {
            this.fn = fn;
            Milliseconds = maxWait;
            StopCode = stopCode;
            ErrorMessage = errorMessage;
            Current = GetEnumerator();
        }

        public sealed override IEnumerator GetEnumerator()
        {
            var wait = sw.Elapsed.TotalMilliseconds + Milliseconds;
            while (!fn() && sw.Elapsed.TotalMilliseconds < wait)
            {
                yield return null;
            }
            if (!fn() && StopCode)
            {
                if (ErrorMessage != "")
                {
                    DebugWindow.LogMsg(ErrorMessage);
                }
                Logger.Log.Error($"Code Stopped in {this}");
                yield break;
            }

            yield return RealWork;
        }
    }

    public class WaitTime : YieldBase
    {
        public WaitTime(int milliseconds)
        {
            Milliseconds = milliseconds;
            Current = GetEnumerator();
        }

        public int Milliseconds { get; }

        public sealed override IEnumerator GetEnumerator()
        {
            var wait = sw.Elapsed.TotalMilliseconds + Milliseconds;

            while (sw.Elapsed.TotalMilliseconds < wait)
            {
                yield return null;
            }

            yield return RealWork;
        }
    }

    public abstract class YieldBase : IYieldBase
    {
        protected static readonly Stopwatch sw = Stopwatch.StartNew();
        public static object RealWork { get; } = new object();

        public bool MoveNext()
        {
            if ((Current as IEnumerator).MoveNext()) return true;

            Current = GetEnumerator();
            return false;
        }

        public void Reset()
        {
        }

        public object Current { get; protected set; }
        public abstract IEnumerator GetEnumerator();
    }

    public interface IYieldBase : IEnumerable, IEnumerator
    {
    }
}
