namespace MangaScrapeLib.Repositories
{
    public class SeriesMetadataSupport
    {
        public readonly bool Cover = false;
        public readonly bool Author = false;
        public readonly bool Release = false;
        public readonly bool Tags = false;
        public readonly bool Description = false;

        public SeriesMetadataSupport() : this(false)
        {

        }

        public SeriesMetadataSupport(bool value) : this(value, value, value, value, value)
        {

        }

        public SeriesMetadataSupport(bool cover, bool author, bool release, bool tags, bool description)
        {
            Cover = cover;
            Author = author;
            Release = release;
            Tags = tags;
            Description = description;
        }
    }
}
