```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4751/23H2/2023Update/SunValley3)
Apple Silicon, 6 CPU, 6 logical and 6 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 9.0.1 (9.0.124.61010), Arm64 RyuJIT AdvSIMD


```
| Method                        | Mean           | Error         | StdDev        | Median         | Gen0       | Completed Work Items | Lock Contentions | Gen1      | Gen2      | Allocated  |
|------------------------------ |---------------:|--------------:|--------------:|---------------:|-----------:|---------------------:|-----------------:|----------:|----------:|-----------:|
| BenchmarkInitializeDatabase   |       220.1 μs |       1.69 μs |       1.41 μs |       219.9 μs |          - |                    - |                - |         - |         - |      472 B |
| BenchmarkIndexCollectionAsync |   567,008.7 μs | 124,658.29 μs | 363,634.11 μs |   369,110.8 μs |          - |               1.0000 |                - |         - |         - |   823560 B |
| BenchmarkIsInitialized        |       215.4 μs |       2.38 μs |       2.83 μs |       214.7 μs |          - |                    - |                - |         - |         - |      472 B |
| BenchmarkLoadAssets           | 2,735,390.9 μs |  93,414.08 μs | 265,000.23 μs | 2,707,952.5 μs | 10000.0000 |              83.0000 |                - | 7000.0000 | 6000.0000 | 46441208 B |
