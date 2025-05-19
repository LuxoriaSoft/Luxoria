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
    },
    /// Clear a module
    Clear {
        /// Directory to clear
        dir: String,
    },
}

struct Project {
    name: String,
    path: String,
    luxconfig_path: String,
}

fn get_projectcfg(path: &str) -> Result<Project, String> {
    // Check if the path exists
    if !std::path::Path::new(path).exists() {
        return Err(String::from("Path does not exist (expected: LuxMod or ./LuxMod/luxmod.json"));
    }

    // Check if the path contains a luxmod.json file
    let luxmod_path: String = format!("{}/luxmod.json", path);
    if !std::path::Path::new(&luxmod_path).exists() {
        return Err(String::from("Path does not contain a luxmod.json file (expected: luxmod.json)"));
    }

    // Check if the luxmod.json file is valid


    return Ok(Project {
        name: "Luxoria".to_string(),
        path: "path".to_string(),
        luxconfig_path: "luxconfig_path".to_string(),
    });
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
            ModSubcommands::Build { dir } => {
                println!("LuxCLI > Building module in directory: {}...", dir);
                match get_projectcfg(&dir) {
                    Ok(project) => {
                        println!("Project name: {}", project.name);
                        println!("Project path: {}", project.path);
                        println!("Luxconfig path: {}", project.luxconfig_path);
                    }
                    Err(e) => {
                        println!("Error: {}", e);
                    }
                }
            }
            ModSubcommands::Clear { dir } => {
                println!("LuxCLI > Clearing module in directory: {}...", dir);
            }
        },
    }
}
