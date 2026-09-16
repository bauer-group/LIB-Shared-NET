# Changelog

All notable changes to this project are documented here. This file is maintained
automatically by [semantic-release](https://github.com/semantic-release/semantic-release)
on every release to `main`.

## [4.0.2](https://github.com/bauer-group/LIB-Shared-NET/compare/v4.0.1...v4.0.2) (2026-09-16)

### 🐛 Bug Fixes

* **browser:** replaced missing icon resources with vector geometry ([2f32af6](https://github.com/bauer-group/LIB-Shared-NET/commit/2f32af667ba8bfa9f4d3da03f9ac932e2f310633)), closes [#139](https://github.com/bauer-group/LIB-Shared-NET/issues/139)
* **logging:** fixed the doubled colon in the shared layout ([4c417c6](https://github.com/bauer-group/LIB-Shared-NET/commit/4c417c622834d49edb7d42728039944216d83ba9))
* **logging:** made the log target toggles idempotent ([75918b4](https://github.com/bauer-group/LIB-Shared-NET/commit/75918b4fcd47cb984d53836cd9391fea103195ac)), closes [#138](https://github.com/bauer-group/LIB-Shared-NET/issues/138)

## [4.0.1](https://github.com/bauer-group/LIB-Shared-NET/compare/v4.0.0...v4.0.1) (2026-09-16)

### 🐛 Bug Fixes

* **build:** made InternalsVisibleTo work in unsigned builds ([42d6761](https://github.com/bauer-group/LIB-Shared-NET/commit/42d676131497306622f8d7f257af8dd6ee382e49))
* **security:** lifted the vulnerable SqlClient from Stimulsoft ([9b620a0](https://github.com/bauer-group/LIB-Shared-NET/commit/9b620a058525a8d5b503244eb239e4a6c4c5d864))

## [4.0.0](https://github.com/bauer-group/LIB-Shared-NET/compare/v3.1.0...v4.0.0) (2026-09-16)

### ⚠ BREAKING CHANGES

* **data:** BAUERGROUP.Shared.Data no longer brings in
  SQLitePCLRaw.bundle_e_sqlcipher. Applications that open encrypted
  SQLite databases through it must reference an encryption provider
  themselves (e.g. SQLite3 Multiple Ciphers or a licensed SQLCipher
  build). Applications targeting .NET Framework through the
  netstandard2.0 build now need .NET Framework 4.7.1 or later
  (SQLitePCLRaw 3 requirement).
* **desktop:** BAUERGROUP.Shared.Desktop no longer brings in
  ReactiveUI, Splat or Splat.NLog. Applications that use them must add
  their own PackageReference.

### deps

* **data:** removed the SQLCipher bundle from Shared.Data ([502c28a](https://github.com/bauer-group/LIB-Shared-NET/commit/502c28a50f745d6b97a584238d153f8419e39113))
* **desktop:** removed unused ReactiveUI and Splat references ([ce9d992](https://github.com/bauer-group/LIB-Shared-NET/commit/ce9d9924427b6fc580fc795e7077c52bfa0ec1e7))

### 🚀 Features

* **avalonia:** added a cross-platform live log viewer package ([500405e](https://github.com/bauer-group/LIB-Shared-NET/commit/500405e0fc15dcaaf6acbd4164819f02072d3950)), references [#136](https://github.com/bauer-group/LIB-Shared-NET/issues/136)
* **logging:** added an in-process live log sink and view buffer ([16daa65](https://github.com/bauer-group/LIB-Shared-NET/commit/16daa65371621098b86ed9b9bf1f842b57d89138)), references [#136](https://github.com/bauer-group/LIB-Shared-NET/issues/136)

### 🐛 Bug Fixes

* **ci:** pinned the Linux validation job to net10.0 ([8620f0e](https://github.com/bauer-group/LIB-Shared-NET/commit/8620f0e29cf0293249158a834f0737fdac54b6be))
* **desktop:** rebuilt the log viewer on the in-process live sink ([95e9736](https://github.com/bauer-group/LIB-Shared-NET/commit/95e9736e1c06b9615dc2b037b1ca8e622d36dcf0)), references [#136](https://github.com/bauer-group/LIB-Shared-NET/issues/136)
* **desktop:** restored the missing window icon resource ([73a546d](https://github.com/bauer-group/LIB-Shared-NET/commit/73a546dcae4419738cc1c3a0d9d4048e351a3fb2))

### ♻️ Code Refactoring

* **logging:** deprecated the UDP log listener and trace listener ([334de66](https://github.com/bauer-group/LIB-Shared-NET/commit/334de66bc1b34454d3f931ebf86bfb827ac5b2d5)), references [#136](https://github.com/bauer-group/LIB-Shared-NET/issues/136)

## [3.1.0](https://github.com/bauer-group/LIB-Shared-NET/compare/v3.0.8...v3.1.0) (2026-09-15)

### 🚀 Features

* **error-tracking:** enabled the Sentry offline cache by default ([735bce6](https://github.com/bauer-group/LIB-Shared-NET/commit/735bce69c1836bb4b614af25b24b1c80cc300877)), closes [#133](https://github.com/bauer-group/LIB-Shared-NET/issues/133)

### 🐛 Bug Fixes

* **ci:** added the missing permissions block ([c803b6e](https://github.com/bauer-group/LIB-Shared-NET/commit/c803b6e66870292bbad7c367cb433362b2830a2c))
* **core:** fixed empty names and folders in single-file applications ([155e7c7](https://github.com/bauer-group/LIB-Shared-NET/commit/155e7c7989964f07d381c06b9f8004ff375775fe)), closes [#132](https://github.com/bauer-group/LIB-Shared-NET/issues/132)
* **core:** resolved Linux data and log folders to writable locations ([afdb837](https://github.com/bauer-group/LIB-Shared-NET/commit/afdb837a50ae14654dc323d6a30ee30db0cd531e)), closes [#135](https://github.com/bauer-group/LIB-Shared-NET/issues/135)
* **logging:** fixed daily log archives collapsing into one file ([8da54ab](https://github.com/bauer-group/LIB-Shared-NET/commit/8da54abe7bdfceb147055b9a10ac6ed7c0911e6a)), closes [#137](https://github.com/bauer-group/LIB-Shared-NET/issues/137)
* **logging:** replaced hard-coded backslash in log file paths ([e00d4c6](https://github.com/bauer-group/LIB-Shared-NET/commit/e00d4c6c401f09ed4b162a492840218d26ded134)), closes [#134](https://github.com/bauer-group/LIB-Shared-NET/issues/134)

### 🔧 Maintenance

* **ci:** removed redundant teams notification ([02325dd](https://github.com/bauer-group/LIB-Shared-NET/commit/02325dd7bc0ccfcb3856509dd1aba6616197527d))
* **codeowners:** reassigned ownership to core team [skip ci] ([8f66d33](https://github.com/bauer-group/LIB-Shared-NET/commit/8f66d338c8c5ec098c764df34813c674bcb896e0))
* **codeowners:** reassigned ownership to core team [skip ci] ([87c8168](https://github.com/bauer-group/LIB-Shared-NET/commit/87c8168f5154ca6b1552af5eee6ececf52da5ef0))

## [3.0.8](https://github.com/bauer-group/LIB-Shared-NET/compare/v3.0.7...v3.0.8) (2026-06-14)

## [3.0.7](https://github.com/bauer-group/LIB-Shared-NET/compare/v3.0.6...v3.0.7) (2026-06-14)

## [3.0.6](https://github.com/bauer-group/LIB-Shared-NET/compare/v3.0.5...v3.0.6) (2026-06-14)

### ♻️ Refactoring

* **repo:** renamed to BAUERGROUP.Shared, migrated to slnx ([c039d39](https://github.com/bauer-group/LIB-Shared-NET/commit/c039d395cd243033a76ee76079fcf221770d420b))

## [3.0.5](https://github.com/bauer-group/LIB-Shared-NET/compare/v3.0.4...v3.0.5) (2026-06-12)

### 🐛 Bug Fixes

* update copyright year to reflect the current year dynamically ([e040c6e](https://github.com/bauer-group/LIB-Shared-NET/commit/e040c6e4fbeab507f9ce7c6b066e2725cfb0357e))

## [3.0.4](https://github.com/bauer-group/LIB-Shared-NET/compare/v3.0.3...v3.0.4) (2026-04-05)

### 🐛 Bug Fixes

* Increase timing margins in SchedulerTests to prevent flaky CI failures ([#42](https://github.com/bauer-group/LIB-Shared-NET/issues/42)) ([f92c330](https://github.com/bauer-group/LIB-Shared-NET/commit/f92c3306e34517690cb7b72cf578ea828968cb1d))
* Increase timing margins in SchedulerTests to prevent flaky CI failures ([#42](https://github.com/bauer-group/LIB-Shared-NET/issues/42)) ([ead5fe6](https://github.com/bauer-group/LIB-Shared-NET/commit/ead5fe6501e1f0c411789930875cf6ade733c124))
* make assembly signing conditional on SNK key file existence ([8492cd8](https://github.com/bauer-group/LIB-Shared-NET/commit/8492cd8e9c5faae9778b06630c09ddcebab1edaf))

## [3.0.3](https://github.com/bauer-group/LIB-Shared-NET/compare/v3.0.2...v3.0.3) (2026-03-25)

### 🐛 Bug Fixes

* Non-breaking bug fixes, code quality improvements, and dead dependency removal ([#41](https://github.com/bauer-group/LIB-Shared-NET/issues/41)) ([7978dd2](https://github.com/bauer-group/LIB-Shared-NET/commit/7978dd2eb1575d3391b844b6eeacd3dcc9442f0c))

## [3.0.2](https://github.com/bauer-group/LIB-Shared-NET/compare/v3.0.1...v3.0.2) (2026-01-28)

### 🐛 Bug Fixes

* Update CloudinaryDotNet and WebView2 package versions; upgrade System.Text.Json and AsyncInterfaces ([f684962](https://github.com/bauer-group/LIB-Shared-NET/commit/f68496201ec112d60e3b2401cea01b17320534cb))

## [3.0.1](https://github.com/bauer-group/LIB-Shared-NET/compare/v3.0.0...v3.0.1) (2026-01-28)

### 🐛 Bug Fixes

* Generic API service clients was missing ([1b16a51](https://github.com/bauer-group/LIB-Shared-NET/commit/1b16a51506ee251e5ce73b3e968861697360cc21))

## [3.0.0](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.3.0...v3.0.0) (2026-01-19)

### ⚠ BREAKING CHANGES

* Public Release 3.0.0

### 🔧 Chores

* Public Release 3.0.0 ([e41e7c9](https://github.com/bauer-group/LIB-Shared-NET/commit/e41e7c91498e10dff0a6e64bfe11c48c23226913))

## [2.3.0](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.2.0...v2.3.0) (2026-01-18)

### 🚀 Features

* Enhanced Codecoverage ([c7b87a5](https://github.com/bauer-group/LIB-Shared-NET/commit/c7b87a5a4340f1993bbd137c02a6d6c642e406c6))

## [2.2.0](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.1.9...v2.2.0) (2026-01-18)

### 🚀 Features

* XML doc to be released ([d463d88](https://github.com/bauer-group/LIB-Shared-NET/commit/d463d8829cdcadd655fb5cb9fa6b8379efe0e477))

## [2.1.9](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.1.8...v2.1.9) (2026-01-17)

### ♻️ Refactoring

* update warning suppression and improve thread safety in tests ([ae50cff](https://github.com/bauer-group/LIB-Shared-NET/commit/ae50cffe226642ad0bc3537c020320933d7d90e3))

## [2.1.8](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.1.7...v2.1.8) (2026-01-17)

### ♻️ Refactoring

* improve file reading and downloading utilities with enhanced error handling and async support ([b7e8c1f](https://github.com/bauer-group/LIB-Shared-NET/commit/b7e8c1fd6c9cd65263f86e19a4a11da45fa646bb))

## [2.1.7](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.1.6...v2.1.7) (2026-01-17)

### ♻️ Refactoring

* move MimeTypeSniffer to core and update tests for new namespace ([e2e3dd4](https://github.com/bauer-group/LIB-Shared-NET/commit/e2e3dd4f023013f3eb4ffc4ab4ef5752bbdcd328))

## [2.1.6](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.1.5...v2.1.6) (2026-01-17)

### 🐛 Bug Fixes

* update MIME type detection to return specific types and improve test cases ([a10b9af](https://github.com/bauer-group/LIB-Shared-NET/commit/a10b9afa2a4e311196a431c7b74beb8f71c040d4))
* update mime type handling to include application/octet-stream ([d45c912](https://github.com/bauer-group/LIB-Shared-NET/commit/d45c912aaad6919926fda671653b7f6c8d9a8367))

## [2.1.5](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.1.4...v2.1.5) (2026-01-17)

### ♻️ Refactoring

* remove hungarin notation refactor ([1dfb74e](https://github.com/bauer-group/LIB-Shared-NET/commit/1dfb74e3cb0aa9955dfee026c4233c5724e3646f))

## [2.1.4](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.1.3...v2.1.4) (2026-01-17)

### 🐛 Bug Fixes

* versioning clean up redundant entries ([f775647](https://github.com/bauer-group/LIB-Shared-NET/commit/f77564716d35542fe8898857003b569164642603))

## [2.1.3](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.1.2...v2.1.3) (2026-01-17)

### ♻️ Refactoring

* remove hungarin notation ([9f174ba](https://github.com/bauer-group/LIB-Shared-NET/commit/9f174ba0672905db27212e0495c8d873808ecce5))

## [2.1.2](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.1.1...v2.1.2) (2026-01-17)

### ♻️ Refactoring

* Update nullable reference types project ([a4f03dc](https://github.com/bauer-group/LIB-Shared-NET/commit/a4f03dc97382333c7ed7f2a3ab6020aee3364a6e))
* Update nullable reference types project ([9224db2](https://github.com/bauer-group/LIB-Shared-NET/commit/9224db249f80fe9266856f1d4a08aeae19bc2484))
* Update nullable reference types project ([90cab37](https://github.com/bauer-group/LIB-Shared-NET/commit/90cab37cc66683b58fb7efc8806283742e9c298e))

## [2.1.1](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.1.0...v2.1.1) (2026-01-17)

### ♻️ Refactoring

* Update nullable reference types project ([eec566f](https://github.com/bauer-group/LIB-Shared-NET/commit/eec566feaf9e9815fc117e5586fa803d4bcbdbd6))

## [2.1.0](https://github.com/bauer-group/LIB-Shared-NET/compare/v2.0.1...v2.1.0) (2026-01-17)

### 🚀 Features

* Add comprehensive unit tests for core components ([b253abe](https://github.com/bauer-group/LIB-Shared-NET/commit/b253abe13e2f33d33d3faac12618780616f42deb))
* Update logging configuration to set default log directory ([4d3f87f](https://github.com/bauer-group/LIB-Shared-NET/commit/4d3f87ff51b1f44e33c6aa6887a8e1c0b2693d3c))
