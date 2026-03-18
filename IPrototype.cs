namespace StreamingPlatform.Interfaces
{
    public interface IPrototype<T>
    {
        T ShallowClone();
        T DeepClone();
    }
}
