| Method         | Mean          | Error        | StdDev       | Gen0        | Gen1      | Gen2      | Allocated    |
|--------------- |--------------:|-------------:|-------------:|------------:|----------:|----------:|-------------:|
| LiteDbInsert   |      89.17 us |     2.098 us |     6.186 us |     12.5732 |    0.1221 |         - |     25.76 KB |
| SqliteInsert   |   2,528.32 us |    49.612 us |    96.764 us |           - |         - |         - |      2.26 KB |
| LiteDbUpdate   |      62.87 us |     0.715 us |     0.669 us |     23.9258 |         - |         - |      48.9 KB |
| SqliteUpdate   |      36.28 us |     0.170 us |     0.142 us |      1.0376 |         - |         - |      2.19 KB |
| LiteDbReadById |      15.50 us |     0.123 us |     0.103 us |     14.0991 |         - |         - |     28.82 KB |
| SqliteReadById |      32.04 us |     0.159 us |     0.124 us |      1.3428 |         - |         - |      2.74 KB |
| SqliteReadAll  | 122,643.14 us | 2,418.939 us | 4,483.658 us |   6000.0000 | 3500.0000 | 1000.0000 |  34557.14 KB |
| LiteDbReadAll  | 232,447.70 us | 3,807.350 us | 3,375.117 us | 145000.0000 | 2000.0000 |         - | 303074.56 KB |