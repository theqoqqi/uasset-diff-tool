
# UAsset Diff Tool

A command-line tool for comparing `.uasset` files to identify differences between two versions of a game’s assets.

This tool was developed to help modders track changes between game updates and replicate modified assets in
their own Unreal Engine 4 projects. Since the game does not provide any modding API, assets often need
to be recreated manually. This tool streamlines the process by highlighting differences.

## Features

- Compares individual `.uasset` files or directories.
- Supports filtering by referenced dependencies from UE4 project.
- Provides option to handle renamed files.
- Optionally focuses on Blueprint assets only.
- Outputs results to console or file.

## Installation

### Download a release
1. Grab the latest binary from Releases and unpack it anywhere.
2. Run the tool using command line:
```sh
UAssetDiffTool.exe <pathA> <pathB> [options]
```

### Manual installation

1. Clone and build [UAssetAPI](https://github.com/atenfyr/UAssetAPI) manually. It's not available on NuGet.
2. Clone this repository
3. In the solution for **UAssetDiffTool**, add a project reference to the built **UAssetAPI**
4. Build

## Usage

```sh
UAssetDiffTool.exe <pathA> <pathB> [options]
```

### Arguments

- `pathA`: Path to the first `.uasset` file or directory (e.g. old version).
- `pathB`: Path to the second `.uasset` file or directory (e.g. new version).

### Options

- `--output <file>`  
  If set, writes output to a file instead of the console.

- `--renamed-files <file>`  
  Path to a file listing renamed files (space-separated pairs).  
  Example:
  ```
  Content/main/lib1.uasset Content/main/lib.uasset
  Content/audio/misc/att_default.uasset Content/audio/misc/att_default_AO.uasset
  ```

- `--filter-by-deps <file>`  
  Only show diffs for assets listed in a UE4 dependency file.  
  See workflow section below for how to generate it.

- `--diff-types <Added|Removed|Changed>`  
  Types of differences to include. Can be multiple values, separated by commas or spaces.  
  Default: `Added`, `Removed`, `Changed`

- `--blueprints-only`  
  Only include assets that are Blueprint classes (i.e., have functions or properties).

## Workflow Example

1. **Extract game assets** using [FModel](https://fmodel.app) for both the old and new game versions.
2. Put them in two folders:
   ```
   /old-game-assets/...
   /new-game-assets/...
   ```
3. Generate a dependency list from UE4:
    - Select relevant assets (e.g., your mod folders) in Content Browser.
    - Right-click → `Reference Viewer`.
    - Right-click your selected assets node → `Copy referenced objects list`.
    - Paste into a `.txt` file, e.g.  `project-deps.txt`.

4. **First comparison run**:
   ```sh
   UAssetDiffTool.exe old-game-assets new-game-assets --output diff.txt --filter-by-deps project-deps.txt
   ```

5. **Handle renamed assets**:
    - Manually inspect removed/added assets and note file renames.
    - Create `renamed-files.txt` with the old and new file paths (space-separated).

6. **Final run** with rename mapping:
   ```sh
   UAssetDiffTool.exe old-game-assets new-game-assets --output diff.txt --filter-by-deps project-deps.txt --renamed-files renamed-files.txt
   ```

## Example Output

```
- Asset 'objects\test' Removed
+ Asset 'objects\coolBox' Added
~ Asset 'main\lib1' Changed
  Path: main\lib1 => main\lib
~ Asset 'objects\actorChipPile' Changed
  Property changes:
    - Property 'flameBase' Removed
    + Property 'fireHealth' Added
  Function changes:
    ~ Function 'microwave' Changed
        Input param changes:
          + Property 'microwave' Added
    ~ Function 'getActionOptions' Changed
        Input param changes:
          + Property 'numberIn' Added
    - Function 'turnToPile1' Removed
    + Function 'turnToPile' Added
```

## License

This project is licensed under the [MIT License](LICENSE).
