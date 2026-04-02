# Stu — Tailwind Utility CSS Transformer

Stu downloads the Tailwind v4 standalone CLI, discovers all available utility classes from the Tailwind source, and transforms them into a standalone CSS file with plain `px` sizes and `#hex` colors. No Node.js required.

Designed for simpler HTML templating environments that support Tailwind class names but not the full Tailwind build pipeline — for example, email templates, PDF renderers, or embedded HTML editors that need computed CSS values.

## Installation

Add the NuGet package to your project:

```sh
dotnet add package Stu
```

This adds an MSBuild integration that automatically generates the Tailwind utility CSS file as part of your build or publish pipeline.

### MSBuild configuration

Configure Stu in your `.csproj` file:

```xml
<PropertyGroup>
  <!-- Where to write the generated CSS (relative to project root) -->
  <StuOutput>wwwroot/css/tailwind-utilities.css</StuOutput>

  <!-- When to run: Build, Publish, or Both (default: Publish) -->
  <StuRunOn>Publish</StuRunOn>

  <!-- Limit to specific color families (default: all) -->
  <StuColors>gray,red,green,blue</StuColors>

  <!-- Base font size for rem-to-px conversion (default: 16) -->
  <StuBaseFontSize>16</StuBaseFontSize>

  <!-- Minify the output (default: false) -->
  <StuMinify>false</StuMinify>

  <!-- Force re-download of the Tailwind CLI (default: false) -->
  <StuForceDownload>false</StuForceDownload>
</PropertyGroup>
```

With `StuRunOn` set to `Publish` (the default), the CSS file is generated during `dotnet publish`. Set it to `Build` to regenerate on every build, or `Both` for both.

You can also trigger it manually:

```sh
dotnet msbuild -t:StuTransform
```

### MSBuild properties

| Property | Default | Description |
|----------|---------|-------------|
| `StuOutput` | `tailwind-utilities.css` | Output CSS file path |
| `StuColors` | `all` | Comma-separated color families, or `all` |
| `StuBaseFontSize` | `16` | Base font size in px for rem and line-height conversion |
| `StuMinify` | `false` | Minify the output CSS |
| `StuRunOn` | `Publish` | When to run: `Build`, `Publish`, or `Both` |
| `StuForceDownload` | `false` | Force re-download of Tailwind CLI |
| `StuCacheDir` | `~/.cache/tailwind` | Tailwind CLI binary cache directory |

### Available color families

`slate`, `gray`, `zinc`, `neutral`, `stone`, `red`, `orange`, `amber`, `yellow`, `lime`, `green`, `emerald`, `teal`, `cyan`, `sky`, `blue`, `indigo`, `violet`, `purple`, `fuchsia`, `pink`, `rose`

Special colors (`black`, `white`, `transparent`, `current`, `inherit`) are always included regardless of the `StuColors` filter.

## Standalone CLI usage

Stu can also be run directly from the command line:

```sh
dotnet run --project src/Stu -- [options]
```

### CLI options

| Option | Default | Description |
|--------|---------|-------------|
| `-o`, `--output <path>` | `tailwind-utilities.css` | Output CSS file path |
| `-c`, `--colors <families>` | `all` | Comma-separated color families to include |
| `--base-font-size <px>` | `16` | Base font size for rem-to-px and line-height conversion |
| `--minify` | `false` | Output minified CSS |
| `--cache-dir <path>` | `~/.cache/tailwind` | Directory to cache the Tailwind binary |
| `--force-download` | `false` | Re-download the Tailwind CLI even if cached |
| `-v`, `--verbose` | `false` | Verbose logging |

### CLI examples

```sh
dotnet run --project src/Stu -- --colors gray,red,green -o utilities.css
dotnet run --project src/Stu -- --base-font-size 14 --minify -o utilities.min.css
dotnet run --project src/Stu -- --colors blue,indigo -o theme.css -v
```

## What it does

Each run performs this pipeline:

