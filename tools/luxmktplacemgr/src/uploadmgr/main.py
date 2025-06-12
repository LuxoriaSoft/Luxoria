import sys
import os
import shutil
import tempfile
from git import Repo

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

### ERROR CODES
ERR_RET_CODE = "Args should be : [REPO URL FROM] [REPO URL TO] [BRANCH NAME]"
ERR_REP_NOT_GITHUB = "Repo should be hosted on Github (https://github.com/)"
ERR_REP_GH_NOT_FOUND = "GITHUB_TOKEN environment variable not set"

### INTERNAL FUNCTIONS
def is_github(repo_url: str) -> bool:
    """Verifies if the URL is hosted on github"""
    return repo_url.startswith("https://github.com") and repo_url.endswith(".git")

def clone_repo(url: str, branch: str, path: str) -> Repo:
    """Clones repository using URL & specific branch"""
    print("Cloning repository [{} @ {}] at : ".format(url, branch), path)
    repo = Repo.clone_from(url, path, branch=branch)
    print("OK!")
    return repo

def inject_token(url: str, token: str) -> str:
    """Injects a GitHub Personal Access Token into a GitHub HTTPS URL"""
    if not url.startswith("https://github.com"):
        raise ValueError("URL must start with 'https://github.com'")
    return url.replace("https://", f"https://{token}@")


def upload_dirs(repo_src: str, repo_dst: str, branch_src: str, branch_dst: str, repo_token: str):
    """Uploads the dirs from src to dst (target)"""
    folders = ["Luxoria.App", "Modules"]

    print("Uploading files...")
    print("From repository [{} @ {}] to [{} @ {}]".format(repo_src, branch_src, repo_dst, branch_dst))

    if is_github(repo_src) == False or is_github(repo_dst) == False:
        exit(ERR_REP_NOT_GITHUB)
    
    print("Creating [TMP] transfer directory...")
    transfer_dir = tempfile.mkdtemp(prefix="lux")

    src_repo_path = os.path.join(transfer_dir, "src")
    src_repo = clone_repo(repo_src, branch_src, src_repo_path)

    dest_repo_path = os.path.join(transfer_dir, "dest")
    dest_repo = clone_repo(inject_token(repo_dst, repo_token), "main", dest_repo_path)

    print("Creating new branch...")
    head = dest_repo.create_head(branch_dst)
    print("OK!")

    head.checkout()

    print("Transferring files...")
    for folder in folders:
        src_path = os.path.join(src_repo_path, folder)
        dest_path = os.path.join(dest_repo_path, folder)
        print("Uploading folder [{}] from {} to {}...".format(folder, src_path, dest_path))
        shutil.copytree(src_path, dest_path)
    print("OK!")

    print("Indexing files into repository...")
    dest_repo.index.add(folders)
    print("OK!")

    print("Committing files...")
    dest_repo.index.commit("feat: " + branch_dst)
    print("OK!")

    print("Pushing changes...")
    dest_repo.git.push("origin", branch_dst)
    print("OK!")
    
def main(args=sys.argv):
    print("Upload Manager is running...")
    
    # Check arguments
    if len(args[1:]) != 3:
        exit(ERR_RET_CODE)
    
    # Retreive token from env var
    GH_TOKEN = os.getenv("GITHUB_TOKEN")

    if GH_TOKEN == None or len(GH_TOKEN) == 0:
        exit(ERR_REP_GH_NOT_FOUND)

    repo_from, repo_to, branch_name = args[1], args[2], args[3]
    # Start uploading
    upload_dirs(repo_src=repo_from, branch_src="main", repo_dst=repo_to, branch_dst=branch_name, repo_token=GH_TOKEN)

if __name__ == '__main__':
    main()