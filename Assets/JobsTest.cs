using System.Diagnostics;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Debug = UnityEngine.Debug;
using System.Threading.Tasks;
using Unity.Burst;

public class JobsTest : MonoBehaviour
{
    [SerializeField]
    private int _iterations = 0;

    private enum Method
    {
        Standard,
        Tasks,
        JobsArray
    }

    [SerializeField]
    private Method _method = Method.Standard;

    private bool _run = false;

    private readonly Stopwatch _stopWatch = new Stopwatch();

    private NativeArray<JobHandle> _nativeHandles;
    private Task[] _tasks;

    private void Update()
    {
        if (!_run)
        {
            _nativeHandles = new NativeArray<JobHandle>(
                          _iterations, Allocator.Temp);
            _tasks = new Task[_iterations];

            switch (_method)
            {
                case Method.Standard:
                    {
                        _stopWatch.Start();
                        for (int i = 0; i < _iterations; ++i)
                        {
                            DumbTest();
                        }
                        _stopWatch.Stop();
                        Debug.Log(_method.ToString() + " " +
                                   _stopWatch.ElapsedMilliseconds + "ms");
                        break;
                    }
                case Method.Tasks:
                    {
                        _stopWatch.Start();
                        for (int i = 0; i < _iterations; ++i)
                        {
                            _tasks[i] = Task.Factory.StartNew(DumbTest);
                        }
                        Task.WaitAll(_tasks);
                        _stopWatch.Stop();
                        Debug.Log(_method.ToString() + " " +
                                    _stopWatch.ElapsedMilliseconds + "ms");
                        break;
                    }
                case Method.JobsArray:
                    {
                        _stopWatch.Start();
                        for (int i = 0; i < _iterations; ++i)
                        {
                            _nativeHandles[i] = CreateJobHandle();
                        }
                        JobHandle.CompleteAll(_nativeHandles);
                        _stopWatch.Stop();
                        Debug.Log(_method.ToString() + " " +
                                   _stopWatch.ElapsedMilliseconds + "ms");
                        break;
                    }
            }
        }
        _run = true;
    }

    public JobHandle CreateJobHandle()
    {
        var job = new TestJob();
        return job.Schedule();
    }

    [BurstCompile]
    public struct TestJob : IJob
    {
        public void Execute()
        {
            DumbTest();
        }
    }

    public static void DumbTest()
    {
        float value = 0f;
        for (int i = 0; i < 600000; ++i)
        {
            value += Mathf.Exp(i) * Mathf.Sqrt(value);
        }
    }
}
