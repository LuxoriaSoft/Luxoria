import os
import sys
import shutil
import platform

# Define ARCH x64, x86, ARM, ARM64
def get_short_architecture():
    arch = platform.machine().lower()
    if arch == 'amd64' or arch == 'x86_64':
        return 'x64'
    elif arch == 'x86':
        return 'x86'
    elif 'arm' in arch:
        if '64' in arch:
            return 'ARM64'
        else:
            return 'ARM'
    else:
        raise ValueError(f"Unsupported architecture: {arch}")

# Define ARCH win-x64, win-x86, win-arm, win-arm64
def get_architecture():
    arch = platform.machine().lower()
    if arch == 'amd64' or arch == 'x86_64':
        return 'win-x64'
    elif arch == 'x86':
        return 'win-x86'
    elif 'arm' in arch:
        if '64' in arch:
            return 'win-arm64'
        else:
            return 'win-arm'
    else:
        raise ValueError(f"Unsupported architecture: {arch}")

short_arch = get_short_architecture()
current_arch = get_architecture()

## Check overriden architecture
if len(sys.argv) > 1:
    short_arch = sys.argv[1]
    current_arch = sys.argv[2]

print(f"Current architecture: {current_arch} targetting /bin/{short_arch}...")

# Constants
LUX_DLL_NAME = "LuxStudio.dll"
LUX_FINAL_NAME = "LuxStudio.Lux.dll"
LUX_MOD_FOLDER = "LuxStudio"
SOURCE_DIR = "./LuxStudio/bin/Debug/net9.0-windows10.0.26100.0/" + current_arch + "/publish"
DEST_DIR = "../../Luxoria.App/Luxoria.App/bin/" + short_arch + "/Debug/net9.0-windows10.0.26100.0/" + current_arch + "/modules/" + LUX_MOD_FOLDER
MODULE_SOURCE_DIR = "./LuxStudio/bin/Debug/net9.0-windows10.0.26100.0/" + current_arch + "/LuxStudio"
MODULE_DEST_DIR = "../../Luxoria.App/Luxoria.App/bin/" + short_arch + "/Debug/net9.0-windows10.0.26100.0/" + current_arch + "/LuxStudio"

# Function to build the project
def build_project():
    print(f"Building the project for {current_arch} targetting /bin/{short_arch}...")
    os.system(f"dotnet publish -c Debug -r {current_arch}")

# Function to copy the published files to the destination directory
def copy_files():
    if not os.path.exists(SOURCE_DIR):
        print(f"Source directory does not exist: {SOURCE_DIR}")
        return

    print(f"Copying files from {SOURCE_DIR} to {DEST_DIR}...")

    # Create destination directory if it doesn't exist
    os.makedirs(DEST_DIR, exist_ok=True)

    # Copy files, excluding .pdb files
    for item in os.listdir(SOURCE_DIR):
        s = os.path.join(SOURCE_DIR, item)
        d = os.path.join(DEST_DIR, item if item != LUX_DLL_NAME else LUX_FINAL_NAME)

        if os.path.isfile(s):
            if not s.endswith('.pdb'):  # Exclude .pdb files
                shutil.copy2(s, d)  # Copy file and preserve metadata
                print(f"Copied {s} to {d}")

# Function to copy the module directory to the AppX destination
def copy_module_directory():
    source_path = os.path.abspath(MODULE_SOURCE_DIR)
    dest_path = os.path.abspath(MODULE_DEST_DIR)

    if not os.path.exists(source_path):
        print(f"Source directory does not exist: {source_path}")
        return

    # Remove the old directory if it exists
    if os.path.exists(dest_path):
        print(f"Removing old directory: {dest_path}")
        shutil.rmtree(dest_path)

    # Copy the source directory to the destination
    print(f"Copying directory from {source_path} to {dest_path}...")
    shutil.copytree(source_path, dest_path)
    print(f"Copied directory {source_path} to {dest_path}")

if __name__ == "__main__":
    build_project()
    copy_files()
    copy_module_directory()
    input("Press enter to finish")
