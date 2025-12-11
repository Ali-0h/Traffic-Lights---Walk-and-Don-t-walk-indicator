using System;

namespace TrafficLightMealy
{
    public enum CarLight { Green, Yellow, Red }
    public enum PedSignal { Walk, DontWalk, Flashing }

    public enum TState
    {
        Green,
        Yellow,
        RedWalk,
        RedFlash,
        RedWait
    }

    public class TrafficControllerEventArgs : EventArgs
    {
        public TState Previous { get; private set; }
        public TState Current { get; private set; }
        public CarLight CarOutput { get; private set; }
        public PedSignal PedOutput { get; private set; }

        public TrafficControllerEventArgs(TState previous, TState current, CarLight car, PedSignal ped)
        {
            Previous = previous;
            Current = current;
            CarOutput = car;
            PedOutput = ped;
        }
    }

    public class TrafficController
    {
        public TState CurrentState { get; private set; } = TState.Green;
        public CarLight CarOutput { get; private set; } = CarLight.Green;
        public PedSignal PedOutput { get; private set; } = PedSignal.DontWalk;

        // durations in ms
        public int GreenDuration { get; set; } = 5000;
        public int YellowDuration { get; set; } = 3000;
        public int RedWalkDuration { get; set; } = 8000;
        public int RedFlashDuration { get; set; } = 3000;
        public int RedWaitDuration { get; set; } = 2000;

        private int elapsed = 0;
        private bool pedQueued = false;

        public event EventHandler<TrafficControllerEventArgs> StateChanged;
        public event EventHandler OutputsUpdated;

        public void PressPedButton()
        {
            pedQueued = true;
        }

        /// <summary>
        /// Call regularly from UI timer. deltaMs = timer interval in ms.
        /// </summary>
        public void Tick(int deltaMs)
        {
            elapsed += deltaMs;

            switch (CurrentState)
            {
                case TState.Green:
                    CarOutput = CarLight.Green;
                    PedOutput = PedSignal.DontWalk;
                    if (elapsed >= GreenDuration)
                    {
                        TransitionTo(TState.Yellow);
                    }
                    break;

                case TState.Yellow:
                    CarOutput = CarLight.Yellow;
                    PedOutput = PedSignal.DontWalk;
                    if (elapsed >= YellowDuration)
                    {
                        if (pedQueued)
                        {
                            pedQueued = false;
                            TransitionTo(TState.RedWalk);
                        }
                        else
                        {
                            TransitionTo(TState.RedWait);
                        }
                    }
                    break;

                case TState.RedWalk:
                    CarOutput = CarLight.Red;
                    PedOutput = PedSignal.Walk;
                    if (elapsed >= RedWalkDuration)
                    {
                        TransitionTo(TState.RedFlash);
                    }
                    break;

                case TState.RedFlash:
                    CarOutput = CarLight.Red;
                    PedOutput = PedSignal.Flashing;
                    if (elapsed >= RedFlashDuration)
                    {
                        TransitionTo(TState.RedWait);
                    }
                    break;

                case TState.RedWait:
                    CarOutput = CarLight.Red;
                    PedOutput = PedSignal.DontWalk;
                    if (elapsed >= RedWaitDuration)
                    {
                        TransitionTo(TState.Green);
                    }
                    break;
            }

            var handlerOut = OutputsUpdated;
            if (handlerOut != null) handlerOut(this, EventArgs.Empty);
        }

        private void TransitionTo(TState next)
        {
            TState prev = CurrentState;
            CurrentState = next;
            elapsed = 0;

            // update outputs immediately to reflect new state
            switch (next)
            {
                case TState.Green:
                    CarOutput = CarLight.Green; PedOutput = PedSignal.DontWalk; break;
                case TState.Yellow:
                    CarOutput = CarLight.Yellow; PedOutput = PedSignal.DontWalk; break;
                case TState.RedWalk:
                    CarOutput = CarLight.Red; PedOutput = PedSignal.Walk; break;
                case TState.RedFlash:
                    CarOutput = CarLight.Red; PedOutput = PedSignal.Flashing; break;
                case TState.RedWait:
                    CarOutput = CarLight.Red; PedOutput = PedSignal.DontWalk; break;
            }

            var handler = StateChanged;
            if (handler != null) handler(this, new TrafficControllerEventArgs(prev, next, CarOutput, PedOutput));
            var handlerOut = OutputsUpdated;
            if (handlerOut != null) handlerOut(this, EventArgs.Empty);
        }
    }
}
