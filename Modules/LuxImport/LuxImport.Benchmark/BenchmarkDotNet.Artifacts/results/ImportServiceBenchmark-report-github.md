```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4751/23H2/2023Update/SunValley3)
Apple Silicon, 4 CPU, 4 logical and 4 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 9.0.1 (9.0.124.61010), Arm64 RyuJIT AdvSIMD


```
| Method                        | Mean         | Error       | StdDev       | Median       | Completed Work Items | Lock Contentions | Allocated |
|------------------------------ |-------------:|------------:|-------------:|-------------:|---------------------:|-----------------:|----------:|
| BenchmarkIsInitialized        |     219.6 μs |     4.36 μs |      3.64 μs |     219.9 μs |                    - |                - |     472 B |
| BenchmarkInitializeDatabase   |     217.9 μs |     1.69 μs |      1.41 μs |     218.2 μs |                    - |                - |     472 B |
| BenchmarkIndexCollectionAsync | 502,401.9 μs | 9,990.52 μs | 22,345.23 μs | 494,135.7 μs |               1.0000 |                - | 1631400 B |
| BenchmarkLoadAssets           |           NA |          NA |           NA |           NA |                   NA |               NA |        NA |

Benchmarks with issues:
  ImportServiceBenchmark.BenchmarkLoadAssets: DefaultJob
