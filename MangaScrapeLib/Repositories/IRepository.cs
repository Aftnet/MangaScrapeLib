using MangaScrapeLib.Models;
using System;
using System.Threading.Tasks;

namespace MangaScrapeLib.Repositories
{
    public interface IRepository
    {
        Uri MangaIndexPage { get; }
        string Name { get; }
        Uri RootUri { get; }

        Task<Series[]> GetSeriesAsync();
        Task<Series[]> SearchSeriesAsync(string query);
    }
}