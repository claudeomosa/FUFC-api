using FUFC.Shared.Data;

namespace FUFC.Shared.Common;

public interface IDatabaseService<T> where T: class
{
    IQueryable<T> GetAll(UfcContext context);
    void Add(UfcContext context, T entity);
}