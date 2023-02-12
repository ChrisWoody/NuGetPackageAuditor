namespace NuGetPackageAuditor.Tests;

public class CatalogRootBuilder
{
    private readonly CatalogRoot _catalogRoot = new();

    public CatalogRootBuilder WithPackage(Package package)
    {
        _catalogRoot.CatalogPages ??= Array.Empty<CatalogPage>();
        if (!_catalogRoot.CatalogPages.Any())
            _catalogRoot.CatalogPages = new[] {new CatalogPage()};
        _catalogRoot.CatalogPages[0].Packages ??= Array.Empty<Package>();

        _catalogRoot.CatalogPages[0].Packages = _catalogRoot.CatalogPages[0].Packages.Concat(new[] {package}).ToArray();

        return this;
    }

    public CatalogRoot Build() => _catalogRoot;
}