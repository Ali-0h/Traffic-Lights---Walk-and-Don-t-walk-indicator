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
        GreenFinished,
        RedWait
    }

    public class TrafficControllerEventArgs : EventArgs
    {
        public TState Previous { get; }
        public TState Current { get; }
        public CarLight CarOutput { get; }
        public PedSignal PedOutput { get; }

        public TrafficControllerEventArgs(
            TState previous,
            TState current,
            CarLight car,
            PedSignal ped)
        {
            Previous = previous;
            Current = current;
            CarOutput = car;
            PedOutput = ped;
        }
    }

    public class TrafficController
    {
        // ================= STATE =================
        public TState CurrentState { get; private set; } = TState.Green;
        public CarLight CarOutput { get; private set; } = CarLight.Green;
        public PedSignal PedOutput { get; private set; } = PedSignal.DontWalk;

        // ================= DURATIONS (ms) =================
        public int GreenDuration { get; set; } = 30000;        // 30s
        public int YellowDuration { get; set; } = 3000;        // 3s
        public int RedWalkDuration { get; set; } = 15000;      // 15s
        public int RedFlashDuration { get; set; } = 5000;      // flashing
        public int GreenFinishedDuration { get; set; } = 3000; // buffer
        // ==================================================

        private int elapsed = 0;
        private bool pedQueued = false;

        // ================= UI ACCESS =================
        public int ElapsedMilliseconds
        {
            get { return elapsed; }
        }

        public bool IsPedRequestQueued
        {
            get { return pedQueued; }
        }
        // =============================================

        public event EventHandler<TrafficControllerEventArgs> StateChanged;
        public event EventHandler OutputsUpdated;

        // ================= INPUT =================
        public void PressPedButton()
        {
            // Only queue during Green
            if (CurrentState == TState.Green)
                pedQueued = true;
        }

        // ================= CLOCK =================
        public void Tick(int deltaMs)
        {
            elapsed += deltaMs;

            switch (CurrentState)
            {
                case TState.Green:
                    CarOutput = CarLight.Green;
                    PedOutput = PedSignal.DontWalk;

                    if (elapsed >= GreenDuration)
                        TransitionTo(TState.Yellow);
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
                            TransitionTo(TState.GreenFinished);
                        }
                    }
                    break;

                case TState.RedWalk:
                    CarOutput = CarLight.Red;
                    PedOutput = PedSignal.Walk;

                    if (elapsed >= RedWalkDuration)
                        TransitionTo(TState.RedFlash);
                    break;

                case TState.RedFlash:
                    CarOutput = CarLight.Red;
                    PedOutput = PedSignal.Flashing;

                    if (elapsed >= RedFlashDuration)
                        TransitionTo(TState.GreenFinished);
                    break;

                case TState.GreenFinished:
                    CarOutput = CarLight.Red;
                    PedOutput = PedSignal.DontWalk;

                    if (elapsed >= GreenFinishedDuration)
                        TransitionTo(TState.Green);
                    break;
            }

            OutputsUpdated?.Invoke(this, EventArgs.Empty);
        }

        // ================= TRANSITION =================
        private void TransitionTo(TState next)
        {
            TState prev = CurrentState;
            CurrentState = next;
            elapsed = 0;

            switch (next)
            {
                case TState.Green:
                    CarOutput = CarLight.Green;
                    PedOutput = PedSignal.DontWalk;
                    break;

                case TState.Yellow:
                    CarOutput = CarLight.Yellow;
                    PedOutput = PedSignal.DontWalk;
                    break;

                case TState.RedWalk:
                    CarOutput = CarLight.Red;
                    PedOutput = PedSignal.Walk;
                    break;

                case TState.RedFlash:
                    CarOutput = CarLight.Red;
                    PedOutput = PedSignal.Flashing;
                    break;

                case TState.GreenFinished:
                    CarOutput = CarLight.Red;
                    PedOutput = PedSignal.DontWalk;
                    break;
            }

            StateChanged?.Invoke(
                this,
                new TrafficControllerEventArgs(prev, next, CarOutput, PedOutput)
            );

            OutputsUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