1. **Downloads** the Tailwind v4 standalone CLI binary (cached locally)
2. **Discovers** all utility class names by fetching and parsing Tailwind's `utilities.ts` source from GitHub
3. **Assembles** candidate class names by combining discovered prefixes with value scales (spacing, colors, fractions, etc.)
4. **Generates** an `input.css` with `@source inline()` directives for all candidates
5. **Runs** the Tailwind CLI to produce a full CSS output
6. **Parses** the generated CSS and extracts all style rules
7. **Resolves** CSS custom properties (`var(--spacing)`, `var(--color-*)`, etc.) to their computed values, and evaluates `calc()` expressions
8. **Filters** out responsive prefixes, state variants, pseudo-classes, arbitrary value classes, and Tailwind internal `--tw-*` plumbing
9. **Transforms** `rem`/`em` to `px`, unitless `line-height` to `px`, and color functions (`oklch`, `rgb`, `hsl`) to hex
10. **Aggregates**, deduplicates, and sorts rules
11. **Writes** the final CSS file

Because utility names are discovered from Tailwind's source rather than hardcoded, Stu automatically picks up new utilities when Tailwind releases updates.

## Features

- **MSBuild integration** — runs as a build action on publish, build, or both
- **No Node.js** — uses the Tailwind standalone CLI binary, downloaded automatically
- **Auto-discovery** — utility class names are parsed from Tailwind's source, not hardcoded
- **All utility classes** — layout, spacing, sizing, typography, backgrounds, borders, effects, filters, flexbox, grid, transforms, transitions, interactivity, SVG, accessibility, tables, and gradients
- **Color family filtering** — reduce output size by including only the color families you need
- **Full variable resolution** — all Tailwind CSS custom properties resolved to concrete values
- **Unit conversion** — all `rem` and `em` values converted to `px`, unitless `line-height` computed to `px`
- **Color conversion** — `oklch()`, `oklab()`, `rgb()`, `hsl()` all converted to `#RRGGBB` (or `#RRGGBBAA` for transparent colors)
- **Clean output** — no responsive breakpoints, no hover/focus/active variants, no arbitrary values, no internal `--tw-*` custom properties, no empty declarations
- **Minified output** option
- **Cross-platform** — works on Windows, Linux, and macOS (x64 and ARM64)

## What's excluded (by design)

- Responsive prefixes (`sm:`, `md:`, `lg:`, etc.)
- State variants (`hover:`, `focus:`, `active:`, `disabled:`, etc.)
- Pseudo-elements (`before:`, `after:`, `placeholder:`, etc.)
- Arbitrary values (`w-[100px]`, `bg-[#ff0000]`, etc.)
- Stacking/combining classes
- `@keyframes` and `@media` blocks

## Output format

The generated CSS contains one rule per utility class with fully computed values:

```css
/* Generated by Stu — Tailwind Utility CSS Transformer | tailwindcss v4.2.2 */
/* 5200 rules | 2026-04-01 19:30:00 UTC */

.p-4 {
  padding: 16px;
}

.text-red-500 {
  color: #EF4444;
}

.text-lg {
  font-size: 18px;
  line-height: 28px;
}

.rounded-lg {
  border-radius: 8px;
}

.top-1\/2 {
  top: 50%;
}
```

## Publishing the package

```sh
cd src/Stu
dotnet publish -c Release -o tools
dotnet pack -c Release --no-build
```

The resulting `.nupkg` in `bin/Release/` contains the tool in `tools/`, MSBuild integration in `build/` and `buildTransitive/`, and the README.

## Project structure

```
src/
  Stu/
    build/              MSBuild .props and .targets for NuGet consumers
    buildTransitive/    Same, for transitive package references
    Config/             Color families, CLI options
    Discovery/          Auto-discovery of utility names from Tailwind source
    Download/           Tailwind CLI binary downloader + platform detection
    Generation/         input.css builder + Tailwind CLI runner
    Parsing/            Regex-based CSS parser
    Transformation/     Variable resolution, unit/color/line-height conversion,
                        filtering, aggregation
    Output/             CSS writer
    Program.cs          Pipeline orchestration
  Stu.Tests/
    Discovery/          Utility discovery + class name assembler tests
    Parsing/            CSS parser tests
    Transformation/     Unit converter, color converter, variable resolver,
                        line-height converter tests
```

## Building from source

Requires .NET 10 SDK.

```sh
dotnet build src/Scott.Tailwind.UtilityClassDeposer.slnx
dotnet test src/Scott.Tailwind.UtilityClassDeposer.slnx
```
