/*
LUXCLI - Luxoria CLI

THE LUXORIA PROJECT (https://github.com/LuxoriaSoft/Luxoria)
LICENSED UNDER THE APACHE LICENSE, VERSION 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
SEE LICENSE AT https://docs.luxoria.bluepelicansoft.com/license
*/

/*
PURPOSE
This CLI is used to interact with the entire Luxoria ecosystem.
Used to :

Luxoria.App
- Build Luxoria.App
- Clear dev cache
- Run Luxoria.App

Modules
- Create a new module using a template
- Build a module & import it into Luxoria.App
- Clear dev cache
*/

use clap::{Parser, Subcommand};
use serde::Deserialize;
use std::fs;
use std::fmt;
use std::path::{Path, PathBuf};
use std::collections::HashMap;
use std::process::Command;

#[derive(Parser)]
#[command(name = "luxcli", version, about = "Luxoria CLI Tool")]
struct Cli {
    #[command(subcommand)]
    command: Commands,
}

#[derive(Subcommand)]
enum Commands {
    Build,
    Clear,
    Info,
    Mod {
        #[command(subcommand)]
        subcommand: ModSubcommands,
    },
}

#[derive(Subcommand)]
enum ModSubcommands {
    /// Build a module
    Build {
        /// Directory to build
        dir: String,
        /// Target path to Luxoria.App (./Luxoria.App)
        target: String,
        /// Optional architecture (x86, x64 or arm64)
        #[arg(short, long)]
        arch: Option<String>,
        /// Optional Configuration (Debug or Release)
        #[arg(short, long)]
        config: Option<String>,
    },
    /// Clear a module
    Clear {
        /// Directory to clear
        dir: String,
    },
}

struct Project {
    luxmod_path: PathBuf,
    data: ModuleInfo,
}

#[derive(Debug, Deserialize)]
struct ModuleInfo {
    luxmodversion: u32,
    name: String,
    description: String,
    author: String,
    email: String,
    license: String,
    url: String,
    repository: String,
    compatibility: Compatibility,
    dependencies: HashMap<String, String>,
    keywords: Vec<String>,
    build: Build,
}

#[derive(Debug, Deserialize)]
struct Compatibility {
    #[serde(rename = "mininumVersion")]
    mininum_version: String,
    #[serde(rename = "maximumVersion")]
    maximum_version: String,
}

#[derive(Debug, Deserialize)]
struct Build {
    config: String,
    #[serde(rename = "csproj")]
    csproj_path: String,
    #[serde(rename = "dll")]
    dll_name: String,
    #[serde(rename = "bin")]
    bin_path: String,
    runtimes: HashMap<String, String>,
    #[serde(rename = "targetFramework")]
    target_framework: String,
}

impl fmt::Display for ModuleInfo {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        writeln!(f, "ModuleInfo:")?;
        writeln!(f, "  Name: {}", self.name)?;
        writeln!(f, "  Description: {}", self.description)?;
        writeln!(f, "  Author: {}", self.author)?;
        writeln!(f, "  Email: {}", self.email)?;
        writeln!(f, "  License: {}", self.license)?;
        writeln!(f, "  URL: {}", self.url)?;
        writeln!(f, "  Repository: {}", self.repository)?;
        writeln!(f, "  Luxmod Version: {}", self.luxmodversion)?;
        writeln!(f, "  Compatibility: {}", self.compatibility)?;
        writeln!(f, "  Dependencies:")?;
        for (k, v) in &self.dependencies {
            writeln!(f, "    {}: {}", k, v)?;
        }
        writeln!(f, "  Keywords: {:?}", self.keywords)?;
        writeln!(f, "  Build Settings:")?;
        writeln!(f, "    Build Configuration: {}", self.build.config)?;
        writeln!(f, "    Csproj Path: {}", self.build.csproj_path)?;
        writeln!(f, "    Dll Name: {}", self.build.dll_name)?;
        writeln!(f, "    Bin Folder: {}", self.build.bin_path)?;
        writeln!(f, "    Target Framework: {}", self.build.target_framework)?;
        for (k, v) in &self.build.runtimes {
            writeln!(f, "    => {} : {}", k, v)?;
        }
        Ok(())
    }
}

impl fmt::Display for Compatibility {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        write!(
            f,
            "[mininumVersion: {}, maximumVersion: {}]",
            self.mininum_version, self.maximum_version
        )
    }
}

fn get_projectcfg(path: &str) -> Result<Project, String> {
    let base = Path::new(path);
    if !base.exists() {
        return Err("Path does not exist (expected: LuxMod or ./LuxMod/luxmod.json)".to_string());
    }

    let luxmod_path = base.join("luxmod.json");
    if !luxmod_path.exists() {
        return Err("Path does not contain a luxmod.json file (expected: luxmod.json)".to_string());
    }

    let luxmod_content = fs::read_to_string(&luxmod_path)
        .map_err(|_| format!("Failed to read luxmod.json file at {:?}", luxmod_path))?;

    let luxmod: ModuleInfo = serde_json::from_str(&luxmod_content)
        .map_err(|_| format!("Failed to parse luxmod.json file at {:?}", luxmod_path))?;

    Ok(Project {
        luxmod_path,
        data: luxmod,
    })
}

