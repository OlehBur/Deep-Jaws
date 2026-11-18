using System;

public class ProgressEventArgs : EventArgs {
    public ProgressEventArgs() {
        IsCaviarEaten = false;
    }
    public ProgressEventArgs(bool isCaviarEaten) {
        IsCaviarEaten = isCaviarEaten;
    }

    public bool IsCaviarEaten { get; }
    //public bool IsFishBited { get; }
}