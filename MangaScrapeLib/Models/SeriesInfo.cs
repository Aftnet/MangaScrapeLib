using MangaScrapeLib.Repositories;
using System;

namespace MangaScrapeLib.Models
{
    public class SeriesInfo
    {
        public IMangaRepository ParentRepository { get; set; }

        public string Name { get; set; }
        public Uri SeriesPageUri { get; set; }

        public string Tags { get; set; }
        public string Description { get; set; }
    }
}
