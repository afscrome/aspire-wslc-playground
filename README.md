# AspireSample

[![CI](https://github.com/afscrome/aspire-wslc-playground/actions/workflows/ci.yml/badge.svg)](https://github.com/afscrome/aspire-wslc-playground/actions/workflows/ci.yml)

A sample .NET Aspire application with a Blazor frontend, API service, Redis cache, and xUnit integration tests.

## Aspire configuration

- The AppHost uses the Aspire CLI bundle (`AspireUseCliBundle=true`).
- `ASPIRE_CONTAINER_RUNTIME` is set through `IConfiguration` in `AspireSample.AppHost/AppHost.cs` to select WSL Containers (`wslc`).
- Aspire packages and the AppHost SDK are pinned to 13.6.1, the latest public stable release available when this sample was created. Aspire 15.6.1 was not available from the public NuGet feed.

## Run

```powershell
aspire start --non-interactive
aspire wait cache
wslc ps
```

Run the integration tests with:

```powershell
dotnet test --solution AspireSample.sln
```

`global.json` selects Microsoft.Testing.Platform for the xUnit test project on .NET 10. The integration test starts its own AppHost, waits for the frontend to become healthy (including its Redis and API dependencies), and checks that the frontend returns HTTP 200. Stop any manually started sample before running tests so its build outputs are released.

## WSLc runtime note

WSLc requires an Aspire CLI bundle containing DCP 0.27.0 or later and WSLc 3.0.2.0 or later.

Validation passed on 2026-10-10 using the existing installed CLI `17.0.0-preview.1.26510.2+c18bc24d9a6e124581e11767e63195871165c35a`, its bundled DCP 0.27.0, and WSLc 3.0.3.0:

- AppHost started successfully using `ASPIRE_CONTAINER_RUNTIME` from `IConfiguration`.
- `aspire wait cache` and `aspire wait webfrontend` succeeded; Redis, the API, and the frontend were all running and healthy.
- `wslc ps` showed the running Redis container `cache-nzcbgqcv` (ID `8a010c5c418b`, image `redis:8.6`).
- `dotnet test --solution AspireSample.sln --verbosity minimal` passed: 1 succeeded, 0 failed, 0 skipped.
- The manually started AppHost was stopped cleanly before running the integration test.

## Windows GitHub Actions

The Windows-only CI workflow adapts the reusable workflow structure from [aspire-dotnet-jwts](https://github.com/afscrome/aspire-dotnet-jwts/blob/main/.github/workflows/ci.yml). It logs `wsl --version`, whether `Get-Command wslc` succeeds, and `wslc version` before upgrading. Each run installs prerelease WSL and the latest daily Aspire CLI, restores and builds the solution, and runs the integration tests. The tests manage their own AppHost lifecycle. Logs and build diagnostics are uploaded even when a step fails.

Daily CLI installation resolves the newest `Aspire.Cli` version from Microsoft's daily NuGet feed, downloads that version's Windows ZIP directly from Microsoft's build storage, and extracts it into the runner's normal Aspire installation directory. There are no installation retries or checksum validation. `WSL_UTF8=1` is set for all steps. Certificate trust uses `aspire certs trust --non-interactive`, with a two-minute timeout to catch a Windows trust dialog hang, followed by a trust check.
