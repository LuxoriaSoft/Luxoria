# LuxImport

LuxImport is a image collection management system designed for efficient indexing, asset organization, and metadata handling. It provides capabilities for importing, processing, and managing image datasets, making it an ideal solution for large-scale photography and media applications.

## Architecture

LuxImport follows a modular architecture to ensure scalability, maintainability, and high performance. The key components are:

### 1. Core Components
- **ImportService**: Handles the import process, including asset hashing, metadata extraction, and database initialization.
- **Manifest Repository**: Manages the manifest file that keeps track of indexed assets.
- **LuxConfig Repository**: Stores configuration settings related to collections and assets.
- **FileHasherService**: Computes unique hashes for images to prevent duplication.
- **SkiaSharp Integration**: Used for image processing, including metadata extraction and transformations.

### 2. Data Flow
1. **Initialization**: The system checks whether the target collection exists and initializes it if necessary.
2. **Indexing**: Images are scanned, hashed, and added to the database with relevant metadata.
3. **Storage & Retrieval**: Indexed assets are stored with reference to their respective manifest files.
4. **Loading Assets**: Assets are loaded from storage and processed based on application requirements.

### 3. Technologies Used
- **.NET 9**: Primary Framework.
- **BenchmarkDotNet**: Used for performance benchmarking.
- **System.Threading.Tasks**: Ensures efficient async processing.
- **Concurrent Collections**: Optimizes parallel data processing.
- **SkiaSharp**: Handles image processing operations such as metadata extraction and transformations.

---

## Testing

LuxImport includes a robust testing strategy to ensure stability and correctness.

### 1. Unit Tests
- Covers core functionalities such as:
  - Initialization of import service.
  - Manifest file creation and retrieval.
  - File hashing and metadata extraction.
- Uses **xUnit** for testing.

### 2. Integration Tests
- Verifies the interaction between different modules.
- Tests how the ImportService handles large datasets.

### 3. Performance Tests
- Uses **BenchmarkDotNet** to measure execution time, memory allocation, and threading behavior.
- Ensures that asset indexing, loading, and retrieval are optimized for large datasets.

### 4. Edge Case Handling
- Ensures stability with:
  - Empty folders
  - Corrupt image files
  - Large-scale imports

---

## Benchmarking

Performance is a key focus of LuxImport. The system is benchmarked to analyze indexing and retrieval efficiency.

### 1. Benchmark Strategy
- Uses **BenchmarkDotNet** to measure:
  - Execution time per operation.
  - Memory allocations and garbage collection pressure.
  - Threading efficiency and lock contention.

### 2. Benchmark Scenarios
- **Initialization**: Measures the time required to set up a new collection.
- **Indexing**: Evaluates performance when scanning and hashing assets.
- **Database Operations**: Tests manifest file read/write performance.
- **Asset Loading**: Benchmarks retrieval times for indexed images.

### 3. Sample Benchmark Results
Result is in microseconds (`μs`).  
You can find detailed benchmark results in the `LuxImport.Benchmark/BenchmarkDotNet.Artifacts` folder.  
The provided benchmark results are for reference only and may vary based on the system configuration.  
It has been tested on an Apple Silicon M2 chip on Parallels Desktop.  

With 3 different collections, 50, 100, and 200 images, the benchmark results are as follows:  

Go to the [Benchmark Report](./LuxImport.Benchmark/results/ImportServiceBenchmark-report-github.md) for more details.