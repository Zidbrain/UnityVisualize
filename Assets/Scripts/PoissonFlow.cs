using System;
using MathNet.Numerics.Distributions;

namespace Assets.Scripts
{
    public class PoissonFlow : IAgent
    {
        private float _rate;
        private Action<float> _onEmit;
        private bool _isStopped = true;
        private float _nextTime;
        private System.Random _rand = new System.Random();

        public PoissonFlow(float rate, Action<float> onEmit)
        {
            _rate = rate;
            _onEmit = onEmit;
        }

        public void Start(float startTime)
        {
            if (!_isStopped) return;

            _isStopped = false;
            _nextTime = startTime + (float)Exponential.Sample(_rand, _rate);
        }

        public void Stop()
        {
            _isStopped = true;
        }

        public bool UpdateAgent(float modelTime)
        {
            if (_isStopped) return false;

            while (_nextTime < modelTime)
            {
                if (_isStopped) return false;
                _onEmit(_nextTime);
                _nextTime += (float)Exponential.Sample(_rand, _rate);
            }

            return false;
        }
    }
}