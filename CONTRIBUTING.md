# Contributing to BetterBetterTeleporter

Thanks for your interest in contributing! Community help is always appreciated.

---

## 🐞 Reporting Bugs or Asking Questions

- Check the **README** first: it covers how to use and configure the mod.
- If the issue isn’t covered, open a **GitHub Issue** with:
  - Steps to reproduce (if it’s a bug)
  - Expected vs. actual behavior
  - Any relevant screenshots or log snippets

Questions and suggestions can go in issues, too (just label them accordingly).

---

## 💡 Suggesting Enhancements

If you have an idea for improvement:

- Open an **enhancement issue** describing what you’d like to see and why.
- You may **open a pull request** that implements it to speed things up.
- There's no guarantee your suggestion/implementation will be approved.

---

## 🛠 How to Contribute

1. **Fork** the repository and create a new branch for your changes.
2. **Make your edits**, keeping the code clean and consistent.
3. **Update the README** if your changes affect installation, configuration, or usage.
4. **Open a Pull Request (PR)** with a clear description of what you changed and why.
5. **Be patient** if your changes are significant, as they will need to be properly tested.

---

## 💻 Building Locally

You can build the mod locally using the [.NET SDK](https://dotnet.microsoft.com/en-us/download).

### Basic Build Command

From the root of the repository, run:

```bash
dotnet build -c Release
```

This compiles the mod and runs all configured build steps.

### Build Outputs

The compiled DLL will appear under `BetterBetterTeleporter/bin/Release/netstandard2.1/`. The DLL will also be automatically copied into your BepInEx plugins folder. See variables in `Directory.Build.props`:

- `GameDir`: Install location for Lethal Company
- `BepInExDir`: Location of active BepInEx folder, which may depend on your mod manager.

`BetterBetterTeleporter/build.targets` will run the following automation:

1. **Version Sync**: Ensures the Thunderstore `manifest.json` version matches the `.csproj` version.
2. **Changelog Check**: If the changelog does not contain the current version, prints a warning message.
3. **Thunderstore Packaging**: Creates the zip file to upload to Thunderstore under `/builds`.
4. **Auto-Copy To Mod Folder**: Puts the DLL into the appropriate location from `BepInExDir` for immediate testing.

You can comment out any of these targets if you don't want them to execute on your local machine.

## 💬 Style & Guidelines

- Keep code readable and follow existing formatting.
- Test your changes before submitting.
- Include configuration for any new features.
- Be respectful and constructive in all discussions.

---

Thanks again for contributing and helping make the mod better!
