#!/usr/bin/env bash
set -euo pipefail

# Runs all BenchmarkDotNet benchmarks dynamically (no need to update when adding new benchmarks).
# Does NOT generate the consolidated markdown report anymore; use the harness:
#   dotnet run -c Release --project benchmarks/Veggerby.Boards.Benchmarks.csproj -- --generate-report [--report-out <path>]
# Usage:
#   ./tools/run-all-benchmarks.sh            # Release build, default runtime
#   ENV VARS:
#     BENCH_CONFIG=Release|Debug (default Release)
#     BENCH_FILTER="*Pattern*" to restrict (optional)
#     BENCH_JOB="--job short" or any extra BenchmarkDotNet job args (optional)
#     DOTNET_RUN_OPTS to pass additional dotnet run options (optional)
# Output:
# Output:
#   BenchmarkDotNet artifact files under BenchmarkDotNet.Artifacts/results (CSV/HTML/md per benchmark).
#   For a consolidated markdown summary invoke the harness with --generate-report.
#
# Note: For full statistically robust results remove any short job override and allow the Default job.

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BENCH_PROJ="$ROOT_DIR/benchmarks/Veggerby.Boards.Benchmarks.csproj"
BENCH_CONFIG="${BENCH_CONFIG:-Release}"
BENCH_FILTER="${BENCH_FILTER:-}"  # optional pattern
EXTRA_JOB_ARGS="${BENCH_JOB:-}"   # e.g. --job short
RUN_OPTS="${DOTNET_RUN_OPTS:-}"   # additional user-specified options
BENCH_OUT="${BENCH_OUT:-}"        # optional output file or directory for report

TIMESTAMP="$(date -u +'%Y-%m-%dT%H:%M:%SZ')"

echo "[bench] Configuration: $BENCH_CONFIG" >&2
if [[ -n "$BENCH_FILTER" ]]; then
  echo "[bench] Filter: $BENCH_FILTER" >&2
fi
if [[ -n "$EXTRA_JOB_ARGS" ]]; then
  echo "[bench] Extra job args: $EXTRA_JOB_ARGS" >&2
fi

pushd "$ROOT_DIR" >/dev/null

echo "[bench] Building solution..." >&2
dotnet build "$ROOT_DIR/Veggerby.Boards.sln" -c "$BENCH_CONFIG" >/dev/null

echo "[bench] Running benchmarks..." >&2
RUN_ARGS=("--project" "$BENCH_PROJ" "-c" "$BENCH_CONFIG" "--")
if [[ -n "$BENCH_FILTER" ]]; then
  RUN_ARGS+=("--filter" "$BENCH_FILTER")
else
  RUN_ARGS+=("--filter" "*")
fi
if [[ -n "$EXTRA_JOB_ARGS" ]]; then
  for tok in $EXTRA_JOB_ARGS; do RUN_ARGS+=("$tok"); done
fi
dotnet run "${RUN_ARGS[@]}" $RUN_OPTS

echo "[bench] Benchmarks complete at $TIMESTAMP. Use harness --generate-report to build consolidated markdown." >&2

popd >/dev/null

exit 0
