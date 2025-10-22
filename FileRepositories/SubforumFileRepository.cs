using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class SubforumFileRepository : ISubforumRepository
{
    private readonly string _filePath = "subforums.json";

    public SubforumFileRepository()
    {
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    private async Task<List<Subforum>> GetSubforumsFromFile()
    {
        string subforumsAsJson = await File.ReadAllTextAsync(_filePath);
        List<Subforum> subforums = JsonSerializer.Deserialize<List<Subforum>>(subforumsAsJson)!;
        return subforums;
    }

    private async Task SavePostsToFile(List<Subforum> subforums)
    {
        string subforumsAsJson = JsonSerializer.Serialize(subforums);
        await File.WriteAllTextAsync(_filePath, subforumsAsJson);
    }

    public async Task<Subforum> AddAsync(Subforum subforum)
    {
        List<Subforum> subforums = await GetSubforumsFromFile();

        // Tjek om URL'et er taget
        if (subforums.Exists(s => s.URL == subforum.URL))
        {
            throw new InvalidOperationException("URL name already taken");
        }

        subforum.Id = subforums.Count != 0
            ? subforums.Max(p => p.Id) + 1
            : 1;
        subforums.Add(subforum);

        await SavePostsToFile(subforums);

        return subforum;
    }

    public async Task UpdateAsync(Subforum subforum)
    {
        List<Subforum> subforums = await GetSubforumsFromFile();

        Subforum? currentSubforum = subforums.SingleOrDefault(s => s.Id == subforum.Id);
        if (currentSubforum is null)
        {
            throw new InvalidOperationException($"Subforum with ID '{subforum.Id}' not found");
        }

        // Tjek om URL'et er taget
        if (subforums.Exists(s => s.URL == subforum.URL && s.Id != subforum.Id))
        {
            throw new InvalidOperationException("URL name already taken");
        }

        subforums.Remove(currentSubforum);
        subforums.Add(subforum);

        await SavePostsToFile(subforums);
    }

    public async Task DeleteAsync(int id)
    {
        List<Subforum> subforums = await GetSubforumsFromFile();

        Subforum? subforumToRemove = subforums.SingleOrDefault(p => p.Id == id);
        if (subforumToRemove is null)
        {
            throw new InvalidOperationException($"Subforum with ID '{id}' not found");
        }

        subforums.Remove(subforumToRemove);

        await SavePostsToFile(subforums);
    }

    public async Task<Subforum> GetAsync(int id)
    {
        List<Subforum> subforums = await GetSubforumsFromFile();

        Subforum? subforum = subforums.SingleOrDefault(p => p.Id == id);
        if (subforum is null)
        {
            throw new InvalidOperationException($"Subforum with ID '{id}' not found");
        }

        return subforum;
    }

    public IQueryable<Subforum> GetMany()
    {
        string subforumsAsJson = File.ReadAllTextAsync(_filePath).Result;
        List<Subforum> subforums = JsonSerializer.Deserialize<List<Subforum>>(subforumsAsJson)!;
        return subforums.AsQueryable();
    }

    public async Task<Subforum?> GetByName(string name)
    {
        List<Subforum> subforums = await GetSubforumsFromFile();

        return subforums.SingleOrDefault(p => p.Name == name);
    }

    public async Task<Subforum?> GetByURL(string url)
    {
        List<Subforum> subforums = await GetSubforumsFromFile();
        
        Console.WriteLine("SIZE ahhh: " + subforums[0].Id);

        return subforums.SingleOrDefault(p => p.URL == url);
    }
}