# Akka.Persistence.MongoDb Benchmarks

Benchmark for per read and write operation speed of Akka.Persistence.MongoDb in milliseconds.

* Uses Mongo2Go to provide backing MongoDb database.
* Measured over 1000 reads and writes.
* Payload is a 16,384,000 bytes (16 Mb) byte array, close to MongoDb document limit.
* Two types of payload are used, static payload where each operation uses the same payload and dynamic payload where each write operation uses a randomized payload content.
* Both write transaction and non-transaction are measured when the plugin supports it.
* Outlier measurements are discarded from mean and median calculation.

## Static Payload

### 1.5.12

Mongo2Go version: 2.2.16

| Operation    | Count |      Mean |    Median |    StdDev | Outliers | Failures |
|:-------------|------:|----------:|----------:|----------:|---------:|---------:|
| SaveSnapshot |  1000 | 74.768 ms | 76.101 ms | 23.784 ms |       35 |        0 |
| LoadSnapshot |  1000 | 25.503 ms | 19.865 ms |  9.724 ms |       16 |        0 |

### 1.5.12.1

Mongo2Go version: 2.2.16

| Operation    | Count | TXN  |      Mean |    Median |    StdDev | Outliers | Failures |
|:-------------|------:|:----:|----------:|----------:|----------:|---------:|---------:|
| SaveSnapshot |  1000 |  Y   | 91.786 ms | 98.350 ms | 23.501 ms |       15 |        0 |
| SaveSnapshot |  1000 |  N   | 80.779 ms | 85.646 ms | 24.230 ms |       31 |        0 |
| LoadSnapshot |  1000 |  Y   | 28.905 ms | 24.373 ms |  9.972 ms |        6 |        0 |
| LoadSnapshot |  1000 |  N   | 28.593 ms | 23.683 ms | 10.485 ms |       20 |        0 |

### 1.5.32

Mongo2Go version: 4.1.0

| Operation    | Count | TXN |      Mean |    Median |    StdDev | Outliers | Failures |
|:-------------|------:|:---:|----------:|----------:|----------:|---------:|---------:|
| SaveSnapshot |  1000 |  Y  | 85.349 ms | 81.906 ms | 13.671 ms |        4 |        0 |
| SaveSnapshot |  1000 |  N  | 72.221 ms | 69.954 ms |  9.630 ms |       28 |        0 |
| LoadSnapshot |  1000 |  Y  | 20.793 ms | 19.744 ms |  2.268 ms |       39 |        0 |
| LoadSnapshot |  1000 |  N  | 19.759 ms | 19.209 ms |  1.362 ms |       60 |        0 |

### Dev Branch (pre-1.5.38)

Mongo2Go version: 4.1.0

| Operation    | Count | TXN |      Mean |    Median |    StdDev | Outliers | Failures |
|:-------------|------:|:---:|----------:|----------:|----------:|---------:|---------:|
| SaveSnapshot |  1000 |  Y  | 83.360 ms | 81.443 ms | 10.347 ms |       38 |        0 |
| SaveSnapshot |  1000 |  N  | 73.945 ms | 71.953 ms |  8.890 ms |       23 |        0 |
| LoadSnapshot |  1000 |  Y  | 19.740 ms | 18.877 ms |  1.829 ms |       41 |        0 |
| LoadSnapshot |  1000 |  N  | 19.506 ms | 18.788 ms |  1.834 ms |       45 |        0 |

## Dynamic Payload

### 1.5.12

Mongo2Go version: 2.2.16

| Operation    | Count | Mean       | Median     | Standard Deviation | Outliers | Failures |
|--------------|-------|------------|------------|--------------------|----------|----------|
| SaveSnapshot | 1000  | 151.144 ms | 122.674 ms | 87.693 ms          | 36       | 1        |
| LoadSnapshot | 1000  | 28.286 ms  | 23.209 ms  | 10.584 ms          | 13       | 1        |

### 1.5.12.1

Mongo2Go version: 2.2.16

| Operation    | Count | TXN |       Mean |     Median |    StdDev | Outliers | Failures |
|:-------------|------:|:---:|-----------:|-----------:|----------:|---------:|---------:|
| SaveSnapshot |  1000 |  Y  | 158.895 ms | 135.187 ms | 83.601 ms |       50 |        0 |
| SaveSnapshot |  1000 |  N  | 157.417 ms | 126.874 ms | 92.712 ms |       50 |        0 |
| LoadSnapshot |  1000 |  Y  |  27.172 ms |  22.517 ms |  9.539 ms |       15 |        0 |
| LoadSnapshot |  1000 |  N  |  25.530 ms |  21.036 ms |  9.160 ms |       24 |        0 |


### 1.5.32

Mongo2Go version: 4.1.0

| Operation    | Count | TXN |       Mean |     Median |     StdDev | Outliers | Failures |
|:-------------|------:|:---:|-----------:|-----------:|-----------:|---------:|---------:|
| SaveSnapshot |  1000 |  Y  | 237.952 ms | 187.307 ms | 119.311 ms |        4 |        0 |
| SaveSnapshot |  1000 |  N  | 239.547 ms | 187.697 ms | 126.669 ms |        4 |        0 |
| LoadSnapshot |  1000 |  Y  |  22.621 ms |  22.811 ms |   1.869 ms |       29 |        0 |
| LoadSnapshot |  1000 |  N  |  21.244 ms |  21.184 ms |   1.519 ms |       32 |        0 |

### Dev Branch (pre-1.5.38)

Mongo2Go version: 4.1.0

| Operation    | Count | TXN |       Mean |     Median |     StdDev | Outliers | Failures |
|:-------------|------:|:---:|-----------:|-----------:|-----------:|---------:|---------:|
| SaveSnapshot |  1000 |  Y  | 242.717 ms | 190.419 ms | 121.734 ms |        4 |        0 |
| SaveSnapshot |  1000 |  N  | 239.827 ms | 189.845 ms | 131.364 ms |        4 |        0 |
| LoadSnapshot |  1000 |  Y  |  19.045 ms |  18.994 ms |   1.292 ms |       20 |        0 |
| LoadSnapshot |  1000 |  N  |  20.061 ms |  19.897 ms |   1.529 ms |       18 |        0 |

