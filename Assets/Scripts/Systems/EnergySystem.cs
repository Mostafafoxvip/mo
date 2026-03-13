using System;
using LuckyQuest.Data;

namespace LuckyQuest.Systems
{
    public sealed class EnergySystem
    {
        private readonly int _maxEnergy;
        private readonly int _minutesPerEnergy;

        public EnergySystem(int maxEnergy, int minutesPerEnergy)
        {
            _maxEnergy = Math.Max(1, maxEnergy);
            _minutesPerEnergy = Math.Max(1, minutesPerEnergy);
        }

        public void Regenerate(PlayerState state, long lastUnixSeconds, long nowUnixSeconds)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (state.Energy >= _maxEnergy)
                return;

            long elapsedSeconds = Math.Max(0, nowUnixSeconds - lastUnixSeconds);
            long secondsPerEnergy = _minutesPerEnergy * 60L;
            int recovered = (int)(elapsedSeconds / secondsPerEnergy);

            if (recovered <= 0)
                return;

            state.Energy = Math.Min(_maxEnergy, state.Energy + recovered);
            state.ClampToValidRanges();
        }
    }
}
