# Benchmarking Script for Parallel Funcalc #

This repository contains the script for running parallel benchmarks and performance data.  We introduce one branch per machine on which we test.

How to benchmark:

1. Copy the file `benchmark.bat` to the Funcalc repository.
2. Run `>benchmark.bat path\to\sheets 100` or similar.
3. Copy files from the newly created `log` subfolder into this repository.
4. Commit to the correct branch!


# Current(ly Planned) Branches #

| Branch                | Purpose                                 |
|-----------------------|-----------------------------------------|
| [origin/master]()     | For workflow changes and analysis code. |
| [origin/win7-i7]()    | For Intel i7 benchmark data.            |
| [origin/win10-xeon]() | For Intel Xeon benchmark data.          |
