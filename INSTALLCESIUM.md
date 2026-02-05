# Unofficial Guide: Building Cesium for Unity on Linux

**Status:** Verified Working (Nov 2025)\
**Environment:** Linux (Ubuntu/Pop!\_OS/Debian)\
**Supported Unity Version:** 2021.3 LTS or later

## 1. Executive Summary

Cesium for Unity does not currently ship with pre-compiled binaries for
Linux. To use it, you must manually:

-   Clone the source code with submodules.
-   Compile the C# "Reinterop" tool.
-   Generate the C++ interface code using Unity.
-   Build the native C++ engine (.so libraries) using CMake.

## 2. Prerequisites

Open your terminal and run these commands to ensure your system has the
required build tools.

``` bash
# 1. Install build tools, CMake, and dependencies
# 'nasm' is critical for texture decoding performance.
# 'git-lfs' is required to download large binary assets in the repo.
sudo apt update
sudo apt install build-essential cmake libssl-dev nasm git-lfs

# 2. Initialize Git LFS
git lfs install

# 3. Verify .NET SDK (Required: v6.0 or later)
dotnet --version
# If missing, install it (example for Ubuntu):
# sudo apt install dotnet-sdk-6.0
```

## 3. Step-by-Step Installation

### Step A: Clone the Repository

❗ **Do not use Unity's Package Manager "Add from Git URL".** It fails
to clone submodules.

``` bash
mkdir -p Packages/com.cesium.unity

# The --recurse-submodules flag is MANDATORY.
git clone --recurse-submodules https://github.com/CesiumGS/cesium-unity.git Packages/com.cesium.unity
```

### Step B: Build the Reinterop Tool

``` bash
cd Packages/com.cesium.unity
dotnet publish Reinterop~ -o .
```

Check that `Reinterop.dll` appears.

### Step C: Generate C++ Interop Code

Open Unity and wait for import.\
Then check:

    Packages/com.cesium.unity/native~/Runtime/generated

If empty:

1.  Open `Runtime/ConfigureReinterop.cs`\
2.  Add a blank line and save\
3.  Return to Unity to recompile

### Step D: Compile the Native Engine

Close Unity first.

``` bash
cd Packages/com.cesium.unity/native~
cmake -B build -S . -DCMAKE_BUILD_TYPE=Release -DVCPKG_TRIPLET=x64-linux
cmake --build build --config Release --target install -j8
```

## 4. Final Configuration (Linking in Unity)

Open Unity and locate the `.so` files:

-   `libCesiumForUnityNative-Runtime.so`
-   `libCesiumForUnityNative-Editor.so`

If missing, copy from:

    native~/build/src/

### Import Settings

#### Runtime library

-   Uncheck: **Any Platform**\
-   Check: **Editor**\
-   Check: **Standalone (Linux x64)**

#### Editor library

-   Check: **Editor**\
-   Uncheck: **Standalone**

## 5. Verification

In Unity:

`Cesium > Cesium Window`\
Add **Cesium World Terrain**.

If the Scene loads terrain, the installation succeeded.

------------------------------------------------------------------------

## Troubleshooting

  ---------------------------------------------------------------------------------------------------------------
  Error Message                                        Likely Cause                 Fix
  ---------------------------------------------------- ---------------------------- -----------------------------
  `ReinteropNativeImplementationAttribute not found`   Step B failed                Run `dotnet publish` again

  `NotImplementedException (Cesium3DTileset.Update)`   Step D failed / import wrong Ensure `.so` files exist and
                                                                                    Linux is checked

  `CMake Error: Cannot guess VCPKG_TRIPLET`            Missing Linux flag           Delete build folder, rerun
                                                                                    CMake with
                                                                                    `-DVCPKG_TRIPLET=x64-linux`

  Pink/Magenta Materials                               Pipeline mismatch            Install sample materials or
                                                                                    convert
  ---------------------------------------------------------------------------------------------------------------
