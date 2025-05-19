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
use std::vec;
use std::process::Command;

#[derive(Parser)]
#[command(name = "luxcli", version, about = "Luxoria CLI Tool")]
struct Cli {
    #[command(subcommand)]
    command: Commands,
}

#[derive(Subcommand)]
enum Commands {
    /// Build the project
    Build,
    /// Clear the project
    Clear,
    /// Show project info
    Info,
    /// Work with modules
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
    luxmod_path: String,
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
    dependencies: std::collections::HashMap<String, String>,
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
    #[serde(rename = "config")]
    config: String,
    #[serde(rename = "csproj")]
    csproj_path: String,
    #[serde(rename = "DllName")]
    dll_name: String,
    #[serde(rename = "runtimes")]
    runtimes: std::collections::HashMap<String, String>,
}

// Implement Display for pretty printing

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
    // Check if the path exists
    if !std::path::Path::new(path).exists() {
        return Err(String::from("Path does not exist (expected: LuxMod or ./LuxMod/luxmod.json)"));
    }

    // Check if the path contains a luxmod.json file
    let luxmod_path: String = format!("{}/luxmod.json", path);
    if !std::path::Path::new(&luxmod_path).exists() {
        return Err(String::from("Path does not contain a luxmod.json file (expected: luxmod.json)"));
    }

    // Check if the luxmod.json file is valid
    let luxmod_content = fs::read_to_string(&luxmod_path).map_err(|_| {
        format!("Failed to read luxmod.json file at {}", luxmod_path)
    })?;

    let luxmod: ModuleInfo = serde_json::from_str(&luxmod_content).map_err(|_| {
        format!("Failed to parse luxmod.json file at {}", luxmod_path)
    })?;


    return Ok(Project {
        luxmod_path,
        data: luxmod,
    });
}

fn get_os_arch() -> String {
    let os = std::env::consts::OS;
    let arch = std::env::consts::ARCH;
    format!("{}-{}", os, arch)
}

fn is_arch_compatible(os_arch: &str) -> bool {
    let compatibility_list: Vec<&'static str> = vec![
        "windows-x86_64",
        "windows-x86",
        "windows-arm64"
    ];

    // Check if the current OS/Arch is in the compatibility list
    compatibility_list.contains(&os_arch)
}

fn get_short_arch(arch: &str) -> String {
    match arch {
        "x86_64" => "x64".to_string(),
        "windows-x86_64" => "x64".to_string(),
        "x86" => "x86".to_string(),
        "windows-x86" => "x86".to_string(),
        "arm64" => "arm64".to_string(),
        "windows-arm64" => "arm64".to_string(),
        _ => arch.to_string(),
    }
}

// Execute dotnet publish PATH_TO_MAIN_CSPROJ -c CONFIGURATION -r RUNTIME_ID
fn publish_module(path: &str, config: &str, runtime_id: &str) -> Result<(), String> {
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
        println!("Built Successfully :\n{}", String::from_utf8_lossy(&output.stdout));
        Ok(())
    } else {
        println!("Build failed!");
        println!("{}", String::from_utf8_lossy(&output.stdout));
        Err(format!(
            "Build failed with error: {}",
            String::from_utf8_lossy(&output.stderr)
        ))
    }
}

fn main() {
    let cli = Cli::parse();

    match cli.command {
        Commands::Build => {
            println!("LuxCLI > Building the project...");
        }
        Commands::Clear => {
            println!("LuxCLI > Clearing the project...");
        }
        Commands::Info => {
            println!("LuxCLI > Showing project info...");
        }
        Commands::Mod { subcommand } => match subcommand {
            ModSubcommands::Build { dir, arch, config } => {
                println!("LuxCLI > Building module in directory: {}...", dir);
                
                let os_arch = get_os_arch();
                let mut short_arch = get_short_arch(&os_arch);

                if let Some(arch) = arch {
                    println!("Architecture: {}", arch);
                    short_arch = get_short_arch(&arch);
                } else {
                    println!("Architecture: {}", os_arch);
                    // Check if the architecture is compatible
                    if !is_arch_compatible(&os_arch) {
                        println!("Error: Architecture not compatible for building. (expected: windows-x86_64, windows-x86, windows-arm64)");
                        return;
                    }
                }

                match get_projectcfg(&dir) {
                    Ok(project) => {
                        println!("Project name: {}", project.data.name);
                        println!("Project path: {}", project.luxmod_path);
                        // Print the module info
                        println!("{}", project.data);

                        // Retrieve build configuration (Debug or Release)
                        let build_config = config.unwrap_or(project.data.build.config);
                        
                        // Retrieve runtime ID
                        let runtime_id = match project.data.build.runtimes.get(&short_arch) {
                            Some(id) => id,
                            None => {
                                println!("Error: Runtime ID not found for architecture: {}", short_arch);
                                return;
                            }
                        };

                        println!("Build configuration: {}", build_config);
                        println!("Runtime ID: {}", runtime_id);

                        // Concatenate path to csproj
                        let csproj_path = format!("{}/{}", dir, project.data.build.csproj_path);

                        // Publish the module
                        println!("Publishing module...");
                        match publish_module(&csproj_path, &build_config, runtime_id) {
                            Ok(_) => {
                                println!("Module built successfully!");
                            }
                            Err(e) => {
                                println!("Error: {}", e);
                                // return an error code
                                std::process::exit(1);
                            }
                        }
                    }
                    Err(e) => {
                        println!("Error: {}", e);
                        // return an error code
                        std::process::exit(1);
                    }
                }
            }
            ModSubcommands::Clear { dir } => {
                println!("LuxCLI > Clearing module in directory: {}...", dir);
                match get_projectcfg(&dir) {
                    Ok(project) => {
                        println!("Project name: {}", project.data.name);
                        println!("Project path: {}", project.luxmod_path);
                        // Print the module info
                        println!("{}", project.data);
                    }
                    Err(e) => {
                        println!("Error: {}", e);
                        // Return an error code
                        std::process::exit(1);
                    }
                }
            }
        },
    }
}
