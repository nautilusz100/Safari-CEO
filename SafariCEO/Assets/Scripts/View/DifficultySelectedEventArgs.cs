using System;

public class DifficultySelectedEventArgs : EventArgs
{
    public Difficulty SelectedDifficulty { get; }

    public DifficultySelectedEventArgs(Difficulty difficulty)
    {
        SelectedDifficulty = difficulty;
    }
}
