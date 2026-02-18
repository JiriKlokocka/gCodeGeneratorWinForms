# gCodeGeneratorWinForms — CLAUDE.md

## Project Overview

A Windows Forms (.NET 10) application that generates G-code for CNC lathe turning operations. Users input turning parameters (dimensions, feeds, radii/chamfers) and the app produces G-code, displays it as text, and renders a 2D toolpath preview.

## Architecture

### Key Files

- `gCodeGeneratorWinForms/Gcodegenerator.cs` — Core logic: `TurningParameters` (data class) + `GCodeGenerator` (produces G-code string)
- `gCodeGeneratorWinForms/Form1.cs` — UI: input panel builder, parameter reading, G-code viewer, 2D toolpath renderer
- `gCodeGeneratorWinForms/Form1.Designer.cs` — Designer-generated controls (SplitContainers, panels, TextBoxes, CheckBoxes)
- `gCodeGeneratorWinForms/Program.cs` — Entry point

### Solution

- `gCodeGeneratorWinForms.slnx` — Visual Studio solution (new `.slnx` format)
- Target framework: `net10.0-windows`

## Domain Knowledge

### Coordinate System (Lathe)

- **X axis** — diameter/radius (radial, perpendicular to spindle axis)
- **Z axis** — along spindle axis; Z=0 is the right face, negative Z goes left (into the part)
- G-code uses **radius** values for X (not diameter), multiplied correctly in generator
- Viewer maps: Z=0 at screen right (inverted), X=0 at screen top (positive grows down)

### G-code Conventions

- `G0` — rapid move
- `G1` — linear feed
- `G2` — arc clockwise
- `G3` — arc counter-clockwise
- `I`, `K` — arc center offsets (relative to arc start, in X and Z respectively)
- `G18` — XZ plane selection (required for lathe arcs)
- `M3 S500` — spindle on CW at 500 RPM; `M5` — spindle stop
- All values use dot decimal (locale-safe via `CultureInfo.InvariantCulture`); commas replaced with dots at output
- Coordinates rounded to 4 decimal places via `R()` helper

### Turning Parameters (`TurningParameters`)

| Property | Description |
|---|---|
| `Length` | Part length (mm) |
| `InitialDiameter` | Starting stock diameter |
| `TargetDiameter` | Final turned diameter |
| `Cut` | Depth of cut per pass (min 0.01, self-corrected to distribute remainder evenly) |
| `RoughFeed` / `FinishFeed` | Feed rates (mm/min); finish used on last pass |
| `LeftRadius` | Radius at left (far) end; cannot exceed `depth` or `Length/2` |
| `LeftChamfer` | If true, chamfer instead of radius at left end |
| `RightRadius` | Radius at right (near) end: positive = outer radius, negative = inner radius |
| `RightChamfer` | If true, chamfer instead of radius at right end |
| `AutoRadies` | Auto-sets left/right radii to max possible values |
| `ShowArrows` | Show direction arrows on 2D viewer |
| `Clear` | Clearance distance for rapid retract (mm) |
| `FileName` | Output file path (default: `C:\Mach3\GCode\_myFile.gcode`) |
| `LastCutTest` | Internal flag (0 = special last-cut arc behavior) |

### G-code Generation Logic

1. Divides total depth into `mainSteps` passes; distributes any remainder evenly across all steps
2. Per pass:
   - Right side: outer radius (G03), inner radius (G02), chamfer (G01), or straight approach
   - Feed move along Z to length
   - Left side: radius (G02) or chamfer (G01) at end of part
   - Retract and rapid return to Z=0
3. Arcs grow incrementally each pass (`rightRadiusFraction` per step for outer right radius)
4. Offset calculations (`leftOffset`, `rightOffset`) determine at which depth arcs begin

### 2D Viewer

- Parses generated G-code back into `Segment` list (rapid/feed/arc)
- Arcs rendered as polylines (32 steps) to avoid GDI+ flip issues
- Colors: Red dashed = rapid, Blue = feed, Green = arc
- Aborts rendering if segment count > 1500 (too many steps)

## Build & Run

```
dotnet build
dotnet run --project gCodeGeneratorWinForms
```

Or open `gCodeGeneratorWinForms.slnx` in Visual Studio 2022+.

## Common Pitfalls

- **Locale**: Always use `CultureInfo.InvariantCulture` for all numeric parsing and formatting. Czech locale uses comma as decimal separator; the app explicitly handles this.
- **Arc sweep calculation**: The sweep angle logic in `ParseGCode` was manually adjusted (see comment "New Code" in `Form1.cs`). Be careful modifying it.
- **`AutoRadies` typo**: The property is intentionally spelled `AutoRadies` (not `AutoRadii`) — matches the existing codebase.
- **Segment count limit**: The viewer bails at >1500 segments. Very small `Cut` values with large depth will hit this.
- **`LastCutTest`**: Currently hardcoded to `1` in `TurningParameters`; value `0` enables a different last-cut arc path in the generator.
