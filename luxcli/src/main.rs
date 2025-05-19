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

fn main() {
    let cli = Cli::parse();

    match cli.command {
        Commands::Build => {
            println!("Building the project...");
        }
        Commands::Clear => {
            println!("Clearing the project...");
        }
        Commands::Info => {
            println!("Showing project info...");
        }
        Commands::Mod { subcommand } => match subcommand {
            ModSubcommands::Build { dir } => {
                println!("Building module in directory: {}", dir);
            }
            ModSubcommands::Clear { dir } => {
                println!("Clearing module in directory: {}", dir);
            }
        },
    }
}