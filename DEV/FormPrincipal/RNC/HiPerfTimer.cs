using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace RNC
{
   
        public class HiPerfTimer
        {
            [DllImport("Kernel32.dll")]
            private static extern bool QueryPerformanceCounter(
                out long lpPerformanceCount);

            [DllImport("Kernel32.dll")]
            private static extern bool QueryPerformanceFrequency(
                out long lpFrequency);

            private long startTime, stopTime;
            private long freq;

            private bool _bStarted;
            public bool m_bStarted
            {
                get { return _bStarted; }

            }
            private bool _bStoped;
            public bool m_bStoped
            {
                get { return _bStoped; }
            }
            // Constructor

            public HiPerfTimer()
            {
                startTime = 0;
                stopTime = 0;
                _bStarted = false;
                _bStoped = true;

                if (QueryPerformanceFrequency(out freq) == false)
                {
                    // high-performance counter not supported

                    throw new Win32Exception();
                }
            }

            // Start the timer

            public void Start()
            {
                // lets do the waiting threads there work

                Thread.Sleep(0);
                QueryPerformanceCounter(out startTime);
                _bStarted = true;
                _bStoped = false;
            }

            // Stop the timer

            public void Stop()
            {
                QueryPerformanceCounter(out stopTime);
                _bStarted = false;
                _bStoped = true;
            }

            // Returns the duration of the timer (in seconds)

            public double Duration
            {
                get
                {
                    return (double)(stopTime - startTime) / (double)freq;
                }
            }
        }
 
}
