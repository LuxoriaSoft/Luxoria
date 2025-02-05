
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
Apple Silicon, 6 CPU, 6 logical and 6 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), Arm64 RyuJIT AdvSIMD DEBUG
  DefaultJob : .NET 9.0.1 (9.0.124.61010), Arm64 RyuJIT AdvSIMD


 Method            | Mean                | Error            | StdDev           | Gen0       | Completed Work Items | Lock Contentions | Allocated   |
------------------ |--------------------:|-----------------:|-----------------:|-----------:|---------------------:|-----------------:|------------:|
 ComputeResolution |            18.65 ns |         0.113 ns |         0.100 ns |          - |                    - |                - |           - |
 ComputeSharpness  | 1,247,542,131.41 ns | 4,759,688.559 ns | 3,974,555.516 ns | 94000.0000 |                    - |                - | 396981440 B |
 ComputeBrisque    |   167,962,263.89 ns |   936,623.349 ns |   731,253.903 ns |          - |                    - |                - |      1296 B |
