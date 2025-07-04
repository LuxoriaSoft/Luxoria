# Luxoria UI Manual Test Protocol

## Description
This document defines the manual test protocol for building and validating Luxoria.App and its associated modules and components on three target architectures: x64, x86, and ARM64.
Each module LuxImport, LuxFilter, LuxEditor, LuxExport, LuxStudio and the standard component Marketplace is built and tested individually. Developers are responsible for executing and documenting all feature tests.

## Scope
- Build validation for Luxoria.App on x64, x86, ARM64.
- Module assembly and feature verification for each module and the Marketplace component.
- Manual test cases executed by module developers.

## Prerequisites
- Source code checkout of Luxoria repository (latest main branch).
- Visual Studio 2022 + WinUI Developpement Tool
- Windows App 10/11/1X SDK
- Windows 10/11

## Build and Verification Procedures
1. Open VS 2022
2. Load Luxoria.App.sln
3. Run Luxoria.App
4. Build each module such as 
- LuxImport
- LuxFilter
- LuxEditor
- LuxExport
- LuxStudio
5. Execute each testing requirements (see below for specifications)

## Testing Specifications

### LuxImport
TEST 1:
> Open an existing Collection : OK

TEST 2:
> Open a new folder, then create a new name and click on import : OK

Success : A pop-up is being displayed after the importation has been completed

### Luxfilter
TEST 1:
> Check Brisque Algorithm  
> Click on Save / Sync : OK

TEST 2:
> Check CLIP Model  
> Click on Save / Sync: OK

TEST 3:
> Select another model/algorithm  
> Click on Save / Sync : OK

### LuxEditor
TEST 1:
> Tricks some parameters : OK

TEST 2:
> Tricks some parameters  
> Crop image : OK

TEST 3:
> Tricks some parameters  
> Crop the image
> Use the layer system  
> Crop image : OK

### LuxExport
TEST 1:
> Export as PNG : OK

TEST 2:
> Export as other format  
> Usage of watermark : OK

TEST 3:
> Export as other format  
> Usage of watermark (icon)  
> Usage of naming template : OK

### LuxExport + LuxStudio
TEST 1:
> Connect to Studio : OK

TEST 2:
> Connect to Studio : OK   
> Export to studio : OK


### Bugs / Issues
For any bug found, please create an issue here : https://github.com/LuxoriaSoft/Luxoria/issues