import sys
import os
import json

"""
Marketplace Controller
Upload Manager
v0.1.0

Requirements :
Python >= 3.11
Poetry Manager

Configuration : 
Needs Environment variable : GITHUB_TOKEN (Token has RW on the target repository)

Usage : 
> poetry install
> poetry run upload-base https://github.com/ORG_USER/REPO_SRC.git https://github.com/ORG_USER/REPO_TGT.git BRANCH_TBCREATED

Released under Apache 2.0
The Luxoria Project
"""

### GLOBAL VARS
LUXMOD_FILENAME = "luxmod.json"
DOTNET_EXECUTOR = "dotnet"

### ERROR CODES
ERR_RET_CODE = "Args should be : [PATH TO LUXMOD.JSON]"

### INTERNAL FUNCTIONS

class BuildSystem:
    def __init__(self, dir, arch):
        """Reads the path as a json file"""
        self.dir = dir
        self.config_file_path = os.path.join(dir, LUXMOD_FILENAME)
        self.arch = arch

        self.log("Retreiving configuration from " + self.config_file_path)
        file = open(self.config_file_path, 'r')
        self.file_data = json.load(file)

    def log(self, content: str):
        print("[\\]\t" + content)

    def build(self):
        """Build the solution using configuration"""
        self.log(f"Loading configuration... defined by archs [{self.file_data["build"]["runtimes"]}]")
        self.log(f"Checking compatibility with {self.arch}")
        if self.arch not in self.file_data["build"]["runtimes"]:
            self.log("err: Module not compatible with the specified architecture !")
            self.log("err: Build cancelled !")
            return
        
        cmd_tbe = f"{DOTNET_EXECUTOR} publish {self.file_data["build"]["csproj"]} -c {self.file_data["build"]["config"]} -r {self.file_data["build"]["runtimes"][self.arch]}"
        self.log(f"Executing: {cmd_tbe} in [{self.dir}]")

        os.system(f"cd {self.dir} && {cmd_tbe}")
        

class ModuleBuilder:
    def __init__(self, dir, arch):
        """Fetches every folder in the specified directory"""

        # Fetch every folder in dir
        dirs = os.listdir(dir)

        for module_dir in dirs:
            path = os.path.join(dir, module_dir)
            luxmodjson_path = os.path.join(path, LUXMOD_FILENAME)
            if os.path.exists(luxmodjson_path):
                print("[*]Building [{}] \t => \t ...".format(module_dir))
                module_builder = BuildSystem(path, arch)
                module_builder.build()
            else:
                print("[ ]Skipping [{}] \t => \t err: {} is missing.".format(module_dir, LUXMOD_FILENAME))
    
def convert_long_to_short_arch(long_arch):
    archs = {
        "win-x64": "x64",
        "win-x86": "x86",
        "win-arm64": "arm64"
    }
    return archs[long_arch]

def main(args=sys.argv):
    print(f"Building architecture : [{args[1]}] ...")

    # Check arguments
    if len(args[1:]) != 2:
        exit(ERR_RET_CODE)
    
    sarch = convert_long_to_short_arch(args[1])
    dir = args[2]
    
    mbuilder = ModuleBuilder(dir, sarch)

if __name__ == '__main__':
    main()
