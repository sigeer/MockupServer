# AGENTS.md

## Structure
- .NET 7 solution `MockupServer.sln`, two projects, no test/lint/CI config anywhere.
  - `MockupServer/` — **class library** (no `Main`): Kestrel host `WebServerService.Create(GlobalCache)` (`WebServerService.cs`), request pipeline `LocalDataSource/MockupService.cs`, persistence `Storage/DataService.cs` (FreeSql + SQLite), Roslyn random-data codegen `Response/CSharpUtility.cs`.
  - `MockupServer.UI/` — WinForms exe (`net7.0-windows`), the **only runnable project**: `Program.cs` → `Form1`; Start button hosts the web server in-process (`Form1.cs:48`).
- `MockupServer/Properties/launchSettings.json` is stale (its project is a library): `dotnet run --project MockupServer` fails with "no Main".

## Commands
- Verify changes: `dotnet build MockupServer.sln` — the only automated check (no tests, no lint/format). Both projects have `Nullable` enabled, so nullable warnings (CS8603/CS8604) are the typecheck signal. Always-expected warnings: NU1701 (`ICSharpCode.TextEditor` is .NET-Framework-only), NETSDK1138 (net7.0 EOL).
- Run the app: `dotnet run --project MockupServer.UI` (Windows only).
- Smoke-test the server headlessly (there are no tests): a throwaway console project referencing `MockupServer.csproj` that calls `WebServerService.Create(new GlobalCache { ListenerPort = N })` then `StartServerAsync()` (extension typo: `StopServerAysnc`). Verified: starts and answers 404 on `http://N` and `https://N+1` with an empty DB.

## Runtime data / config gotchas
- All state (domains, mock records, port) lives in SQLite `database.db` at a **path relative to the process working directory** (`ServiceCollectionExtensions.cs:16`); FreeSql `UseAutoSyncStructure(true)` creates/alters the schema — no migration files. `database.db` is **not gitignored**: running from the repo root leaves an untracked file (delete it, never commit). Different working directory = different, empty dataset.
- Listener port comes from the DB (`GlobalCache.ListenerPort`, default 9550), edited in the UI — not from `appsettings.json`. Kestrel binds `http://*:Port` and `https://*:Port+1`.
- `appsettings.json` keys (`Port`, `OriginalServiceUrl`, `MongoDB`) are loaded but never read by any code; `appsettings.Development.json` is never used because the host forces environment `Staging` (`WebServerService.cs:27`).
- Helpers `ArgumentsUtils`, `GetMimeTypeByFileName`, `CustomWriteTo` come from NuGet (`SigeerUtility`, `SigeerUtility.Serilog.Extension`), not this repo.

## Request pipeline (`MockupService.GetResponseMessage`)
- Matches `Request.GetDisplayUrl()` (scheme + **Host header** + path + query) against `domain` rows: longest prefix wins, rows with empty `Target` are skipped, no match → 404. Per matching row: local mock lookup (`Method` + relative path) → else proxy to `Target`. So a proxying client must preserve the original Host, or rules must be written for the Host actually sent.
- Relative path = `totalUrl.Substring(prefix.Length - 1)` (`MockupService.cs:53`) → **domain prefixes must end with `/`**. Without it the prefix's last char leaks into the path and the proxied URL becomes invalid (`Invalid port specified`). `README.MD`'s example row for prefix `.../auth/token` (claiming `/1`) does not match the code; trust the code.
- Query strings are part of the mock lookup key, so a record saved without its `?...` will not be hit.

## Conventions
- Adding a `ResponseType` enum member requires three edits: `Response/ResponseType.cs` (see its XML doc), `DataService.GenerateResponseMessage`, and UI branches in `EditorForm.cs` / `DataEditForm.cs`.
- Random-JSON feature: the C# snippet is compiled at runtime; entry class must be named `Root` and expose a `data` property (`[Range]`/`[Default]` attributes shape values) — documented in `README.MD` section "新功能".
- **Encoding**: sources are UTF-8 with BOM except `MockupServer.UI/Form1.cs`, which is GBK/CP936 (verified — its Chinese strings are not valid UTF-8). Preserve each file's existing encoding; re-saving `Form1.cs` as UTF-8 garbles it. UI strings/comments are Chinese — keep new ones Chinese.
- Git: active branch `work`, merged into `master` by PR; commit subjects are Chinese with a type prefix, e.g. `更新：...`, `修复：...`, `优化：...`, `其他：...`, `文本：...`.
