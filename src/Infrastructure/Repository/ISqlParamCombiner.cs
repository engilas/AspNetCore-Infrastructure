namespace Infrastructure.Repository
{
    public interface ISqlParamCombiner
    {
        ISqlParamCombiner Add(object item);
        ISqlParamCombiner AddId(int id);
        ISqlParamCombiner Add(string key, object value);
    }
}