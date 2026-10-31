# Barstow PC Pros Remote Support

This is the source for the **Barstow PC Pros Remote Support** app (`BarstowPCPros-Support.exe`), a branded build of
[Remotely](https://github.com/immense/Remotely) by Immense Networks, licensed under the GNU GPL v3 (see `LICENSE`).

Based on upstream tag `v2024.08.07.0819`. Changes are on the `barstow-branding` branch:

- Branded Windows desktop client: name, icons, file properties, and a redesigned main window, permission prompt and session banner.
- Built-in server address (`https://support.barstowpcpros.com/`) and organization, so the app works under its plain file name.
  A `[...]` server tag in the file name still overrides it, like upstream.

Build:

```
dotnet publish Desktop.Win -p:PublishProfile=desktop-win-x64 -c Release
```

Questions: Barstow PC Pros, (760) 282-4859, https://barstowpcpros.com
