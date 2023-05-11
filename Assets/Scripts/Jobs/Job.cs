using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Jobs
{

    public delegate bool JobAction(float modelTime);

    public class Job
    {
        private JobAction _action;
        public Job(JobAction action)
        {
            _action = action;
        }

        public Job Start()
        {
            MainAgent.Jobs.Add(this);
            return this;
        }

        public virtual bool Run(float modelTime)
        {
            return _action(modelTime);
        }

        public static Job Wait(float currentModelTime, float length, Action<float> after)
        {
            var start = currentModelTime;
            return new Job(time =>
            {
                var finished = time - currentModelTime >= length;
                if (finished) after(time);
                return finished;
            });
        }
    }
}
