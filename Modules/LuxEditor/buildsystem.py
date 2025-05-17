import os
import shutil


# Constants
LUX_DLL_NAME = "LuxEditor.dll"
LUX_FINAL_NAME = "LuxEditor.Lux.dll"
LUX_MOD_FOLDER = "LuxEditor"
SOURCE_DIR = "./LuxEditor/bin/Debug/net9.0-windows10.0.26100.0/publish"
DEST_DIR = "../../Luxoria.App/Luxoria.App/bin/x64/Debug/net9.0-windows10.0.26100.0/win-x64/modules/" + LUX_MOD_FOLDER
MODULE_SOURCE_DIR = "./LuxEditor/bin/Debug/net9.0-windows10.0.26100.0/LuxEditor"
MODULE_DEST_DIR = "../../Luxoria.App/Luxoria.App/bin/x64/Debug/net9.0-windows10.0.26100.0/win-x64/LuxEditor"

# Function to build the project
def build_project():
    print("Building the project...")
    os.system("dotnet publish -c Debug")

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
