#!/usr/bin/env bash
set -e

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
WEB_PROJECT="$PROJECT_DIR/src/SmartLeads.Web"
URL="http://localhost:5284"
SWAGGER_URL="http://localhost:5284/swagger"

echo "=========================================="
echo "  SmartLeads - Starting Application"
echo "=========================================="
echo ""

# Start the .NET project in background
dotnet run --project "$WEB_PROJECT" &
DOTNET_PID=$!

# Wait for the server to be ready
echo "Waiting for server to start..."
while ! curl -s "$URL" > /dev/null 2>&1; do
    sleep 1
done

echo "Server is ready!"
echo ""

# Open both URLs in the default browser
if command -v xdg-open &> /dev/null; then
    xdg-open "$URL"
    xdg-open "$SWAGGER_URL"
elif command -v open &> /dev/null; then
    open "$URL"
    open "$SWAGGER_URL"
elif command -v start &> /dev/null; then
    start "$URL"
    start "$SWAGGER_URL"
else
    echo "Could not detect browser opener. Please visit:"
    echo "  Frontend: $URL"
    echo "  Swagger:  $SWAGGER_URL"
fi

echo ""
echo "Frontend: $URL"
echo "Swagger:  $SWAGGER_URL"
echo ""
echo "Press Ctrl+C to stop the application"

# Wait for dotnet process to finish
wait $DOTNET_PID
