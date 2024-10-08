#!/bin/bash
set -e
cd "$(dirname "$0")"

cd ../

dotnet run --configuration Release --project ./src/TemplateRefGenerator \
  --source-folder ../bicep-types-az/generated \
  --output-folder ./generated \
  --verbose