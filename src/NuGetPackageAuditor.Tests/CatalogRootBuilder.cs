﻿namespace NuGetPackageAuditor.Tests;

public class CatalogRootBuilder
{
    private readonly CatalogRoot _catalogRoot = new();

    public CatalogRootBuilder WithPackage(NuGetApi.Package package)
    {
        _catalogRoot.CatalogPages ??= Array.Empty<CatalogPage>();
        if (!_catalogRoot.CatalogPages.Any())
            _catalogRoot.CatalogPages = new[] {new CatalogPage()};
        _catalogRoot.CatalogPages[0].Packages ??= Array.Empty<NuGetApi.Package>();

        _catalogRoot.CatalogPages[0].Packages = _catalogRoot.CatalogPages[0].Packages.Concat(new[] {package}).ToArray();

        return this;
    }

    public CatalogRootBuilder WithCatalogPage(CatalogPage catalogPage)
    {
        _catalogRoot.CatalogPages ??= Array.Empty<CatalogPage>();
        _catalogRoot.CatalogPages = _catalogRoot.CatalogPages.Concat(new[] {catalogPage}).ToArray();

        return this;
    }

    public CatalogRoot Build() => _catalogRoot;
}