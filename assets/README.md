# Assets

Place package assets here:

- `ApplicationIcon.png` — embedded in NuGet packages as `PackageIcon` (referenced from `Directory.Build.props`, conditional on existence — packing works without it)
- `Application.ico` — Windows application icon for host EXEs (referenced from individual host csproj files when present)

The sister [Plattform repo](https://github.com/bauer-group/LIB-Shared-Plattform-NET) ships these in its `assets/` folder; the Business framework can reuse those files for visual consistency.
