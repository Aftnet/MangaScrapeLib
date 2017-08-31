namespace MangaScrapeLib
{
    public class SeriesMetadataSupport
    {
        public readonly bool Cover = false;
        public readonly bool Author = false;
        public readonly bool Release = false;
        public readonly bool Tags = false;
        public readonly bool Description = false;

        internal SeriesMetadataSupport() : this(false)
        {

        }

        internal SeriesMetadataSupport(bool value) : this(value, value, value, value, value)
        {

        }

        internal SeriesMetadataSupport(bool cover, bool author, bool release, bool tags, bool description)
        {
            Cover = cover;
            Author = author;
            Release = release;
            Tags = tags;
            Description = description;
        }
    }
}
