# Tailwind CSS Utility Extractor — Implementation Plan

## Context

Build a .NET 8 console app (`stu`) that downloads the Tailwind v4 standalone CLI, generates a complete CSS file of all utility classes, then parses/filters/transforms it into a simplified CSS file with px sizes and hex colors. Designed for simple HTML templates that need Tailwind classes without the full Tailwind build pipeline.

## Why Tailwind v4

- Standalone CLI binaries on GitHub (no Node.js needed)
- CSS-first config: `@import "tailwindcss"` + `@source inline()` for safelisting
- `@theme { --breakpoint-*: initial; }` disables responsive prefixes at generation time
- We never include variant prefixes in `@source inline()`, so no hover:/focus:/etc. are generated

## Pipeline

```
1. Parse CLI args
2. Download/cache Tailwind v4 standalone CLI from GitHub releases
3. Build input.css programmatically (with @source inline() for all utility patterns)
4. Run: ./tailwindcss -i input.css -o output.css
5. Parse generated CSS (regex-based — Tailwind output is flat and predictable)
6. Filter: remove any leftover variants, arbitrary values, @keyframes, @media
7. Transform: rem/em → px, oklch/rgb/hsl → hex
8. Aggregate: merge duplicates, sort, remove --tw-* internals
9. Write final CSS
```

## Project Structure

```
src/
  Stu/
    Stu.csproj
    Program.cs                        # Entry point + CLI parsing
    Config/
      CliOptions.cs                   # --colors, --output, --base-font-size, etc.
      TailwindColorFamilies.cs        # 22 color families + shades (50–950)
      UtilityCatalog.cs               # Full catalog of all utility patterns
    Download/
      TailwindCliDownloader.cs        # Download + cache binary from GitHub
      PlatformDetector.cs             # OS/arch → binary name mapping
    Generation/
      InputCssBuilder.cs              # Builds input.css with @source inline() directives
      TailwindRunner.cs               # Executes CLI as subprocess
    Parsing/
      CssParser.cs                    # Regex-based CSS rule extraction
      CssRule.cs                      # Selector + declarations model
      CssDeclaration.cs               # Property + value model
    Transformation/
      VariantFilter.cs                # Strip pseudo-class/element/media selectors
      ArbitraryValueFilter.cs         # Strip bracket-notation classes
      UnitConverter.cs                # rem/em → px
      ColorConverter.cs               # oklch/rgb/hsl → hex
      CssAggregator.cs               # Merge duplicates, sort, clean
    Output/
      CssWriter.cs                    # Write formatted CSS
  Stu.Tests/
    Stu.Tests.csproj
    Transformation/
      UnitConverterTests.cs
      ColorConverterTests.cs
    Parsing/
      CssParserTests.cs
```

## Dependencies

| Package | Purpose |
|---------|---------|
| `System.CommandLine` (2.0.0-beta4) | CLI argument parsing |
| `xunit` + runner | Unit tests |

No CSS parser library needed — regex-based parsing is sufficient for Tailwind's machine-generated, flat CSS output.

## CLI Design

```
stu [options]

  -o, --output <path>        Output file [default: tailwind-utilities.css]
  -c, --colors <families>    Comma-separated families or "all" [default: all]
                              Valid: slate,gray,zinc,neutral,stone,red,orange,amber,
                              yellow,lime,green,emerald,teal,cyan,sky,blue,indigo,
                              violet,purple,fuchsia,pink,rose
  --cache-dir <path>         Binary cache dir [default: ~/.stu/cache]
  --force-download           Re-download even if cached
  --base-font-size <px>      1rem = Npx [default: 16]
  --minify                   Minified output
  -v, --verbose              Verbose logging
```

Example: `stu --colors gray,red,green -o my-utilities.css`

## Key Implementation Details

### Generating all utilities (`UtilityCatalog.cs` + `InputCssBuilder.cs`)

Tailwind v4 is JIT-only, so we enumerate every utility pattern via `@source inline()` with brace expansion:

```css
@import "tailwindcss";
@theme { --breakpoint-*: initial; }

@source inline("p-{0,px,0.5,1,1.5,2,2.5,3,3.5,4,5,6,8,10,12,14,16,20,24,28,32,36,40,44,48,52,56,60,64,72,80,96}");
@source inline("m-{0,px,0.5,1,...,auto}");
@source inline("bg-red-{50,100,200,300,400,500,600,700,800,900,950}");
/* ... all utility categories ... */
```

`UtilityCatalog.cs` is the data backbone — full enumeration of every Tailwind utility by category (layout, spacing, sizing, typography, backgrounds, borders, effects, filters, flexbox/grid, transforms, transitions, interactivity, SVG, accessibility). Color-bearing utilities are filtered by the `--colors` parameter.

### Color filtering

For each color-bearing prefix (`bg`, `text`, `border`, `ring`, `shadow`, `fill`, `stroke`, etc.), generate `@source inline()` only for requested families. Always include `black`, `white`, `transparent`, `current`, `inherit`.

### CSS parsing (`CssParser.cs`)

Regex-based, no library needed. Pre-strip `@layer` wrappers, `@property` blocks, `@keyframes`. Then extract rules with:
- Selector + declaration block pattern
- Split declarations by `;`

### Unit conversion (`UnitConverter.cs`)

Regex: `(-?\d*\.?\d+)rem` → multiply by base font size → `{result}px`. Same for `em`. Handle compound values (`0.5rem 1rem` → `8px 16px`).

### Color conversion (`ColorConverter.cs`)

Primary target: `oklch()` (Tailwind v4's default). Pipeline:
1. OKLCH → OKLab (polar to Cartesian)
2. OKLab → linear sRGB (matrix transform)
3. Linear sRGB → sRGB (gamma correction)
4. sRGB → hex, clamp to 0-255

Also handle `rgb()`, `hsl()`, `oklab()`, with and without alpha channels. Alpha → 8-digit hex.

### Tailwind CLI download (`TailwindCliDownloader.cs`)

- Detect OS/arch → binary name (e.g., `tailwindcss-linux-x64`)
- Download from `https://github.com/tailwindlabs/tailwindcss/releases/latest/download/{name}`
- Cache locally, set executable permissions on Linux/macOS
- Re-use cache unless `--force-download`

## Implementation Order

1. **Scaffold**: `dotnet new console`, project structure, `CliOptions`
2. **Download**: `PlatformDetector` + `TailwindCliDownloader`
3. **Catalog**: `TailwindColorFamilies` + `UtilityCatalog` (the bulk of domain knowledge)
4. **Generation**: `InputCssBuilder` + `TailwindRunner`
5. **Parsing**: `CssParser` + models
6. **Transformation**: `UnitConverter`, `ColorConverter`, `VariantFilter`, `ArbitraryValueFilter`
7. **Output**: `CssAggregator` + `CssWriter`
8. **Program.cs**: Wire pipeline together
9. **Tests**: Unit tests for converter/parser/filter logic

## Verification

1. Run `stu --colors gray,red,green -o test.css -v` and inspect output
2. Verify no `hover:`, `focus:`, `sm:`, `md:` etc. in selectors
3. Verify no `rem` or `em` units in values — all converted to `px`
4. Verify no `oklch()`, `rgb()`, `hsl()` — all converted to `#hex`
5. Verify only gray/red/green color families present (plus black/white/transparent)
6. Run `stu --colors all` and confirm full output
7. Unit tests pass for color math, unit conversion, CSS parsing
