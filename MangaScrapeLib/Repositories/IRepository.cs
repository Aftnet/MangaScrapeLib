using System;
using System.Threading.Tasks;
using MangaScrapeLib.Models;

namespace MangaScrapeLib.Repositories
{
    public interface IRepository
    {
        byte[] Icon { get; }
        Uri MangaIndexPage { get; }
        string Name { get; }
        Uri RootUri { get; }

        Task<Series[]> GetSeriesAsync();
        Task<Series[]> SearchSeriesAsync(string query);
    }
}