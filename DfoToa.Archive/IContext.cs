namespace DfoToa.Archive;

public interface IContext : P360Client.IContext
{
    string InProductionDate { get; }
}
