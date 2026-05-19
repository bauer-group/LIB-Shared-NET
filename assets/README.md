# Assets

Place package assets here:

- `ApplicationIcon.png` — embedded in NuGet packages as `PackageIcon` (referenced from `Directory.Build.props`, conditional on existence — packing works without it)
- `Application.ico` — Windows application icon for host EXEs (referenced from individual host csproj files when present)
