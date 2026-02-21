# Moongate v2 Documentation

This folder contains the complete documentation for Moongate v2, built with DocFX.

## Structure

```
docs/
├── docfx.json              # DocFX configuration
├── filterConfig.yml        # API filter configuration
├── index.md                # Documentation homepage
├── toc.yml                 # Main table of contents
├── articles/               # Documentation articles
│   ├── getting-started/    # Introduction and setup
│   │   ├── introduction.md
│   │   ├── quickstart.md
│   │   ├── installation.md
│   │   └── configuration.md
│   ├── architecture/       # System architecture
│   │   ├── overview.md
│   │   ├── network.md
│   │   ├── game-loop.md
│   │   ├── events.md
│   │   ├── sessions.md
│   │   └── solution.md
│   ├── scripting/          # Lua scripting
│   │   ├── overview.md
│   │   ├── modules.md
│   │   └── api.md
│   ├── persistence/        # Data persistence
│   │   ├── overview.md
│   │   ├── format.md
│   │   └── repositories.md
│   └── networking/         # Network protocol
│       ├── packets.md
│       └── protocol.md
└── _site/                  # Generated site (created by build)
```

## Building Documentation

### Prerequisites

- .NET SDK 10.0+
- DocFX tool

### Install DocFX

```bash
dotnet tool update -g docfx
```

### Build Locally

```bash
cd docs
docfx docfx.json
```

### Serve Locally

```bash
cd docs
docfx serve _site --port 8080
```

Open http://localhost:8080 in your browser.

## GitHub Pages

Documentation is automatically built and deployed to GitHub Pages via GitHub Actions.

### Workflow

The `.github/workflows/docs.yml` workflow:

1. Triggers on push to `main` branch (docs folder)
2. Installs .NET SDK and DocFX
3. Builds documentation
4. Deploys to GitHub Pages

### Enable GitHub Pages

1. Go to repository **Settings** → **Pages**
2. Under **Source**, select **GitHub Actions**
3. The workflow will deploy to: `https://moongate-community.github.io/moongatev2/`

## Documentation Guidelines

### Writing Style

- Use **present tense** for features ("Moongate uses...")
- Use **imperative mood** for instructions ("Run this command...")
- Keep sentences **clear and concise**
- Use **active voice** when possible

### Code Examples

Use fenced code blocks with language specification:

````markdown
```csharp
public class Example
{
    public void Method() { }
}
```
````

````markdown
```lua
-- Lua example
local value = 42
```
````

````markdown
```bash
# Shell command
dotnet run --project src/Moongate.Server
```
````

### Navigation

- Each article should have **Previous** and **Next** links at the bottom
- Update `toc.yml` files when adding new articles
- Use relative links for internal navigation

### Cross-References

Link to other articles:

```markdown
- **[Architecture Overview](articles/architecture/overview.md)** - High-level system architecture
- **[API Reference](api/toc.yml)** - Full .NET API documentation
```

Link to external resources:

```markdown
- [GitHub Repository](https://github.com/moongate-community/moongatev2)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
```

## Article Template

```markdown
# Article Title

Brief introduction (1-2 paragraphs).

## Section 1

### Subsection

Content here.

```csharp
// Code example
```

## Section 2

More content.

## Next Steps

- Next article: `<relative-path-to-next-article.md>`
- Previous article: `<relative-path-to-previous-article.md>`

---

**Previous**: `[Previous Article](<relative-path-to-previous-article.md>)` | **Next**: `[Next Article](<relative-path-to-next-article.md>)`
```

## Images

Place images in repository `images/` and reference them from `docs/`:

```markdown
![Moongate Logo](../images/moongate_logo.png)
```

## Updating Documentation

### Adding New Articles

1. Create `.md` file in appropriate folder
2. Add entry to folder's `toc.yml`
3. Add entry to main `toc.yml`
4. Update previous/next links in affected articles

### Updating API Documentation

API docs are auto-generated from XML comments in source code:

```csharp
/// <summary>
/// Brief description.
/// </summary>
/// <param name="param">Parameter description.</param>
/// <returns>Return description.</returns>
public void Method(int param) { }
```

## Troubleshooting

### Build Errors

**Error: Invalid toc.yml**
- Check YAML indentation (use spaces, not tabs)
- Ensure all referenced files exist

**Error: Missing file**
- Verify file paths are relative to `docs/` folder
- Check file extensions (.md, .yml)

### Deployment Issues

**GitHub Pages not updating**
- Check workflow run in Actions tab
- Verify Pages source is set to GitHub Actions
- Check workflow permissions

## Contributing

To contribute documentation:

1. Fork the repository
2. Create a branch (`docs/add-feature-docs`)
3. Make your changes
4. Build locally to verify
5. Submit a pull request

## License

Documentation is licensed under the same license as the project (GPL-3.0).

---

For questions or issues, please open an issue on GitHub or join our Discord community.
