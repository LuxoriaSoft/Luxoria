```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4751/23H2/2023Update/SunValley3)
Apple Silicon, 6 CPU, 6 logical and 6 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), Arm64 RyuJIT AdvSIMD DEBUG
  DefaultJob : .NET 9.0.1 (9.0.124.61010), Arm64 RyuJIT AdvSIMD


```
| Method                        | Mean           | Error        | StdDev        | Median         | Completed Work Items | Lock Contentions | Gen0       | Gen1      | Gen2      | Allocated  |
|------------------------------ |---------------:|-------------:|--------------:|---------------:|---------------------:|-----------------:|-----------:|----------:|----------:|-----------:|
| BenchmarkInitializeDatabase   |       221.5 μs |      4.42 μs |       8.30 μs |       218.0 μs |                    - |                - |          - |         - |         - |      472 B |
| BenchmarkIndexCollectionAsync |   377,959.9 μs |  7,386.42 μs |  17,410.67 μs |   377,715.1 μs |               1.0000 |                - |          - |         - |         - |   823560 B |
| BenchmarkIsInitialized        |       228.1 μs |      4.49 μs |       6.85 μs |       227.3 μs |                    - |                - |          - |         - |         - |      472 B |
| BenchmarkLoadAssets           | 2,952,622.9 μs | 99,609.04 μs | 280,949.09 μs | 2,906,712.0 μs |             123.0000 |           3.0000 | 10000.0000 | 9000.0000 | 7000.0000 | 46259208 B |