fn get_os_arch() -> String {
    format!("{}-{}", std::env::consts::OS, std::env::consts::ARCH)
}

fn is_arch_compatible(os_arch: &str) -> bool {
    matches!(
        os_arch,
        "windows-x86_64" | "windows-x86" | "windows-arm64"
    )
}

fn get_short_arch(arch: &str) -> String {
    match arch {
        "x86_64" | "windows-x86_64" => "x64",
        "x86" | "windows-x86" => "x86",
        "arm64" | "windows-arm64" => "arm64",
        _ => arch,
    }
    .to_string()
}

fn publish_module(path: &Path, config: &str, runtime_id: &str) -> Result<(), String> {
    let output = Command::new("dotnet")
        .arg("publish")
        .arg(path)
        .arg("-c")
        .arg(config)
        .arg("-r")
        .arg(runtime_id)
        .output()
        .expect("Failed to start dotnet process");

    if output.status.success() {
        println!("Build succeeded.");
        println!("{}", String::from_utf8_lossy(&output.stdout));
        Ok(())
    } else {
        println!("Build failed.");
        println!("{}", String::from_utf8_lossy(&output.stdout));
        Err(format!(
            "Build failed: {}",
            String::from_utf8_lossy(&output.stderr)
        ))
    }
}

fn upload_dir(module_name: &str, source: &Path, target: &Path) -> Result<(), String> {
    let source = source.join(module_name);

    if !source.exists() {
        return Ok(());
    }

    if !target.exists() {
        return Err(format!("Target path does not exist: {:?}", target));
    }

    // if target + module_name exists, remove it
    let target_dir = target.join(module_name);
    if target_dir.exists() {
        fs::remove_dir_all(&target_dir)
            .map_err(|_| format!("Failed to remove existing target directory: {:?}", target_dir))?;
    }

    println!("Uploading from {:?} to {:?}", source, target);
    // copy the directory from source to target
    fs::create_dir_all(target)
        .map_err(|_| format!("Failed to create target directory: {:?}", target))?;
    Ok(())
}

fn upload_module(
    module_name: &str,
    path: &Path,
    target: &Path,
    short_arch: &str,
    config: &str,
    target_framework: &str,
    runtime_id: &str,
) -> Result<(), String> {
    let published_path = target
        .join("Luxoria.App")
        .join("bin")
        .join(short_arch)
        .join(config)
        .join(target_framework);
    let source = path.join(config).join(target_framework).join(runtime_id);
    let luxoria_bin_dir = published_path.join(runtime_id);
    let modules_dir = published_path.join(runtime_id).join("modules");

    if !modules_dir.exists() {
        println!("Creating modules directory: {:?}", modules_dir);
        fs::create_dir_all(&modules_dir)
            .map_err(|_| format!("Failed to create modules directory: {:?}", modules_dir))?;
    }

    // Compiled graphics
    upload_dir(module_name, &source, &luxoria_bin_dir)?;
    Ok(())
}

fn main() {
    let cli = Cli::parse();

    match cli.command {
        Commands::Build => println!("LuxCLI > Building the project..."),
        Commands::Clear => println!("LuxCLI > Clearing the project..."),
        Commands::Info => println!("LuxCLI > Showing project info..."),
        Commands::Mod { subcommand } => match subcommand {
            ModSubcommands::Build { dir, arch, config, target } => {
                let target_path = Path::new(&target);
                if !target_path.exists() {
                    eprintln!("Error: Target path does not exist: {:?}", target_path);
                    return;
                }

                let os_arch = get_os_arch();
                let mut short_arch = get_short_arch(&os_arch);

                if let Some(arch) = arch {
                    short_arch = get_short_arch(&arch);
                } else if !is_arch_compatible(&os_arch) {
                    eprintln!("Unsupported architecture: {}", os_arch);
                    return;
                }

                match get_projectcfg(&dir) {
                    Ok(project) => {
                        println!("{}", project.data);
                        let build_config = config.unwrap_or(project.data.build.config.clone());
                        let runtime_id = match project.data.build.runtimes.get(&short_arch) {
                            Some(id) => id,
                            None => {
                                eprintln!("Runtime ID not found for architecture: {}", short_arch);
                                return;
                            }
                        };

                        let csproj_path = Path::new(&dir).join(&project.data.build.csproj_path);

                        if let Err(e) = publish_module(&csproj_path, &build_config, runtime_id) {
                            eprintln!("Error: {}", e);
                            std::process::exit(1);
                        }

                        let bin_path = Path::new(&dir).join(&project.data.build.bin_path);

                        if let Err(e) = upload_module(
                            &project.data.name,
                            &bin_path,
                            target_path,
                            &short_arch,
                            &build_config,
                            &project.data.build.target_framework,
                            runtime_id,
                        ) {
                            eprintln!("Error: {}", e);
                            std::process::exit(1);
                        }

                        println!("Module built and uploaded successfully.");
                    }
                    Err(e) => {
                        eprintln!("Error: {}", e);
                        std::process::exit(1);
                    }
                }
            }
            ModSubcommands::Clear { dir } => {
                match get_projectcfg(&dir) {
                    Ok(project) => println!("{}", project.data),
                    Err(e) => {
                        eprintln!("Error: {}", e);
                        std::process::exit(1);
                    }
                }
            }
        },
    }
}
