# Build assets

## Assembly signing key

Strong-name signing is **conditional** — it activates only if `build/BAUERGROUP.Shared.snk` exists. Builds without the key still succeed (assemblies are unsigned).

### Local development

If you want signed local builds, drop your SNK file at `build/BAUERGROUP.Shared.snk`. The file is ignored by git via the `*.snk` rule in `.gitignore`.

Generate a new key with:

```bash
sn -k BAUERGROUP.Shared.snk
```

### CI/CD

The release workflow consumes the SNK from the `DOTNET_SIGNKEY_BASE64` GitHub Secret, base64-decoded into `build/BAUERGROUP.Shared.snk` before build. See `.github/workflows/dotnet-release.yml` for the wiring.
