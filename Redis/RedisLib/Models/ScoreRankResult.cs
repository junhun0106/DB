namespace RedisLib;

public struct ScoreRankResult<T>
{
    public readonly T Element;
    public readonly double Score;

    public ScoreRankResult(T element, double score)
    {
        this.Element = element;
        this.Score = score;
    }
}
