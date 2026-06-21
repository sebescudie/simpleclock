# simpleclock

## Requirements

In order to publish a new release, the Nuke pipeline will look for an environment variable called `SIMPLECLOCK_GITHUB_TOKEN`. This token should have write permissions to the `Contents` of your repository.

## Nuke

In these commands, replace the path to `vvvvc` with the version of your choice

 - To compile the patch, run

```
nuke compile --compilerpath "C:\Program Files\vvvv\vvvv_gamma_7.2-win-x64\vvvvc.exe"   
```

- To compile and create a Velopac release, run

```
nuke pack --compilerpath "C:\Program Files\vvvv\vvvv_gamma_7.2-win-x64\vvvvc.exe"   
```

- To compile, create a release and upload to Github, run

```
nuke distribute --compilerpath "C:\Program Files\vvvv\vvvv_gamma_7.2-win-x64\vvvvc.exe"   
```