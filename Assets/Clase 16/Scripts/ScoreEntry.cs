public struct ScoreEntry
{
    public string userId;
    public string email;
    public float bestDistance;
    public float bestMaxSpeed;

    public ScoreEntry(string userId, string email, float bestDistance, float bestMaxSpeed)
    {
        this.userId = userId;
        this.email = email;
        this.bestDistance = bestDistance;
        this.bestMaxSpeed = bestMaxSpeed;
    }
}