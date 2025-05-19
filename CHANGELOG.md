## [1.33.1](https://github.com/LuxoriaSoft/Luxoria/compare/v1.33.0...v1.33.1) (2025-05-19)


### Bug Fixes

* Add maxcpucount option to Luxoria.App build step in workflow ([24318f8](https://github.com/LuxoriaSoft/Luxoria/commit/24318f8ae64f9e3801413d5e947568673349882c))
* Add optional flag to disable Luxoria.SDK's PackOnBuild option ([170a40d](https://github.com/LuxoriaSoft/Luxoria/commit/170a40d71ca983c728015bcf27dac4909a5ded08))
* Add renaming steps for LuxImport, LuxFilter, LuxEditor, and LuxExport DLLs in build workflow ([e38ad92](https://github.com/LuxoriaSoft/Luxoria/commit/e38ad928a743c1202e8afda6fd95d22560b14dc3))
* Change Reference Paths in LuxImport csproj file ([eb5e51e](https://github.com/LuxoriaSoft/Luxoria/commit/eb5e51eb7c3f713559760841cccea395a7d96c7e))
* Cleanup unused dependencies in LuxImport ([d9843df](https://github.com/LuxoriaSoft/Luxoria/commit/d9843df3a9acbc8e9e8d0274158792f43de862c6))
* Correct artifact paths for LuxImport, LuxFilter, LuxEditor, and LuxExport in build workflow ([ba1225e](https://github.com/LuxoriaSoft/Luxoria/commit/ba1225e009637b647d54c5d18dd4614cc8d24bd4))
* Enhance platform targeting for Luxoria.App build step to include win-arm64 ([c155fae](https://github.com/LuxoriaSoft/Luxoria/commit/c155fae7663e12e078bd65a648b3c86c15d7ccc0))
* Expand platform matrix to include win-x86 and win-arm64 for build jobs ([a4a8215](https://github.com/LuxoriaSoft/Luxoria/commit/a4a821543e6262010db2f53b68e4f42837b01bc9))
* Refactor DLL renaming steps for LuxImport, LuxFilter, LuxEditor, and LuxExport to use Join-Path for improved path handling ([065e7af](https://github.com/LuxoriaSoft/Luxoria/commit/065e7af3a94399bf97e582815c3a181dd9391882))
* Replacing DLLRef by ProjectRef for both Luxoria.GModules & Luxoria.Modules ([d97f099](https://github.com/LuxoriaSoft/Luxoria/commit/d97f0999b2a2220921be3cddb76dc01b8b7d287b))
* Update artifact paths and rename steps for LuxImport, LuxFilter, LuxEditor, and LuxExport in build workflow ([1ecf85a](https://github.com/LuxoriaSoft/Luxoria/commit/1ecf85a215d68c01b68b0d12ea47b32182fa13cc))
* Update artifact paths for LuxImport, LuxFilter, LuxEditor, and LuxExport in build workflow ([991449b](https://github.com/LuxoriaSoft/Luxoria/commit/991449be68f991a8edba553c1f759b255095d932))
* Update artifact paths for LuxImport, LuxFilter, LuxEditor, and LuxExport in build workflow ([6c70b31](https://github.com/LuxoriaSoft/Luxoria/commit/6c70b31a021fdf381a778cf02c8a567857442532))
* Update build steps to use 'dotnet build' for LuxImport, LuxFilter, LuxEditor, and LuxExport, and adjust artifact paths ([23c5c8d](https://github.com/LuxoriaSoft/Luxoria/commit/23c5c8df45439ee084fcc9f0cc54147b3a2ca681))
* Update build steps to use 'dotnet publish' for LuxImport, LuxFilter, LuxEditor, and LuxExport ([c055bc0](https://github.com/LuxoriaSoft/Luxoria/commit/c055bc05270cddd3777e3d2386e902f06cee6c72))
* Update build steps to use 'dotnet publish' for LuxImport, LuxFilter, LuxEditor, and LuxExport, and adjust artifact paths ([fda3076](https://github.com/LuxoriaSoft/Luxoria/commit/fda30762765ace7f31018d517c34d4addd45d559))
* Update build workflow to enhance module upload steps and improve comments for clarity ([6af42f1](https://github.com/LuxoriaSoft/Luxoria/commit/6af42f11b36c1a6bba07f4578b7a0b2971dfd6ae))

# [1.33.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.32.1...v1.33.0) (2025-05-17)


### Bug Fixes

* Improve file upload validation in AuthController and update logout method in Dashboard ([7a9ee7c](https://github.com/LuxoriaSoft/Luxoria/commit/7a9ee7c07fee2ffb5eb576c81be8c2cd3d513412))
* Update API_URL in config files for consistency and environment variable support ([42db9f3](https://github.com/LuxoriaSoft/Luxoria/commit/42db9f319ee0a2e415bc4ba624fd1bf0ca3738e8))


### Features

* add real-time chat functionality with username support ([17accc1](https://github.com/LuxoriaSoft/Luxoria/commit/17accc1378d5cf28cf2c3d5d812c02b858dcdf41))
* Add user avatar upload and retrieval functionality ([e308b63](https://github.com/LuxoriaSoft/Luxoria/commit/e308b637e7d87eb98d128951d214d3c730be213e))
* Enhance JWT token generation to include user email and secure collection retrieval with authorization ([6bc0db2](https://github.com/LuxoriaSoft/Luxoria/commit/6bc0db27caf152dea90f38893f9abed35ed3c419))
* Implement SignalR chat functionality and add allowed email management for collections ([75c1da4](https://github.com/LuxoriaSoft/Luxoria/commit/75c1da40b6f29b77b155653a87a95df69a7566a8))
* Improve avatar handling and user feedback in registration and collection management ([5bdf4c3](https://github.com/LuxoriaSoft/Luxoria/commit/5bdf4c3f45228c98210e3434cb12fc9490096545))
* update Vite version and add CollectionDetail view ([70b3732](https://github.com/LuxoriaSoft/Luxoria/commit/70b37323d44b51d70d844d5e818000b5ce56b262))

## [1.32.1](https://github.com/LuxoriaSoft/Luxoria/compare/v1.32.0...v1.32.1) (2025-05-15)


### Bug Fixes

* Move dependabot to .github folder, from .github/workflows folder ([d543680](https://github.com/LuxoriaSoft/Luxoria/commit/d5436800500ce7e68cca651bb2dc0f156929bb06))

# [1.32.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.31.1...v1.32.0) (2025-05-11)


### Bug Fixes

* Add API_URL environment variable to luxstudio container ([9de6552](https://github.com/LuxoriaSoft/Luxoria/commit/9de6552995355c7c2fec0ed56ace206cdbaefcc3))
* Add context to build-and-push job in CircleCI configuration ([c67603f](https://github.com/LuxoriaSoft/Luxoria/commit/c67603ff8fe34c75b493432e1d79b3ce8c6af85b))
* Add debug step to check DockerHub credentials before login ([a0f628d](https://github.com/LuxoriaSoft/Luxoria/commit/a0f628d92f9b02c6008bfa725615cd4fb4d19015))
* Correct build context paths for luxapi and luxstudio in Azure Pipelines configuration ([91f90fa](https://github.com/LuxoriaSoft/Luxoria/commit/91f90fadacb2c9c35e41cbef43b76c12c92753fb))
* Move context declaration for build-and-push job to workflows section ([b086199](https://github.com/LuxoriaSoft/Luxoria/commit/b0861998684a58ede96b56f3a88beba280e319c6))
* Rename job from build-images to build-and-push and update logging steps ([af1d20b](https://github.com/LuxoriaSoft/Luxoria/commit/af1d20b080ea27cca771487bbc9cc68218fa791f))
* Rename job from build-local to build-images and update image tag logging ([621bb85](https://github.com/LuxoriaSoft/Luxoria/commit/621bb85396e7622ea686c28845a8c84569365549))
* Restore imageTag variable format for tagging in Azure Pipelines ([214b2d1](https://github.com/LuxoriaSoft/Luxoria/commit/214b2d1186b2194773222b1df8aee2af115a884f))
* Update display name for BuildAndPush stage in Azure Pipelines configuration ([0e9447c](https://github.com/LuxoriaSoft/Luxoria/commit/0e9447c30c2c45b3451f10128dc800e00e5b784d))
* Update display name for BuildAndPush stage in Azure Pipelines configuration ([0af18c8](https://github.com/LuxoriaSoft/Luxoria/commit/0af18c8a8bb34d54e9e2fb91ccd3136ef2ce7f95))
* Update Docker login command for consistency in username flag usage ([e3537e4](https://github.com/LuxoriaSoft/Luxoria/commit/e3537e445aa3357ac098793d1429cc82e31d616a))
* Update Dockerfile paths for luxapi and luxstudio in Azure Pipelines configuration ([2d896d4](https://github.com/LuxoriaSoft/Luxoria/commit/2d896d45f293a34a5069fe60ef9792e295882712))
* Update FrontEnd__URI and API_URL environment variables for luxapi and luxstudio deployments ([673eb81](https://github.com/LuxoriaSoft/Luxoria/commit/673eb81f3afb4bc5027d2750e6473ac12d503d2e))
* Update imageTag variable format for consistency in Azure Pipelines ([e8aea4c](https://github.com/LuxoriaSoft/Luxoria/commit/e8aea4c5a7fd3d7ae1236cce5adf9b7da6c95a84))
* Update imageTag variable format for consistency in Azure Pipelines ([6052948](https://github.com/LuxoriaSoft/Luxoria/commit/6052948c5fa3b408301a6c42f1e5e7907c416cb3))
* Update imageTag variable to use Build.BuildId for tagging in Azure Pipelines ([c70c2ba](https://github.com/LuxoriaSoft/Luxoria/commit/c70c2ba6f9583696ab62673f067fa9c58575d664))
* Update imageTag variable to use Build.BuildNumber for consistency in Azure Pipelines ([646d314](https://github.com/LuxoriaSoft/Luxoria/commit/646d314dfc903f3c3d321a83a507d8d30e9b3104))
* Update imageTag variable to use Build.SourceVersion for tagging in Azure Pipelines ([1f8aa5d](https://github.com/LuxoriaSoft/Luxoria/commit/1f8aa5de8623376cb7b8e176ac9c9a6b037dcc6c))
* Update imageTag variable to use conditional logic for tagging in Azure Pipelines ([31393a4](https://github.com/LuxoriaSoft/Luxoria/commit/31393a4aecd10c7f3377499ba8ff92841bc18358))
* Update ingress configuration to use luxstudio-service instead of pluto-service ([bfa3c60](https://github.com/LuxoriaSoft/Luxoria/commit/bfa3c606ad1425403244d3dbdb60ee66c3d357f2))
* Update ingress configurations for LuxStudio in Pluto and Saturn namespaces to include TLS settings and cert-manager annotations ([bfdd9b0](https://github.com/LuxoriaSoft/Luxoria/commit/bfdd9b06c8c72011e7760a3d80eaa04f133ca057))
* Update kustomization.yaml to use patches with target for ingress configuration ([93a91d5](https://github.com/LuxoriaSoft/Luxoria/commit/93a91d54c23e6a29c492796408627da6ed19fdb8))
* Update secretName for TLS configuration in luxstudio-ingress ([bbe93a1](https://github.com/LuxoriaSoft/Luxoria/commit/bbe93a1d5598d9f52a0351fac8cf886f5c63dbc9))
* Update secretName for TLS configuration in luxstudio-ingress ([fca2963](https://github.com/LuxoriaSoft/Luxoria/commit/fca29637f339037a76df071c7a36cb5a05360ff1))


### Features

* Add ArgoCD application configuration for LuxStudio in Pluto namespace ([7107523](https://github.com/LuxoriaSoft/Luxoria/commit/71075238e3957556548e4080b4068d31f681c8ac))
* Add CircleCI configuration file for CI/CD setup ([a3dcec3](https://github.com/LuxoriaSoft/Luxoria/commit/a3dcec36dbeecac37f3df4347acd1a01267b53b9))
* Add CircleCI configuration for building LuxAPI and LuxStudio Docker images ([6417b1f](https://github.com/LuxoriaSoft/Luxoria/commit/6417b1fd0b9a59b7bf6f6c5fb6bb33ad14213c0e))
* Add Docker and Kubernetes configurations for LuxStudio deployment ([1edf148](https://github.com/LuxoriaSoft/Luxoria/commit/1edf1481000be5aebd972e7dbb9a3b68c77826e6))
* Add Docker support with Dockerfile, entrypoint script, and .dockerignore; include config.js for runtime API URL replacement ([9a361de](https://github.com/LuxoriaSoft/Luxoria/commit/9a361deafb2c0e19b3c1ddbd4301cdde63440ee0))
* Add luxportal service to Docker Compose; update routing and error handling in Login component ([31216ba](https://github.com/LuxoriaSoft/Luxoria/commit/31216ba92e8f79126900bbdd487a8a33e50dcd34))
* Add Minio configuration and implement collection-related models with relationships ([524f688](https://github.com/LuxoriaSoft/Luxoria/commit/524f688c92a42893a920b2d9921e24ad1bc300fa))
* Add starter Azure Pipelines configuration ([92e8416](https://github.com/LuxoriaSoft/Luxoria/commit/92e841632291b730f7820ec9fc658aa8aeb127ce))
* Enhance SystemController to include database connectivity checks and update Docker Compose for improved service configuration ([9785604](https://github.com/LuxoriaSoft/Luxoria/commit/9785604ba6c7292c28e2fec6d27be963754198df))
* Implement dynamic image tagging in CircleCI configuration ([f9020de](https://github.com/LuxoriaSoft/Luxoria/commit/f9020de1c4330c97311295c2ab5a61dfda4c8e65))
* Remove old ingress and configmap configurations; add new deployment and service for LuxStudio in Pluto and Saturn namespaces ([67c198f](https://github.com/LuxoriaSoft/Luxoria/commit/67c198f2cc26c15cdef5b7700003ec2140d46f29))
* Update Azure Pipelines configuration for Docker builds; remove unused adminer service from Docker Compose ([589580c](https://github.com/LuxoriaSoft/Luxoria/commit/589580c962f7a038be305ae20e88914e5f4eb2b9))
* Update Dockerfile for specific Node and Nginx versions; add nginx.conf for server configuration; refactor auth services to use dynamic API URLs from appConfig ([5dfc019](https://github.com/LuxoriaSoft/Luxoria/commit/5dfc01938138888d394e6a151748972b856b3212))

## [1.31.1](https://github.com/LuxoriaSoft/Luxoria/compare/v1.31.0...v1.31.1) (2025-04-08)


### Bug Fixes

* Add Unit tests for LuxExport ([a868c40](https://github.com/LuxoriaSoft/Luxoria/commit/a868c40d95253a87c173d3088d180526e3e71eaf))
* Clean unused code ([0935f7c](https://github.com/LuxoriaSoft/Luxoria/commit/0935f7c3731dbac49bc6a8e936ea5fb768f27741))
* Commentaries in code ([b778fd2](https://github.com/LuxoriaSoft/Luxoria/commit/b778fd25ddf4cf7ddc6d055b3d212889554dc4b0))
* Documentation for lux-export ([c2e9292](https://github.com/LuxoriaSoft/Luxoria/commit/c2e929270ec1ad90c390b85100c98254effdb53f))
* Export File Naming backend ([f1ace86](https://github.com/LuxoriaSoft/Luxoria/commit/f1ace86486026346387d35ee33ef533f95d72662))
* Export location back-end ([1a9fc8d](https://github.com/LuxoriaSoft/Luxoria/commit/1a9fc8da9e2005722cce6f2db329dc2084300513))
* Export location back-end ([7fce05d](https://github.com/LuxoriaSoft/Luxoria/commit/7fce05d100d1e933e9b9581841dd826d5ed951c8))
* Export Pop Up with progress bar and previsualization ([af849b8](https://github.com/LuxoriaSoft/Luxoria/commit/af849b81be165ce0d39d523030ea6cfb7363279c))
* File Path for assets + color profile  + removing broken file format ([845c5ba](https://github.com/LuxoriaSoft/Luxoria/commit/845c5ba9a97bdd23890059100f853f67935a0387))
* File Picker in content dialog + content dialog size ([a658ef3](https://github.com/LuxoriaSoft/Luxoria/commit/a658ef3b2301a20db9110e5351dfb65fa7b140ce))
* Initialisation of the modal using LMGUI ([32b1eb7](https://github.com/LuxoriaSoft/Luxoria/commit/32b1eb7b387a2dd1114cd87ed10bf4fe6de1acee))
* Lux Export Interface ([769e797](https://github.com/LuxoriaSoft/Luxoria/commit/769e79708af2e18f773d72ee5e24f697f36eeb91))
* Lux Export Interface ([e8f7c28](https://github.com/LuxoriaSoft/Luxoria/commit/e8f7c288f4580975eff0be44c73a3f778ca7f8be))
* LuxExport refactor from window to modal ([e658762](https://github.com/LuxoriaSoft/Luxoria/commit/e6587623537d670c47dc7ba67ea729051eb7f9ae))
* Update counter value on multiple photo export ([672312f](https://github.com/LuxoriaSoft/Luxoria/commit/672312f0e705514b1b41451d9d29601e0e83f7de))
* Using window instead of modal because of WinUi restrictions ([9a8f279](https://github.com/LuxoriaSoft/Luxoria/commit/9a8f2794063caea6f723e6f221a4a4ff63f41bb9))

# [1.31.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.30.0...v1.31.0) (2025-03-07)


### Bug Fixes

* Simplify LoadWindowCaption by using local variable 'path' ([c975768](https://github.com/LuxoriaSoft/Luxoria/commit/c9757680610f2062da186592c19839e6a14bb5f9))


### Features

* Add ApplicationIcon 'Luxoria_icon.ico' + Add Luxoria official logo on SplashScreen ([b0142b5](https://github.com/LuxoriaSoft/Luxoria/commit/b0142b57c9a5a177af0c6d5d35c7aa860ca8f188))
* Create LoadWindowCaption method to load an ico for window caption and taskbar ([12e9b56](https://github.com/LuxoriaSoft/Luxoria/commit/12e9b568113a0784b00b512f18ab530cf9aac191))

# [1.30.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.29.1...v1.30.0) (2025-03-06)


### Bug Fixes

* Add Filters (ReadOnlyDictionary?) in ImageData and fix some tests ([cb66737](https://github.com/LuxoriaSoft/Luxoria/commit/cb66737678a99cb5e856876136ecc7df5df47c24))
* Bring clarification and fix nullable variables object? sender => object sender ([9b2128c](https://github.com/LuxoriaSoft/Luxoria/commit/9b2128c37e098f7e1b1845471f456707b3abe2d8))
* Changes Score alignment from Center to Right ([458d0dd](https://github.com/LuxoriaSoft/Luxoria/commit/458d0dde3b1bb5f5e457d4bf2dfbb40ddd42beef))
* Remove gray border to the ItemList on FilterView ([d2b388d](https://github.com/LuxoriaSoft/Luxoria/commit/d2b388ddd75e352a16229c5606dbf364528235bf))
* Remove total weight (1.0 => 100) on the PipelineService ([13c4db9](https://github.com/LuxoriaSoft/Luxoria/commit/13c4db9d993124526255d450429e8542e3c95976))
* Replace Dictionary by ImmutableDictionary (Catalog) in FilterService ([0ac9ee7](https://github.com/LuxoriaSoft/Luxoria/commit/0ac9ee795113fdded8f89ac6855f136b4072bffb))
* Upgrade Luxoria.Algorithm.BrisqueScore from 3.0.2.4100 to 3.0.3.4100 ([5633377](https://github.com/LuxoriaSoft/Luxoria/commit/563337755d4e43f186aa60e518f8a8b406037123))


### Features

* Add base of filters selection with params (weight) ([a277aee](https://github.com/LuxoriaSoft/Luxoria/commit/a277aeeee23891d835d1dc31838809a863693131))
* Add FilterCatalog Event to fetch the entire available filters (ReadyToUse) ([c8850ef](https://github.com/LuxoriaSoft/Luxoria/commit/c8850ef730b49e7d3b5ed78720ef3b8127e07e3d))
* Add StatusView foundation for LuxFilter UI (ListView, ViewModel, ...) ([9235ed6](https://github.com/LuxoriaSoft/Luxoria/commit/9235ed6e528c86ddd2767579557b8a9c6dd1f5cb))
* Add the Status (Logger) UI on LuxFilter 3rd part ([c4d2631](https://github.com/LuxoriaSoft/Luxoria/commit/c4d2631f081e2f6ee53a908cb9d285038bb6fe81))
* Create Filtering Status View (LuxFilter.Views.StatusView) ([535adb0](https://github.com/LuxoriaSoft/Luxoria/commit/535adb0a387331ad22633b0b95efec67b8b14ba3))

## [1.29.1](https://github.com/LuxoriaSoft/Luxoria/compare/v1.29.0...v1.29.1) (2025-02-18)


### Bug Fixes

* Change Compute method usage in LuxFilter.Tests cases ([c23a1b5](https://github.com/LuxoriaSoft/Luxoria/commit/c23a1b5a73c86c5ae55d77ec2afec4d4091888f9))
* Change type of return of Pipeline[Compute] method ([ac8910a](https://github.com/LuxoriaSoft/Luxoria/commit/ac8910acae65315b8a06df2ae147e2455f0df353))
* Static path to assets folder on LuxFilter.TestMain ([18e3ec3](https://github.com/LuxoriaSoft/Luxoria/commit/18e3ec3b3e6fc6251ef952e4a5eea9d921801d42))

# [1.29.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.28.0...v1.29.0) (2025-02-17)


### Features

* Update Dependabot configuration to target the develop branch for all package ecosystems ([cd4e29a](https://github.com/LuxoriaSoft/Luxoria/commit/cd4e29a0d6fb33c2b3f17aed2335ae1870f4da9b))

# [1.28.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.27.0...v1.28.0) (2025-02-17)


### Features

* Add Dependabot configuration for automated dependency updates ([2917a6a](https://github.com/LuxoriaSoft/Luxoria/commit/2917a6a1923dfcf21da9c2af354772ebb676585e))
* Update Dependabot configuration to include commit message prefixes and scope ([5cbf086](https://github.com/LuxoriaSoft/Luxoria/commit/5cbf08695faaad833bfa63fc44364835e918c0c3))

# [1.27.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.26.0...v1.27.0) (2025-02-14)


### Bug Fixes

* Build system of LuxImport for Unpackaged Luxoria version ([8e41e60](https://github.com/LuxoriaSoft/Luxoria/commit/8e41e60bd82d326857fb21a74c3988811ffcf065))
* Change view from ImportView when recentbutton is clicked ([00d0c14](https://github.com/LuxoriaSoft/Luxoria/commit/00d0c14d8bb2c0cc73fc909112e4e3d3f37bfc49))
* Check if CollectionPath already initialized, if yes, goto Indexing View ([fbdc533](https://github.com/LuxoriaSoft/Luxoria/commit/fbdc53322a7d2b4f27315d181b924d5091e35ea5))
* Unassign dialog.Content after dialog closed ([6595d5f](https://github.com/LuxoriaSoft/Luxoria/commit/6595d5ff4cd440e5efc004065ad1681d6e6761fa))


### Features

* Add a progress bar on MainImportView (X/3 steps) ([dc97b96](https://github.com/LuxoriaSoft/Luxoria/commit/dc97b9618a266528746c4ffec4b7987f6c361c72))
* Add base of Importation modal (ImportView) ([1ec97e6](https://github.com/LuxoriaSoft/Luxoria/commit/1ec97e64c0077bc918eab735d63419e612904ea3))
* Add basic layer for ImportView view ([7163528](https://github.com/LuxoriaSoft/Luxoria/commit/716352801ead84aa44a9728f4199b2b361074349))
* Add first importation step dialog (1/3) ([f48c0b3](https://github.com/LuxoriaSoft/Luxoria/commit/f48c0b396c3c4f167aed51f22a0b182c5f6d9e04))
* Add foundation for Importation views part 1/3, 2/3, 3/3 ([7b665bc](https://github.com/LuxoriaSoft/Luxoria/commit/7b665bcff7551c108c0648021379792fb503be6c))
* Add RICollectionRepository system to handle the Recents Imported Collection list ([9c176fc](https://github.com/LuxoriaSoft/Luxoria/commit/9c176fc1193255e9d8f8c5d13b09fed874964815))
* Add the Indexication Log viewer on Indexication view (3/3) ([e47182c](https://github.com/LuxoriaSoft/Luxoria/commit/e47182c45a82eb13875c65d1adc1786e3f65c845))
* Create new buildsystem for LuxImport and Creation of LuxImport 1.0.2 ([4df8a00](https://github.com/LuxoriaSoft/Luxoria/commit/4df8a00c8f6d3abb5f9cd1ebfd4379ff42ba742e))
* Create RequestWindowHandleEvent event to retreive Main Window Ptr ([f5a34f4](https://github.com/LuxoriaSoft/Luxoria/commit/f5a34f4ad66bc4425f290ac741afb89ee9267239))
* Include Luxoria.SDK nuget instead of Luxoria.SDK.dll ([a6cee2f](https://github.com/LuxoriaSoft/Luxoria/commit/a6cee2f5e4e879bf763ecc7b4e8955d9ea85d996))
* Update components on Properties view ([901fc21](https://github.com/LuxoriaSoft/Luxoria/commit/901fc2122ce19aafd7e7fb483e0733c7d4b5950c))

# [1.26.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.25.0...v1.26.0) (2025-02-13)


### Bug Fixes

* Rename artifact using matrix.platform to Luxoria.App.xARCH ([902a93e](https://github.com/LuxoriaSoft/Luxoria/commit/902a93e6b204bb987fb671d310e03369047c59e3))
* Update artifact upload name to include configuration and platform ([f00babb](https://github.com/LuxoriaSoft/Luxoria/commit/f00babb92d668ba975552404aefb040890545a6c))


### Features

* Add feat/unpackaged-version branch to release workflow ([9aed874](https://github.com/LuxoriaSoft/Luxoria/commit/9aed874ac784438dfe37e7ecd09a9da35f4baa23))
* Add upload step for Luxoria.App in release workflow ([269a2cb](https://github.com/LuxoriaSoft/Luxoria/commit/269a2cbc03aa41f3aecdf176460653ac29c41ae9))

# [1.25.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.24.0...v1.25.0) (2025-02-13)


### Bug Fixes

* Code cleanup using visual studio profile 1 ([c0a4697](https://github.com/LuxoriaSoft/Luxoria/commit/c0a4697c56746879636390a9dc29d9747e43bcbd))
* Collection Explorer carrousel ([13e76fd](https://github.com/LuxoriaSoft/Luxoria/commit/13e76fd0f863b23088abf7601e2a57c8b1977d7e))
* Collection Explorer carrousel ([d43fe66](https://github.com/LuxoriaSoft/Luxoria/commit/d43fe66220715a2cc1d32d198390025df79d6fba))
* Display flyout menu on click ([15b9f8b](https://github.com/LuxoriaSoft/Luxoria/commit/15b9f8bcda3d8721fe767e5c9a3bb694e21d5f0c))
* SkiaSharp library ([5ebd5cf](https://github.com/LuxoriaSoft/Luxoria/commit/5ebd5cfff653573423b6322e37e504fea0dc5c6a))
* Sliders in Editor pannel ([3853302](https://github.com/LuxoriaSoft/Luxoria/commit/3853302db5e4b7061be7098f6dd82b80cbfdd9bc))


### Features

* Backend behind Graphical Test Module XAML ([bc1162f](https://github.com/LuxoriaSoft/Luxoria/commit/bc1162fac220744f8c2621a030241e92ac46ac5c))
* EXIFS metada pannel ([32f6a1b](https://github.com/LuxoriaSoft/Luxoria/commit/32f6a1ba9523d106f739c4c9d297abe1657b445b))
* IModuleUI Integration ([699b91e](https://github.com/LuxoriaSoft/Luxoria/commit/699b91eb92a0956839a50f3bd46927c99f04b3ee))
* Load component function ([203ed08](https://github.com/LuxoriaSoft/Luxoria/commit/203ed081208b3fa794580ab148c83e528448c4b3))
* Module in panel integration ([0a503e9](https://github.com/LuxoriaSoft/Luxoria/commit/0a503e977f20357d4233380a5e591d32c0e15037))
* Upgrade Modules SDK to include Windows SDK ([3d78906](https://github.com/LuxoriaSoft/Luxoria/commit/3d78906233d32a1078e3af6c469ba8f701acb1a0))

# [1.24.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.23.0...v1.24.0) (2025-02-05)


### Bug Fixes

* Correct formatting and style in HelloWorld.vue ([1dc7292](https://github.com/LuxoriaSoft/Luxoria/commit/1dc7292127f44c9da6b0c9dfd3f4a40a32cba18d))


### Features

* Add base of SSO ([02a1844](https://github.com/LuxoriaSoft/Luxoria/commit/02a184423d5f077a19f1afc033003242ddf186f1))
* Add Dashboard component with authentication guard and logout functionality ([7e6c316](https://github.com/LuxoriaSoft/Luxoria/commit/7e6c31661d462e52504291814ac125119e447ec3))
* Add Docker configuration and initialization scripts for Luxoria database ([c99de0d](https://github.com/LuxoriaSoft/Luxoria/commit/c99de0d6b64fb87048cfdbed843382ce927a3047))
* Add DTOs for user login, registration, and refresh token requests; implement TokenService for secure token generation and comment in code ([110ce25](https://github.com/LuxoriaSoft/Luxoria/commit/110ce259fe48ba3df76bdc100c480385c566b7ab))
* Add EF connection to LuxAPI ([8a9e395](https://github.com/LuxoriaSoft/Luxoria/commit/8a9e3957372087028284b8380387dd5c9bb14d3e))
* Add GitHub Actions workflow for LuxAPI build process ([981ee30](https://github.com/LuxoriaSoft/Luxoria/commit/981ee300872e40fddc10f3cbb8417f6218aecc56))
* Add initial implementation of LuxAPI ([b552d7c](https://github.com/LuxoriaSoft/Luxoria/commit/b552d7cdb54f646ec5ea69f118306bffb0706c33))
* Add TailwindCSS + DaisyUI on LuxStudio Portal ([2b54396](https://github.com/LuxoriaSoft/Luxoria/commit/2b5439693a01bafd24b15ee684850dcf52621089))
* Add UserId to AuthorizationCode model and implement SSO authorization view ([bf5d767](https://github.com/LuxoriaSoft/Luxoria/commit/bf5d767ae6c0ab1756435d1eeb2298c527262e4f))
* Enhance JWT token generation with user ID and add WhoAmI endpoint for user info retrieval ([1579f96](https://github.com/LuxoriaSoft/Luxoria/commit/1579f96d2be1fff4c00b94a8cc1c5895df7fbfe8))
* Implement authentication with login and registration views, add router, and configure CORS ([3d539c9](https://github.com/LuxoriaSoft/Luxoria/commit/3d539c96c67d1052ddd507668898af1173c6b59e))
* Implement JWT authentication and configure Swagger for API security ([799c33b](https://github.com/LuxoriaSoft/Luxoria/commit/799c33bb9a19a695c7e1ae838a0abd6a14d17f8b))
* Increase token field size to TEXT for AccessToken and RefreshToken in Token model ([9a1e2f3](https://github.com/LuxoriaSoft/Luxoria/commit/9a1e2f3a1d7681356d927eea639539076d6d3b33))
* Initialize Vue 3 + TypeScript + Vite project with basic structure and configuration ([909aace](https://github.com/LuxoriaSoft/Luxoria/commit/909aacea9840b210ed99978c44fc2f8130ad807c))
* Refactor Token model to use UserId and add RefreshToken model with related functionality ([96e9634](https://github.com/LuxoriaSoft/Luxoria/commit/96e9634dc6c0240c8412eaaf58830c5ccb4fc594))
* Update models to use required properties for better validation; adjust nullable types where necessary ([83f82bf](https://github.com/LuxoriaSoft/Luxoria/commit/83f82bf2c09fbc2a28bd6fa290bd4d4c6b9066a5))
* Update package dependencies and add Vue type definitions; enhance router token handling ([73350d5](https://github.com/LuxoriaSoft/Luxoria/commit/73350d5fe46cd602f304ee92e03950589f8ff18e))

# [1.23.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.22.0...v1.23.0) (2025-02-05)


### Features

* Add metrics on LuxFilter & Create Luxoria.SDK nuget ([c42c718](https://github.com/LuxoriaSoft/Luxoria/commit/c42c718760bf0f1f73fb780acc4ea477adb8ae50))

# [1.22.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.21.0...v1.22.0) (2025-02-05)


### Features

* Add EXIF data on ImageData and Include RAW/ARW file extension inside Filter ([8d1b390](https://github.com/LuxoriaSoft/Luxoria/commit/8d1b3907a12d2e61480cd3d82fe79a5694d074cf))
* Enhance EXIF orientation handling in ImageData processing ([64602cb](https://github.com/LuxoriaSoft/Luxoria/commit/64602cbee6474f0438028b727830eaf78ea8ac2a))
* Refactor EXIF orientation handling in ImageData ([e9098b8](https://github.com/LuxoriaSoft/Luxoria/commit/e9098b89c0c6bd97c344430a00dd0528a28e0721))

# [1.21.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.20.0...v1.21.0) (2025-02-03)


### Features

* Adding BrisqueInterop (C++) to BrisqueAlgo (IFilter .NET) ([a0771b7](https://github.com/LuxoriaSoft/Luxoria/commit/a0771b793119bddb581e742daf079b0cf5ff9613))

# [1.20.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.19.0...v1.20.0) (2025-02-03)


### Features

* Add additional diagnostics and exporters to ImportService benchmarking ([5e5ad07](https://github.com/LuxoriaSoft/Luxoria/commit/5e5ad07e80cc01df43a0ca2aed099b6f9e2b77fc))
* Add benchmarks & metrics on ImportService ([280581e](https://github.com/LuxoriaSoft/Luxoria/commit/280581eb9d141a159d1215aa76b2fdbd7eb5277d))
* Enhance benchmarking for ImportService with additional diagnostics and categorization ([ca45740](https://github.com/LuxoriaSoft/Luxoria/commit/ca457407153167d2e3643a3b0c35d701487fbac6))
* Update ImportServiceBenchmark to support multiple dataset paths ([ab8a351](https://github.com/LuxoriaSoft/Luxoria/commit/ab8a351bf13cfe2280ebaa900348ec508cf61214))

# [1.19.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.18.1...v1.19.0) (2025-01-31)


### Bug Fixes

* Remove Exception and Add explanations in LuxFilter.cs file ([bab93d4](https://github.com/LuxoriaSoft/Luxoria/commit/bab93d4c2e2119851361e3a156e727480a6655a3))


### Features

* Add tests for all services & repositories inside LuxImport ([4bca6b0](https://github.com/LuxoriaSoft/Luxoria/commit/4bca6b03ca6f0f31d991e08d975178ca35800c1c))

## [1.18.1](https://github.com/LuxoriaSoft/Luxoria/compare/v1.18.0...v1.18.1) (2025-01-30)


### Bug Fixes

* Correct typo in dotnet test command for LuxFilter module in build workflow ([ea3d510](https://github.com/LuxoriaSoft/Luxoria/commit/ea3d51022b2a9a2d7235679cfe472f914b05cf9b))

# [1.18.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.17.0...v1.18.0) (2025-01-30)


### Features

* Add unit tests for LuxImport module and update build workflow for coverage reports ([672dcb1](https://github.com/LuxoriaSoft/Luxoria/commit/672dcb10472f584550933fde204e012f65175ef6))

# [1.17.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.16.0...v1.17.0) (2025-01-30)


### Bug Fixes

* Remove unused lib on LuxFilter ([053216e](https://github.com/LuxoriaSoft/Luxoria/commit/053216e85322c7bcfb15ff6af7f465915a1401e1))


### Features

* Add all test cases for LuxFilter ([809085c](https://github.com/LuxoriaSoft/Luxoria/commit/809085cb9a05b8d49625ba75e745b2991561d338))
* Add brisque_impl_netlib submodule for enhanced BRISQUE implementation ([9c4f74b](https://github.com/LuxoriaSoft/Luxoria/commit/9c4f74b7b75ab97c9fe978fd898f483311fec3d5))
* Add OpenCV & OpenCV-contrib libs for C++ ([55c837b](https://github.com/LuxoriaSoft/Luxoria/commit/55c837b9249b9856a47a06ec8f8285f0ae3f005c))
* Add unit tests for ImageProcessing and SharpnessAlgo ([41a4f9a](https://github.com/LuxoriaSoft/Luxoria/commit/41a4f9acbfe43abcf31c923a88ccd8d66ea09e54))
* Encapsulate BrisqueAlgorithm inside BrisqueAlgorithm.hpp ([ad8272c](https://github.com/LuxoriaSoft/Luxoria/commit/ad8272c316d962c18506dbd9cc6983d74f35b649))
* Update build workflow to include LuxFilter and merge coverage reports ([efaa01d](https://github.com/LuxoriaSoft/Luxoria/commit/efaa01d2c83dd7973e12d84507edc5eb8172e72a))
* Update CMake configuration and enhance BRISQUE implementation with error handling ([3fa70a2](https://github.com/LuxoriaSoft/Luxoria/commit/3fa70a2d83ad0bd8556fbd53b67cde4d4779b6e2))

# [1.16.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.15.0...v1.16.0) (2025-01-18)


### Bug Fixes

* Disable PublishReadyToRun during application build in LDA workflow ([c14bc04](https://github.com/LuxoriaSoft/Luxoria/commit/c14bc04b142b0a265d1ff35e0169fb21c4a6b174))
* Remove publish step from LDA build workflow ([4665b5b](https://github.com/LuxoriaSoft/Luxoria/commit/4665b5b53577c44b4985a68b62b2e2f8dbee452e))
* Remove redundant environment variable declaration in build workflow ([8d8d634](https://github.com/LuxoriaSoft/Luxoria/commit/8d8d634eac0d3b3d9c23ea6dd2e9adb9b493a9e9))
* Remove runtime identifier from build command in LDA workflow ([935b12d](https://github.com/LuxoriaSoft/Luxoria/commit/935b12db7a58689df6b8bdf85d619ca3c0d19474))
* Set working directory for build and publish steps in LDA workflow ([7c1d3c5](https://github.com/LuxoriaSoft/Luxoria/commit/7c1d3c5e13a6bd7bd9cc7c9e53af051c5207e3cc))
* Specify runtime identifier for LDA project build in workflow ([2dd9a1a](https://github.com/LuxoriaSoft/Luxoria/commit/2dd9a1a6e4ebfd7856480c8cbd27880e7cd639cb))


### Features

* Add build and release workflows for Luxoria Desktop App ([091bc25](https://github.com/LuxoriaSoft/Luxoria/commit/091bc25db4b9918f570118a3a5f47e323bed59c4))

# [1.15.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.14.0...v1.15.0) (2025-01-15)


### Bug Fixes

* Add null handlers to events to avoid null refs ([30e5889](https://github.com/LuxoriaSoft/Luxoria/commit/30e58897cee63d098cb4c08ffba7f63609d64aa2))
* AddAlgorithm method returns itself ([d123bd0](https://github.com/LuxoriaSoft/Luxoria/commit/d123bd0d8180f30e1c22f0bec2267764224d94ec))
* Removing SKBitmapWithSize, simplifying the Compute method ([5d6480a](https://github.com/LuxoriaSoft/Luxoria/commit/5d6480a8b0ce49949c22fd4ce83bc330371837c2))


### Features

* Add base of Brisque algorithm in LuxFilter ([2c9ac2f](https://github.com/LuxoriaSoft/Luxoria/commit/2c9ac2fc582b2ddd08d891abd1843798480d1412))
* Add event handling and unique IDs to pipeline processing ([b1d12d8](https://github.com/LuxoriaSoft/Luxoria/commit/b1d12d83306929700998c979ced3990e686901b1))

# [1.14.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.13.0...v1.14.0) (2025-01-13)


### Bug Fixes

* Load Assemblies inside Luxoria.SDK.dll ([12c4c7f](https://github.com/LuxoriaSoft/Luxoria/commit/12c4c7f903f69b982f79c93aebab799ee42a7210))


### Features

* Add sentry to Luxoria.SDK dll ([63aa028](https://github.com/LuxoriaSoft/Luxoria/commit/63aa0286afa6d0160c06c3949ee1a82cfbc206c2))

# [1.13.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.12.0...v1.13.0) (2025-01-08)


### Features

* Add sentry to project in LoggerService ([5469dcb](https://github.com/LuxoriaSoft/Luxoria/commit/5469dcb54ecc761d48f67e3d13e26b02674cbbb9))

# [1.12.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.11.1...v1.12.0) (2025-01-08)


### Bug Fixes

* Add explanations inside PipelineService file & remove unused libs ([2d5a25c](https://github.com/LuxoriaSoft/Luxoria/commit/2d5a25cb668f52909567567d4ec5469cbf3904db))
* Add multi-thread process on compute function ([c9e2d1f](https://github.com/LuxoriaSoft/Luxoria/commit/c9e2d1f840d57aa4fa6abd111e13b460eea23816))
* Compute function now takes a collection of bitmaps ([1fe57b5](https://github.com/LuxoriaSoft/Luxoria/commit/1fe57b503211df4dbee8d5bc7ff55be03f610e18))
* ComputeVariance function and improve code readability ([001b18f](https://github.com/LuxoriaSoft/Luxoria/commit/001b18fd70d8ae5d92fadb83ac11b5a6ef74c529))
* IPipeline.Compute return fscore as double instead of void ([b21f8d4](https://github.com/LuxoriaSoft/Luxoria/commit/b21f8d45c21a4bc2e267a2154f243d5d34ce3d68))
* Make ApplyLaplacianKernel function as static ([8e3b2ec](https://github.com/LuxoriaSoft/Luxoria/commit/8e3b2ec1398f8855f77b3a95d48e62fe6fa40d36))
* Move BitmapWithSize model to its unique file & remove unused model ([3df6b1c](https://github.com/LuxoriaSoft/Luxoria/commit/3df6b1ca942a0cfa1ba879eb45d1265ce9044ab1))
* Use Multi-Thread in pipeline computation ([e384533](https://github.com/LuxoriaSoft/Luxoria/commit/e384533b4a4fc989c0b2e7469c1e95d26b921d94))


### Features

* Add base of LuxFilter module ([1ea066a](https://github.com/LuxoriaSoft/Luxoria/commit/1ea066a529d907373966195f99a20ffb8cb89197))
* Add compute sharpness of a bitmap ([f52b935](https://github.com/LuxoriaSoft/Luxoria/commit/f52b935e1b803ab918b59564647b77e8b613d038))
* Add Filter Pipeline base ([296a0b3](https://github.com/LuxoriaSoft/Luxoria/commit/296a0b3e61217f7ffcc7b18b5d5c54a5a3dbcc54))
* Add LuxFilter independant buildsystem & Add models ns in PipeSvc ([36221d5](https://github.com/LuxoriaSoft/Luxoria/commit/36221d598ff1f7a0e2ba09608f268c4147775700))
* Add Resolution Algorithm to Algorithms code-base ([9d0cd9d](https://github.com/LuxoriaSoft/Luxoria/commit/9d0cd9d5960cb352999adea2b488cb151ad6f466))
* Add Rust logic functions for LuxFilter ([cdf3b36](https://github.com/LuxoriaSoft/Luxoria/commit/cdf3b36ce73c26394f1f5ed7c961a7fa56583120))

## [1.11.1](https://github.com/LuxoriaSoft/Luxoria/compare/v1.11.0...v1.11.1) (2025-01-08)


### Bug Fixes

* Update LICENSE link in README to point to LICENSE.md ([514101a](https://github.com/LuxoriaSoft/Luxoria/commit/514101a3d41f0c8e15141fcbf03deb8c9c789f2e))

# [1.11.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.10.0...v1.11.0) (2024-12-22)


### Bug Fixes

* Add build step for LuxImport solution in CI workflow ([cead092](https://github.com/LuxoriaSoft/Luxoria/commit/cead0928a8620cffe0d7aa4bd8033e63398d9aff))
* Add docstrings to build functions for better clarity ([8845ea6](https://github.com/LuxoriaSoft/Luxoria/commit/8845ea6361f07d990e6cc494b0b405f8307a842c))
* Add filters in indexation process ([e01f2a9](https://github.com/LuxoriaSoft/Luxoria/commit/e01f2a98da1dba290268865f36327445acb00e4f))
* All unit tests using ImageData model ([513ec49](https://github.com/LuxoriaSoft/Luxoria/commit/513ec49ba4e4a574280065f3f8cfc314fb34227a))
* Increment LuxImport's version from 1.0.0 to 1.0.1 ([1c0cd97](https://github.com/LuxoriaSoft/Luxoria/commit/1c0cd977739049d1ea39ef2c06c15d05ca50014b))
* Replace buildsystem by one that build the entire solution ([2108176](https://github.com/LuxoriaSoft/Luxoria/commit/2108176cadf52394ddc195782cd537d36a4735f2))
* Replace ReadOnlyMemory<byte> with SKBitmap ([22b893c](https://github.com/LuxoriaSoft/Luxoria/commit/22b893cba62bd8a571fa33631a8d33bb5f697bf0))
* Update SonarCloud analysis job name in CI workflow for Luxoria ([adfe133](https://github.com/LuxoriaSoft/Luxoria/commit/adfe133d94f2410a70659e6a1a231ad792fbfdaa))
* Update SonarCloud analysis step in CI workflow for Luxoria.App ([518c53b](https://github.com/LuxoriaSoft/Luxoria/commit/518c53be7f59856d96e3845abe6eb74781989a93))


### Features

* Add Apache License 2.0 to the repository ([1fd74b0](https://github.com/LuxoriaSoft/Luxoria/commit/1fd74b069d479c55024443fd7ded4eca9fd72a8b))
* Add Contributor Covenant Code of Conduct ([af939e2](https://github.com/LuxoriaSoft/Luxoria/commit/af939e27a9996849de9e9edb1a09a3157d713d02))
* Add pull request template for Luxoria contributions ([313fe5c](https://github.com/LuxoriaSoft/Luxoria/commit/313fe5cbc811281c98ac8d66e3dccddb01fafbc6))

# [1.10.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.9.2...v1.10.0) (2024-12-08)


### Bug Fixes

* Add test to cover LogAsync method ([5132654](https://github.com/LuxoriaSoft/Luxoria/commit/513265465a7cb3019988ff2c5a0f6d8414b663cf))
* All warnings about unit tests ([18cf818](https://github.com/LuxoriaSoft/Luxoria/commit/18cf81858a2607c9bd468c940c2246329417983a))
* Fix sonar issues ([67d12a1](https://github.com/LuxoriaSoft/Luxoria/commit/67d12a12cb155a84c7fc8b668bd233a4807bc999))
* Implementation of IEvent interface ([1ddf2b7](https://github.com/LuxoriaSoft/Luxoria/commit/1ddf2b7ef30d70cf13dbc4e078a60ed3dfcc1035))


### Features

* Improve logger system ([7de7d16](https://github.com/LuxoriaSoft/Luxoria/commit/7de7d16ae3d7c34bcc095ef566edf344679e748b))

## [1.9.2](https://github.com/LuxoriaSoft/Luxoria/compare/v1.9.1...v1.9.2) (2024-12-06)


### Bug Fixes

* Add some tests to cover FileExtensionHelper class ([34349cd](https://github.com/LuxoriaSoft/Luxoria/commit/34349cdc8bb7c4a6afc1dd5f7576dde5fa253be0))
* Add some tests to cover Manifest model ([9ccebf4](https://github.com/LuxoriaSoft/Luxoria/commit/9ccebf40c6e9d6779e012c77592f498a0aeea2dc))

## [1.9.1](https://github.com/LuxoriaSoft/Luxoria/compare/v1.9.0...v1.9.1) (2024-12-06)


### Bug Fixes

* Disable tests on some Event models ([79cfabe](https://github.com/LuxoriaSoft/Luxoria/commit/79cfabe49996be94fc866c10af47181f1d2247a4))
* Excluding some models from coverage ([6ea94b0](https://github.com/LuxoriaSoft/Luxoria/commit/6ea94b01a77c2edb994b1a8f0a58edd7088a3109))

# [1.9.0](https://github.com/LuxoriaSoft/Luxoria/compare/v1.8.11...v1.9.0) (2024-12-05)


### Bug Fixes

* Remove .sonar from cache exclusions in build workflow ([32edacd](https://github.com/LuxoriaSoft/Luxoria/commit/32edacdc8109e5411f3cb1a525ba2194b4a0e712))
* Update cache paths for SonarCloud data in build workflow ([1b80d05](https://github.com/LuxoriaSoft/Luxoria/commit/1b80d05270325388a6c8158ad7c36907d582c8f8))


### Features

* Enable caching system on Build workflow ([3d14a7d](https://github.com/LuxoriaSoft/Luxoria/commit/3d14a7d6a5ffbeee2b3ff0417be719bcb90007b9))
* Including .sonar folder in cache ([b36c6aa](https://github.com/LuxoriaSoft/Luxoria/commit/b36c6aac3066f1ec661ccd02addc6f9645182d40))

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
