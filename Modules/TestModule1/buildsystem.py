import os
import shutil

# Constants
LUX_DLL_NAME = "TestModule3.dll"
LUX_FINAL_NAME = "TestModule3.Lux.dll"
LUX_MOD_FOLDER = "TestModule3"
SOURCE_DIR = "./TestModule3/bin/Debug/net9.0-windows10.0.26100.0/publish"
DEST_DIR = "../../Luxoria.App/Luxoria.App/bin/x64/Debug/net9.0-windows10.0.26100.0/win-x64/AppX/modules/" + LUX_MOD_FOLDER

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

if __name__ == "__main__":
    build_project()
    copy_files()
