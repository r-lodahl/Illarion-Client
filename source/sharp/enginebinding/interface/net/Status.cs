namespace Illarion.Client.EngineBinding.Interface.Net
{
    public enum Status 
    {
        Ok,
        Connecting,
        Resolving,
        Requesting,
        Body,
        ConnectionError,
        Unavailable,
        Failed
    }
}