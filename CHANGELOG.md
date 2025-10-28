# Changelog

All notable changes to this package will be documented in this file.

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
