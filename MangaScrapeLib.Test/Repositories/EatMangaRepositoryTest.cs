using MangaScrapeLib.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MangaScrapeLib.Test.Repositories
{
    /// <summary>
    ///This is a test class for EatMangaRepositoryTest and is intended
    ///to contain all EatMangaRepositoryTest Unit Tests
    ///</summary>
    [TestClass]
    public class EatMangaRepositoryTest : MangaRepositoryTestBase
    {
        protected override Repository GetRepository()
        {
            return EatMangaRepository.Instance;
        }
    }
}
