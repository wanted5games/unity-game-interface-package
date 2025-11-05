# Changelog

All notable changes to this package will be documented in this file.

## [1.0.0] - 2025-11-05

### Added

- **Famobi.json Editor Window**: Custom Unity Editor window (`Game Interface ▸ Famobi.json Editor`) for managing `famobi.json` configuration without manual file editing.
  - Edit interstitial and rewarded ad eventIds with format validation
  - Manage IAP products (SKU, title, description, image URI, price)
- **Storage size monitoring**: Warnings logged when individual storage items or total storage size approach configurable limits (default: 1 MB each).
  - Warnings shown at 50% and 100% of limits
  - Each warning only shown once per key to prevent log spam
- **Storage caching**: Automatic in-memory caching for `GetStorageItem` to reduce overhead from frequent reads.
- **IAP product loading from JSON**: `GetIAPProducts()` now automatically loads products from `famobi.json` and merges them with API results.
- **Auto mute/pause behavior**: Automatic mute and pause handling when game visibility changes.
- **MIT License**: Added MIT license file to the package.
- **WebGL Template Installer improvements**:
  - Only copies/updates specific tracked files instead of comparing entire folder
  - Only updates individual changed files instead of deleting and recreating the folder
  - Automatically ignores `.meta` files (Unity generates them)

### Fixed

- **IAP behavior in editor**: Fixed IAP product handling when running in Unity Editor.
- **GameInterface settings fetch**: Fixed issue with fetching correct GameInterface settings.
- **EventId validation**: Added check if ad eventId exists in `famobi.json` before use.

### Changed

- **Storage limits**: Added configurable `MaxStorageItemBytes` and `MaxTotalStorageBytes` properties (default: 1 MB each).
- **README improvements**:
  - Added Storage section with usage examples
  - Enhanced famobi.json documentation with IAP product information
  - Added comprehensive error handling examples for all promise patterns
  - Improved code examples with proper error handling
- **Package description**: Added prominent reminder to read README.md before starting integration.
- **Repository URL**: Updated to correct repository URL.

## [0.1.1] - 2025-10-28

### Added

- Initial Unity package for Famobi Game Interface.
- WebGL bridge (GameInterface.jslib) moved to Runtime/Plugins/WebGL/.
- Optional onError callbacks.
- RewardedAdButton.
- CopyrightLogo helper.

### Fixed

- JS↔C# interop for string returns via UTF8 buffers (GetOffsets, GetConfig, GetCopyrightLogoURL).
- Storage removal bug in JS glue (RemoveStorageItem using key).
- Rewarded availability event payload shape to top-level fields.

### Changed

- Simplified the steps how to use the GameInterface Tester.
- Removed MonoBehaviour from GameInterface.
- README with "Before you start" section and stronger TL;DR pointers.

## [0.1.0] - 2025-10-24

### Added

- Initial Unity package for Famobi Game Interface.
