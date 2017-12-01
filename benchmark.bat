@echo off
if "%1" == "" goto :help
if "%2" == "" goto :help

call git checkout origin/parallel

:: Sequential
call :benchmark %1 %2 logs\seq -n
call :benchmark %1 %2 logs\seq-local -n -l

:: Parallel
call :benchmark %1 %2 logs\par
call :benchmark %1 %2 logs\par-local -l

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
shift /3
set flags=%*
mkdir %log%

:: Log build events
echo Building...
git show -q           >  %log%\git.log
call build -c         >  %log%\build.log 2>&1
call build -r %flags% >> %log%\build.log 2>&1

:: Benchmark Funcalc for each sheet.
for /r %files% %%I in (*.xml) do (
    echo Benchmarking %%I
    call funcalc -r roots %n% "%%I" 1> "%log%\%%~nxI.out" 2> "%log%\%%~nxI.err"
)

echo Done!
endlocal
exit /b
