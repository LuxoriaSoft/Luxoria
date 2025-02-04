```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4751/23H2/2023Update/SunValley3)
Apple Silicon, 4 CPU, 4 logical and 4 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 9.0.1 (9.0.124.61010), Arm64 RyuJIT AdvSIMD


```
| Method                        | Mean           | Error         | StdDev          |
|------------------------------ |---------------:|--------------:|----------------:|
| BenchmarkIsInitialized        |       204.7 μs |       2.38 μs |         2.11 μs |
| BenchmarkInitializeDatabase   |       199.3 μs |       2.29 μs |         2.14 μs |
| BenchmarkIndexCollectionAsync |   487,436.6 μs |   9,645.05 μs |    23,840.17 μs |
| BenchmarkLoadAssets           | 8,870,859.5 μs | 526,318.08 μs | 1,535,294.66 μs |
