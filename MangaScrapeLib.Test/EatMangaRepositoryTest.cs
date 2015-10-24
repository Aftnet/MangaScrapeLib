﻿using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaScrapeLib.Test
{
    /// <summary>
    ///This is a test class for EatMangaRepositoryTest and is intended
    ///to contain all EatMangaRepositoryTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EatMangaRepositoryTest : MangaRepositoryTest
    {
        internal override MangaRepositoryBase CreateMangaRepository()
        {
            return new EatMangaRepository();
        }
    }
}
