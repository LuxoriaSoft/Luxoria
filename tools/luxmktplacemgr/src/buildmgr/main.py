import sys
import os
import json
import shutil
import tempfile
import subprocess

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

PLATFORM_TO_ARCH = {
    "win-x64": "x64",
    "win-x86": "x86",
    "win-arm64": "arm64"
}

### ERROR CODES
ERR_RET_CODE = "Args should be : [PATH TO LUXMOD.JSON]"
ERR_PLATFORM_NOT_SUPPORTED = "The specified platform is not currently supported"

### INTERNAL FUNCTIONS

class BuildSystem:
    def __init__(self, dir, arch, platform):
        """Reads the path as a json file"""
        self.dir = dir
        self.config_file_path = os.path.join(dir, LUXMOD_FILENAME)
        self.arch = arch
        self.platform = platform

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
        
        try:
            cmd_output = subprocess.check_output(f"cd {self.dir} && {cmd_tbe}", shell=True, text=True)
            log_filename = f"{self.file_data["name"]}.{self.arch}.log"
            self.log(f"Saving log as {log_filename}...")
            f = open(log_filename, "w+")
            f.write(cmd_output)
            f.close()
        except subprocess.CalledProcessError as e:
            self.log(f"err: executing command: {e}")
            return 1
        return 0
    
    def prepare_export(self, output):
        """Renames dll from Name.dll to Name.Lux.dll & Moves bin folder to target folder (output)"""
        framework = self.file_data["build"]["targetFramework"]
        config = self.file_data["build"]["config"]
        platform = self.platform
        pdir = os.path.join(self.dir, self.file_data["build"]["bin"], config, framework, platform, "publish")
        
        self.log(f"Renaming DLL in {pdir}")
        os.rename(os.path.join(pdir, self.file_data["build"]["dll"]), self.file_data["build"]["dll"].replace(".dll", ".Lux.dll"))
        shutil.copytree(pdir, os.path.join(output, f"{self.file_data["name"]}.{self.arch}"))

class ModuleBuilder:
    def __init__(self, dir, arch, platform, output_dir):
        """Fetches every folder in the specified directory"""

        self.arch = arch
        self.platform = platform
        self.dir = dir
        self.output_dir = output_dir

        # Fetch every folder in dir
        self.dirs = os.listdir(dir)

    def build_all(self):
        for module_dir in self.dirs:
            path = os.path.join(self.dir, module_dir)
            luxmodjson_path = os.path.join(path, LUXMOD_FILENAME)
            if os.path.exists(luxmodjson_path):
                print("[*]Building [{}] \t => \t ...".format(module_dir))
                module_builder = BuildSystem(path, self.arch, self.platform)
                if module_builder.build() == 0:
                    module_builder.prepare_export(self.output_dir)
            else:
                print("[ ]Skipping [{}] \t => \t err: {} is missing.".format(module_dir, LUXMOD_FILENAME))

def main(args=sys.argv):
    print(f"Building architecture : [{args[1]}] ...")

    # Check arguments
    if len(args[1:]) != 2:
        exit(ERR_RET_CODE)
    
    platform = args[1]
    try:
        arch = PLATFORM_TO_ARCH[platform]
    except:
        exit(ERR_PLATFORM_NOT_SUPPORTED)

    dir = args[2]
    output_dir = "out"

    if os.path.exists(output_dir) == False:
        print(f"Creating directory : {output_dir}...")
        os.mkdir(output_dir)

    print(f"Saving builds in {output_dir}...")
    
    mbuilder = ModuleBuilder(dir, arch, platform, output_dir)
    mbuilder.build_all()

    os.system(f"ls {output_dir}")

if __name__ == '__main__':
    main()
