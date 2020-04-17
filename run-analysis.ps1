# Runs analysis tool: https://docs.microsoft.com/en-us/visualstudio/profiling/concurrency-visualizer-command-line-utility-cvcollectioncmd?view=vs-2019
Remove-Item -Path "analysis/*" -Recurse -Force -ErrorAction SilentlyContinue
& "C:\Program Files (x86)\Microsoft Concurrency Visualizer Collection Tools\CVCollectionCmd.exe" /launch bigsort\bin\Release\netcoreapp3.1\bigsort.exe /launchargs merge-sort /outdir analysis
