namespace Server.Zip
{
    public interface IRequestLifetimeTracker
    {
        void TrackRequestLifetime(long requestStartTimestamp);
    }
}