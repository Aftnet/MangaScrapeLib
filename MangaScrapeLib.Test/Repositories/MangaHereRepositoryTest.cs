﻿using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MangaScrapeLib.Test.Repositories
{
    /// <summary>
    ///This is a test class for EatMangaRepositoryTest and is intended
    ///to contain all EatMangaRepositoryTest Unit Tests.
    ///</summary>
    [TestClass]
    public class MangaHereRepositoryTest : MangaRepositoryTestBase
    {
        internal override Repository GetRepository()
        {
            return MangaHereRepository.Instance;
        }
    }
}