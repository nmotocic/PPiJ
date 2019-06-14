public class Goal
{
    public string Description { get; set; }
    public bool Completed{ get; set; }
    public int CurrentAmount{ get; set; }
    public int RequiredAmount{ get; set; }


    public void Evaluate()
    {
        if (CurrentAmount >= RequiredAmount)
            Completed = true;
    }
}
