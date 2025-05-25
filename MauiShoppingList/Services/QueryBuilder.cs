namespace Test_MauiApp1.Services;

internal class QueryBuilder
{
    List<KeyValuePair<string, string>> _querry = new();
    public QueryBuilder()
    {

    }

    public void Add(string a, string b)
    {
       _querry.Add(new KeyValuePair<string, string>(a, b));

    }

    public async Task<string> GetQuerryUrlAsync()
    {
        return "?"+ await new FormUrlEncodedContent(_querry).ReadAsStringAsync();

    }
}