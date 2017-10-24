# MangaScrapeLib

[![Build status](https://ci.appveyor.com/api/projects/status/sv7vl92s8128k6ak?svg=true)](https://ci.appveyor.com/project/Aftnet/mangascrapelib)
[![NuGet version](https://badge.fury.io/nu/MangaScrapeLib.svg)](https://badge.fury.io/nu/MangaScrapeLib)

This is a library to parse public manga sites.

## Usage

Repositories (manga sharing websites like MangaFox) have multiple series, series have chapters and chapters have pages.
Pages can be queried for the their image, which is returned as a byte array.

Each call to Get[Series|Chapters|Pages|Image]Async results in the corresponding page from the repository being downloaded and the HTML parsed - expect those calls to take in the order of seconds.

```
var repository = EatMangaRepository.Instance
var series = await repository.GetSeriesAsync();

var selectedSeries = series[0];
var chapters = await selectedSeries.GetChaptersAsync();

var selectedChapter = chapters[0];
var pages = await selectedChapter.GetPagesAsync();

var selectedPage = pages[0];
var imageBytes = await selectedPage.GetImageAsync();

var suggestedPath = selectedPage.SuggestPath("C:\\WhereToSaveImage");
```

Repositories can also be searched like so:

```
var repository = EatMangaRepository.Instance
var series = await repository.SearchSeriesAsync("Series name");
```