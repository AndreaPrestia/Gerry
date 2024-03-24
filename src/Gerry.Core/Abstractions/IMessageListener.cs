namespace Gerry.Core.Abstractions;

public interface IMessageListener<in T>
{
    public void Process(T entity);
}