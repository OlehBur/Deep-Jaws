using System;


public enum FoodType {
    Fish,
    Caviar,
    Groundbait,
    Swimbait
}

public class ProgressEventArgs : EventArgs {
    public FoodType FoodType {get; private set;}

    public ProgressEventArgs() {
        FoodType = FoodType.Fish;
    }
    public ProgressEventArgs(FoodType foodType) {
        FoodType = foodType;
    }
}