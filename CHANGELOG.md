## [1.8.11](https://github.com/LuxoriaSoft/Luxoria/compare/v1.8.10...v1.8.11) (2024-12-05)


### Bug Fixes

* Execute a clean code on the overall code base ([5ae0ef6](https://github.com/LuxoriaSoft/Luxoria/commit/5ae0ef695d2ea5a54f90ea0c94a56f4b2425a570))
* Include StartupTests file to test and cover Startup logic file ([ba078f9](https://github.com/LuxoriaSoft/Luxoria/commit/ba078f916d54fc4c62eb6ed8060638b698abd593))
* Make private assert functions static ([3ed16e0](https://github.com/LuxoriaSoft/Luxoria/commit/3ed16e089d38c0365fbc3aaddd2682ffd151826f))
* Run Code-cleaner on StartupTests file ([f0c59c6](https://github.com/LuxoriaSoft/Luxoria/commit/f0c59c60d7cf3a0e205704594d6c1fe88efd9dd1))

## [1.8.10](https://github.com/LuxoriaSoft/Luxoria/compare/v1.8.9...v1.8.10) (2024-12-05)


### Bug Fixes

* Add mutex (multi-thread safe) on all ImageData methods ([d68b8f0](https://github.com/LuxoriaSoft/Luxoria/commit/d68b8f0004cdab265f7642fb8f942623ef79959b))
* Clean sonar issues on Startup file ([f30794b](https://github.com/LuxoriaSoft/Luxoria/commit/f30794baa0623fe2e44973c0cb5515bb1a1e38e9))
* Clean sonar issues on this file ([3811bfa](https://github.com/LuxoriaSoft/Luxoria/commit/3811bfa79af67bd36b8163e5b0a3d8c94ba8ceeb))
* Make UpdateSlashScreenAsync function static ([74df2fc](https://github.com/LuxoriaSoft/Luxoria/commit/74df2fccf8c6f756d9d21e1af2a04bc23a77dcdb))

## [1.8.9](https://github.com/LuxoriaSoft/Luxoria/compare/v1.8.8...v1.8.9) (2024-12-01)


### Bug Fixes

* Rename build job to scan-sonarcloud in workflow configuration ([22b4da3](https://github.com/LuxoriaSoft/Luxoria/commit/22b4da3f182956de969fdd92cdccbd7a91646f0b))

## [1.8.8](https://github.com/LuxoriaSoft/Luxoria/compare/v1.8.7...v1.8.8) (2024-12-01)


### Bug Fixes

* Update commit-checker workflow to trigger on specific pull request events ([0d69964](https://github.com/LuxoriaSoft/Luxoria/commit/0d69964140845b0be526864ae6991f272532e74c))

## [1.8.7](https://github.com/LuxoriaSoft/Luxoria/compare/v1.8.6...v1.8.7) (2024-12-01)


### Bug Fixes

* Change hyperlink to access to SonarCloud from main readme ([7c51bd1](https://github.com/LuxoriaSoft/Luxoria/commit/7c51bd169c9ad02e0efac1c2f2e09961e26b2e8c))

## [1.8.6](https://github.com/LuxoriaSoft/Luxoria/compare/v1.8.5...v1.8.6) (2024-11-29)


### Bug Fixes

* Add SONAR_TOKEN to SonarQube analysis in GitHub Actions workflow ([a786685](https://github.com/LuxoriaSoft/Luxoria/commit/a786685960dd1823fb33c0fe8ba7c4265cf69519))

## [1.8.5](https://github.com/LuxoriaSoft/Luxoria/compare/v1.8.4...v1.8.5) (2024-11-29)


### Bug Fixes

* Update dotnet-coverage command in GitHub Actions workflow for improved test coverage collection ([896d18e](https://github.com/LuxoriaSoft/Luxoria/commit/896d18ed9e36ee0be87452d0af82e6e08be942ba))

## [1.8.4](https://github.com/LuxoriaSoft/Luxoria/compare/v1.8.3...v1.8.4) (2024-11-29)


### Bug Fixes

* Add installation step for .NET coverage tool in GitHub Actions workflow ([8e5d6dd](https://github.com/LuxoriaSoft/Luxoria/commit/8e5d6ddb9ec023b4e70d2ac3f19478505fc995fc))

## [1.8.3](https://github.com/LuxoriaSoft/Luxoria/compare/v1.8.2...v1.8.3) (2024-11-29)


### Bug Fixes

* Update GitHub Actions workflow to use SonarCloud and improve coverage reporting ([e398f25](https://github.com/LuxoriaSoft/Luxoria/commit/e398f25c14f53e292798f1fbeec9610fad470459))

## [1.8.2](https://github.com/Luxoria-EIP/Luxoria/compare/v1.8.1...v1.8.2) (2024-11-28)


### Bug Fixes

* Clean up README formatting by adding missing div closure ([b112f7d](https://github.com/Luxoria-EIP/Luxoria/commit/b112f7d5ccffb5c912468848e5fbf2c389b80b4c))

## [1.8.1](https://github.com/Luxoria-EIP/Luxoria/compare/v1.8.0...v1.8.1) (2024-11-28)


### Bug Fixes

* Update README to replace SonarQube links with SonarCloud and remove development release section ([ce46ec5](https://github.com/Luxoria-EIP/Luxoria/commit/ce46ec581edeab068fcfd54318254d0d429b607e))

# [1.8.0](https://github.com/Luxoria-EIP/Luxoria/compare/v1.7.0...v1.8.0) (2024-11-28)


### Features

* Simplify build workflow by removing unnecessary branch triggers and updating SonarQube command ([e37e995](https://github.com/Luxoria-EIP/Luxoria/commit/e37e9954960dc468173e0205a21611b86cd4d679))

# [1.7.0](https://github.com/Luxoria-EIP/Luxoria/compare/v1.6.0...v1.7.0) (2024-11-28)


### Bug Fixes

* Remove dual SAST checker on SonarQube ([62e2f28](https://github.com/Luxoria-EIP/Luxoria/commit/62e2f28bd2e94aae294d9c0e2aa98316620e748c))


### Features

* Update build workflow to trigger on main and develop branches, and add pull request event types ([29c74f5](https://github.com/Luxoria-EIP/Luxoria/commit/29c74f5ba22a4015235a3f90ab146528bf8ccd85))
* Update SonarQube scanner options in build workflow ([dcc6e35](https://github.com/Luxoria-EIP/Luxoria/commit/dcc6e35baa7a9d4fe99cf08d57f45d25de1cf5ca))

# [1.6.0](https://github.com/Luxoria-EIP/Luxoria/compare/v1.5.0...v1.6.0) (2024-11-27)


### Bug Fixes

* Merge develop into feat/core/main-menu ([9cef3db](https://github.com/Luxoria-EIP/Luxoria/commit/9cef3db71a4bf7bf6f50233ec31e67184a9f9e18))
* Removed unused files ([d6c7ff4](https://github.com/Luxoria-EIP/Luxoria/commit/d6c7ff4143e330e00e781dc08b57373a1332648f))


### Features

* Add Main Menu Bar component and module import setting ([ea3db93](https://github.com/Luxoria-EIP/Luxoria/commit/ea3db93cf7df7f3fe66d8470bfd24544d30daf1b))

# [1.5.0](https://github.com/Luxoria-EIP/Luxoria/compare/v1.4.0...v1.5.0) (2024-11-27)


### Bug Fixes

* Add comments inside Index function ([5cde3b4](https://github.com/Luxoria-EIP/Luxoria/commit/5cde3b4f1d4d61b20bbc11a1e1a3257785063f55))
* Add functions & methods explanations inside files ([37faa88](https://github.com/Luxoria-EIP/Luxoria/commit/37faa88e671aebe9a6a6ecd60af1b56df41a7207))
* Add functions explanations inside 'ImportService' file ([3b4e41d](https://github.com/Luxoria-EIP/Luxoria/commit/3b4e41d2f2a7e36b42dadf73b205829089aaf46b))
* Cleanup the code before merge to 'develop' ([261262e](https://github.com/Luxoria-EIP/Luxoria/commit/261262e9c4e4be2fb7297b394810164952eb0fbb))
* Create Luxoria.App graphical architecture ([a21021a](https://github.com/Luxoria-EIP/Luxoria/commit/a21021a9786403a7cfcfbb14740f41f986df51e2))
* LuxConfigRepository read Guid from json file ([b411639](https://github.com/Luxoria-EIP/Luxoria/commit/b4116390cdb68a1e05b6da836c43bae43aa13098))
* Move 'LuxVersion' inside Luxoria.Modules.Models namespace ([df154d4](https://github.com/Luxoria-EIP/Luxoria/commit/df154d4850ed160eab540bb382befbef89103f0b))
* Optimization of Index function in ImportService ([dc51033](https://github.com/Luxoria-EIP/Luxoria/commit/dc51033efb80fb4371ac85b823adbdcc3863dc80))
* Rename LuxAsset.Config to LuxAsset.MetaData ([ba148c9](https://github.com/Luxoria-EIP/Luxoria/commit/ba148c9ff6f502ad7984586c663bb88cd5ec49d7))


### Features

* Add a toast notification message when CollectionUpdatedEvent ([1c20676](https://github.com/Luxoria-EIP/Luxoria/commit/1c20676c5157e63522fe6386f5b245459c7fc511))
* Add base of Luxoria Indexing Process ([2751565](https://github.com/Luxoria-EIP/Luxoria/commit/2751565c39431bc472b175d877dde8fc0266aef5))
* Add handler to retreive updated collection on MainWindow ([d29f649](https://github.com/Luxoria-EIP/Luxoria/commit/d29f649eae51631ffb716c7703f4992d147bf5ac))
* Add importation cleanup process ([a90ad41](https://github.com/Luxoria-EIP/Luxoria/commit/a90ad410d1484a185a90a822f5ae6ca69394c752))
* Add Load Image bitmap using SixLabors.ImageSharp lib ([92a655f](https://github.com/Luxoria-EIP/Luxoria/commit/92a655fafea810fb3a2982f2835eabb523a3900d))
* Adding LuxCFG creation processus in indexication process ([b70b685](https://github.com/Luxoria-EIP/Luxoria/commit/b70b6854bc65c599b92811d08bd016b4e27c6166))
* Change mono-thread to multi-threads indexation process ([5983b22](https://github.com/Luxoria-EIP/Luxoria/commit/5983b229f166b0a3081e9d1b3b481900e75fb957))
* Close modal when importation is completed or has failed ([8dd21e7](https://github.com/Luxoria-EIP/Luxoria/commit/8dd21e7dcc431e4746a4cda05472796a3d36b33b))
* Refactor module loading logic in App.xaml.cs to load from directories with logging for missing modules. ([5b51c0c](https://github.com/Luxoria-EIP/Luxoria/commit/5b51c0cf449cec1a3c50777c65223c361e37587c))
* Upgrade 'LuxImport' solution to .NET 9.0 ([53afe91](https://github.com/Luxoria-EIP/Luxoria/commit/53afe91421955c22ed4c0157176b5352361bfa82))

# [1.4.0](https://github.com/Luxoria-EIP/Luxoria/compare/v1.3.0...v1.4.0) (2024-11-21)


### Features

* Fix documentation about technical stacks ([04503c7](https://github.com/Luxoria-EIP/Luxoria/commit/04503c7b094160197a631483442d3770ba8ae575))
* Upgrade .NET version to 9.0 ([5e06965](https://github.com/Luxoria-EIP/Luxoria/commit/5e06965b8ba6b24c4ea4f501b402cab53d6680ce))
* Upgrade release workflow to use .NET 9.0 ([375e61c](https://github.com/Luxoria-EIP/Luxoria/commit/375e61ca3c1a0c2946fe5174e26a127a536e702f))

# [1.3.0](https://github.com/Luxoria-EIP/Luxoria/compare/v1.2.0...v1.3.0) (2024-11-21)


### Features

* Update issue templates ([cb0b650](https://github.com/Luxoria-EIP/Luxoria/commit/cb0b650b4c1bbaebde596017bf977626f0f9aa71))

# [1.2.0](https://github.com/Luxoria-EIP/Luxoria/compare/v1.1.0...v1.2.0) (2024-11-21)


### Features

* Update issue templates ([f07d82f](https://github.com/Luxoria-EIP/Luxoria/commit/f07d82f799ea60ac6d963ac0f41b5a9aaf7a7395))
* Update issue templates ([f43d210](https://github.com/Luxoria-EIP/Luxoria/commit/f43d210b6aa7a945e97e33c4e788b8abb5ab031a))

# [1.1.0](https://github.com/Luxoria-EIP/Luxoria/compare/v1.0.0...v1.1.0) (2024-11-19)


### Bug Fixes

* Exclude Startup.cs and *.xaml (design files) from code coverage analysis ([04a3fe4](https://github.com/Luxoria-EIP/Luxoria/commit/04a3fe4c440f41393d9f6cc7eec2a03c54bad5ac))
* Unit test after logger function in initialize ([39fb1b3](https://github.com/Luxoria-EIP/Luxoria/commit/39fb1b3649800d2cfe831dfd84ce5ea9f3ade6d2))
* Update .gitignore file to Ignore .vscode, .DS_Store, any tmp files ([e173f4b](https://github.com/Luxoria-EIP/Luxoria/commit/e173f4b43b0b263b21c0f794c9471fb36ca9712d))
* Update commitlint.config.js to enforce subject case rules ([605e53b](https://github.com/Luxoria-EIP/Luxoria/commit/605e53b994f21d89ac686b317791454904d3cb35))
* Update README.md and package-lock.json ([549f988](https://github.com/Luxoria-EIP/Luxoria/commit/549f988c269525086fecf32b0ccf5008f39240a4))


### Features

* Add base of processing for LuxImport ([ac8e80b](https://github.com/Luxoria-EIP/Luxoria/commit/ac8e80b1184fdc627d10882f4d4988ce31c06b1e))
* Add code coverage test in build workflow ([66792ca](https://github.com/Luxoria-EIP/Luxoria/commit/66792ca55ea6ebe695331e83867a1ab965149d1c))
* add GitHub Actions workflow for building and packaging Luxoria.App ([f4ab8fb](https://github.com/Luxoria-EIP/Luxoria/commit/f4ab8fbb59c629f413baa0bfa18062e4ca45f242))
* Add Luxoria.SDK project ([2ac97f6](https://github.com/Luxoria-EIP/Luxoria/commit/2ac97f67a585e8d1e5d4ca3c069a98e01d8e99b2))
* add modular-arch (Luxoria.App) into main Luxoria repository ([c39fa25](https://github.com/Luxoria-EIP/Luxoria/commit/c39fa25f56c388504afec578eda557340c99e74c))
* Add UnitTest base ([c5688eb](https://github.com/Luxoria-EIP/Luxoria/commit/c5688ebcc8b8f9409456c9bfb55ab0850beec656))
* Create a second module called TestModule2 ([7603f00](https://github.com/Luxoria-EIP/Luxoria/commit/7603f00071d1467f0bbb84d43ad9f02626c171e5))
* fix GitHub Actions workflow for building and packaging Luxoria.App ([819ce3b](https://github.com/Luxoria-EIP/Luxoria/commit/819ce3b4adb03938a2a942da6b215bebb5e2c122))
* fix GitHub Actions workflow for detecting Q&A / SAST ([42f5068](https://github.com/Luxoria-EIP/Luxoria/commit/42f50688f443fcdbb15f7d9ceaf3034124164315))
* Fix ModuleServiceTests to include logger service in initialization ([fb2a32e](https://github.com/Luxoria-EIP/Luxoria/commit/fb2a32e862f550facf884d7b2031990cef635a1f))
* Install dotnet-coverage globally and configure OpenCover reports ([b2a116c](https://github.com/Luxoria-EIP/Luxoria/commit/b2a116c467491c13267050b8ab7f374e50fadc3e))
* Refactor project structure and ignore files ([eefc7aa](https://github.com/Luxoria-EIP/Luxoria/commit/eefc7aa1ae0009ce62ad1c75870a972d7477e100))
* Update release workflow to only build for x64 platform ([143f418](https://github.com/Luxoria-EIP/Luxoria/commit/143f418f77794666905d364eabc260d334e47e18))
* Update test command in build workflow ([a4b3959](https://github.com/Luxoria-EIP/Luxoria/commit/a4b3959dc240ac05bb09d137a205c65cc7ba58be))
* Update test command in build workflow ([f2d9170](https://github.com/Luxoria-EIP/Luxoria/commit/f2d9170ce73a84b404d5b0844aeac09db4e058a9))

# 1.0.0 (2024-11-19)


### Bug Fixes

* Update download link in README.md ([7d9047e](https://github.com/Luxoria-EIP/Luxoria/commit/7d9047ec50398daeb64848158203211e0d822f28))


### Features

* Add Auto release system (Semantic Release) ([776fa11](https://github.com/Luxoria-EIP/Luxoria/commit/776fa11b235b3fd96fc88659ed20deb88a1ebcaa))
* add base of .NET WinUI3 project ([0fd0c4a](https://github.com/Luxoria-EIP/Luxoria/commit/0fd0c4a1d686d05fb722bdf734759cdd5d462c2f))
* add C# .NET WinUI3 base ([307bfc5](https://github.com/Luxoria-EIP/Luxoria/commit/307bfc54be0dae0b34aceb414f94be0d15f93d1e))
* add GitHub Actions workflow for building and analyzing the project ([fabe2e9](https://github.com/Luxoria-EIP/Luxoria/commit/fabe2e9cdaa106f871edc1858f4c40e4633ea589))
* add gitignore and husky configuration files ([f9e0f71](https://github.com/Luxoria-EIP/Luxoria/commit/f9e0f71e57fe99bb71eace499fffec36c5ffc20e))
* Add links to download, documentation, and contribute in README.md ([b2c587e](https://github.com/Luxoria-EIP/Luxoria/commit/b2c587eab7c312b6395abbeffbbb8aef7c2fa508))
* Add Luxoria Documentation ([3bf3421](https://github.com/Luxoria-EIP/Luxoria/commit/3bf34216d38435005e96e39036477e172bd3c7a6))
* add publish profiles inside repository ([1426a90](https://github.com/Luxoria-EIP/Luxoria/commit/1426a9011aa20415ca9a5c382d8f3064fc02a49d))
* fix ci/cd Build and analyze step ([9579291](https://github.com/Luxoria-EIP/Luxoria/commit/95792917434100afb42d6129d91088ac28236963))
* fix ci/cd Build and analyze step ([198b0c5](https://github.com/Luxoria-EIP/Luxoria/commit/198b0c5abdef2ec52bed651b801f400d98d3d91e))
* fix ci/cd build part by specifying target build name ([05d9c01](https://github.com/Luxoria-EIP/Luxoria/commit/05d9c01bd790f75c0f6669cc327236d138dc95c1))
* fix download link in README.md ([f3cb3af](https://github.com/Luxoria-EIP/Luxoria/commit/f3cb3af87cd358bf2b39e5c75bdb4667be4377f2))
* fix exclude some files from SonarQube Analyze ([d86d1d0](https://github.com/Luxoria-EIP/Luxoria/commit/d86d1d006ef6da50fa1ff89a98d84b3ca93c4a7d))
* fix Luxoria README.md and add documentation ([a6f5cfd](https://github.com/Luxoria-EIP/Luxoria/commit/a6f5cfdc314bc975481c8bf7ec65f68fdd685cee))
* fix publisher profile name to "Luxoria Project" ([1df3600](https://github.com/Luxoria-EIP/Luxoria/commit/1df36006c004fd6dea0bbb104fed16072c531791))
* update SonarQube project badges URLs ([2d5a6f0](https://github.com/Luxoria-EIP/Luxoria/commit/2d5a6f0efe7210368c8d451fbade0ab15db505db))
