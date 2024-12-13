"""
Luxoria Module Build and Publish Script
----------------------------------------

This script automates the building and publishing process for the Luxoria modules.
It performs the following tasks for each module:

1. Builds the module using the 'dotnet build' command in Debug configuration.
2. Verifies if the module's DLL is successfully created.
3. Publishes the module using the 'dotnet publish' command in Release configuration to a specified output directory.

Dependencies:
- Python 3.x
- .NET SDK (Required for building and publishing modules)
- tqdm (for displaying progress bars)
- shutil (for removing non-empty directories)

Usage:
1. Ensure that the required modules are defined in the 'modules' dictionary.
2. Run this script to automatically build and publish the modules.
"""

import subprocess
import os
import shutil

# Check if tqdm is installed, if not, install it
try:
    from tqdm import tqdm
except ImportError:
    print("tqdm not found. Installing tqdm...")
    subprocess.run(["pip", "install", "tqdm"], check=True)

from tqdm import tqdm

# Main Build System
main = [
    ["Luxoria.App/Luxoria.App", "win-x64"],
]

# Luxoria Part
luxoria_part = [
    ["Luxoria.App/Luxoria.Core", "bin/Debug/net9.0/Luxoria.Core.dll"],
    ["Luxoria.App/Luxoria.Modules", "bin/Debug/net9.0/Luxoria.Modules.dll"],
    ["Luxoria.App/Luxoria.SDK", "bin/Debug/net9.0/Luxoria.SDK.dll"],
]

# Luxoria Modules
modules = {
    "LuxImport": ["Modules/LuxImport", "LuxImport/bin/Debug/net9.0/LuxImport.dll"],
}

# Function to track real progress based on output
def build_with_progress(command, desc):
    """
    Build a project with a progress bar based on the build output.
    """
    process = subprocess.Popen(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    
    # Use tqdm for real progress based on the build output
    with tqdm(total=100, desc=desc) as pbar:
        for line in process.stdout:
            # Monitor for specific keywords that indicate progress
            if "Compilation started" in line:
                # This is just an example; you'd need to adjust for your specific build output
                pbar.set_postfix(status="Compiling")
            elif "Build succeeded" in line:
                # If the build is successful, set progress to 100%
                pbar.set_postfix(status="Build Succeeded")
                pbar.n = pbar.total
                pbar.last_print_n = pbar.total
                pbar.update(0)
                break
            else:
                # Update the progress bar on each new line of output
                pbar.update(1)  # You can adjust this based on your needs

        process.wait()

# Build main program with progress bar
def build_main():
    """
    Build the main program. (Luxoria.App)
    """
    for project in main:
        try:
            print(f"Building main project {project[0]} for {project[1]}...")
            build_with_progress(
                ["dotnet", "build", project[0], "-c", "Debug", "-r", project[1]],
                f"Building {project[0]}"
            )
            print(f"Main project {project[0]} built successfully.")
        except subprocess.CalledProcessError as e:
            print(f"Error while building {project[0]}: {e}")

# Build Luxoria Part with progress bar
def build_luxoria_part():
    for part in luxoria_part:
        try:
            dll = os.path.join(part[0], part[1])

            # Delete existing DLL if it exists
            if os.path.exists(dll):
                print(f"Removing existing DLL: {dll}")
                os.remove(dll)

            # Build the part
            print(f"Building {part[0]}...")
            build_with_progress(
                ["dotnet", "build", part[0], "-c", "Debug"],
                f"Building {part[0]}"
            )

            # Verify if the DLL is created
            if os.path.exists(dll):
                print(f"{part[0]} built successfully.")
            else:
                print(f"Error while building {part[0]}")
        except subprocess.CalledProcessError as e:
            print(f"Error while building {part[0]}: {e}")

    print("Building Luxoria Part has been completed.")

# Build Luxoria Modules with progress bar
def build_and_publish_modules():
    """
    Build and publish the Luxoria modules.
    """
    for module in modules:
        try:
            print(f"Building and publishing module {module}...")

            # Get the module's path and DLL from the modules dictionary
            module_path, dll_path = modules[module]

            # Build the module
            build_with_progress(
                ["dotnet", "build", module_path, "-c", "Debug"],
                f"Building {module}..."
            )
            
            # Verify if the DLL is created after building
            dll = os.path.join(module_path, dll_path)
            print(f"Checking if DLL exists: {dll}")
            if os.path.exists(dll):
                print(f"Module {module} built successfully.")
            else:
                print(f"Error while building {module}")
                print(f"Skipping publishing for module {module}.")
                continue  # Skip publishing if build fails

            # Publish the module
            print(f"Publishing module {module}...")
            subprocess.run(
                ["dotnet", "publish", module_path, "-c", "Debug"],
                check=True
            )
            print(f"Module {module} published successfully.")
        except subprocess.CalledProcessError as e:
            print(f"Error while building and publishing {module}: {e}")

# Clear the cache
def clear_cache():
    """
    Clear the cache by deleting the obj and bin folders.
    """
    # Delete every obj and bin folder
    for root, dirs, _ in os.walk("Luxoria.App"):
        for dir in dirs:
            if dir == "obj" or dir == "bin":
                dir_path = os.path.join(root, dir)
                print(f"Removing {dir_path}")
                try:
                    shutil.rmtree(dir_path)
                    print(f"Successfully removed {dir_path}")
                except Exception as e:
                    print(f"Error removing {dir_path}: {e}")

# Execute build steps
def main_build():
    print("Starting Luxoria Main Build System...")

    # Clear the cache
    print("[1/4] Clearing cache...")
    clear_cache()

    # Build main program
    print("[2/4] Building main program...")
    build_main()

    # Build Luxoria Part
    print("[3/4] Building Luxoria Part...")
    build_luxoria_part()

    # Build Luxoria Modules
    print("[4/4] Building Luxoria Modules...")
    build_and_publish_modules()
    print("Luxoria Main Build System has been completed.")

# Start the build process
if __name__ == "__main__":
    main_build()
