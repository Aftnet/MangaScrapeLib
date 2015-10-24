using MangaScrapeLibPortable.Repositories;
using System;

namespace MangaScrapeLibPortable.Models
{
    public class SeriesInfo
    {
        public MangaRepository ParentRepository { get; set; }

        public string Name { get; set; }
        public Uri SeriesPageUri { get; set; }

        public string Tags { get; set; }
        public string Description { get; set; }
    }
}
