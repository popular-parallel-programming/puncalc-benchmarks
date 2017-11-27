@echo off
if "%1" == "" goto :help
if "%2" == "" goto :help

:: Sequential
call git checkout origin/parallel
call :benchmark %1 %2 logs\seq -n -t

:: Parallel without thread local optimization
::call git checkout origin/parallel-no-thread-local
::call :benchmark %1 %2 logs\no-thread-local

:: Parallel
call git checkout origin/parallel
call :benchmark %1 %2 logs\parallel -t

exit /b

:help
echo Usage:
echo   benchmark.bat path\to\sheets iterations
echo.
echo path\to\sheets - Path to a folder that contains XML spreadsheets.
echo iterations     - Number of iterations to repeat.
echo.

:: Done
exit /b

:benchmark
setlocal
set files=%1
set n=%2
set log=%3
set flags=%4
mkdir %log%

:: Log build events
echo Building...
git show -q           >  %log%\git.log
call build -c         >  %log%\build.log 2>&1
call build -r %flags% >> %log%\build.log 2>&1

:: Benchmark Funcalc for each sheet.
for /r %files% %%I in (*.xml) do (
    echo Benchmarking %%I
    call funcalc -r full %n% "%%I" 1> "%log%\%%~nxI.out" 2> "%log%\%%~nxI.err"
)

echo Done!
endlocal
exit /b
