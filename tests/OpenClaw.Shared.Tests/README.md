# OpenClaw.Shared.Tests

Unit test suite for the OpenClaw.Shared library.

## Overview

This test project provides comprehensive coverage of the OpenClaw.Shared library, focusing on:
- Data model display text generation
- Gateway client utility methods
- Notification classification
- Tool activity mapping
- Path and label formatting

## Running Tests

```bash
# Run all tests
dotnet test

# Run integration tests (disabled by default)
$env:OPENCLAW_RUN_INTEGRATION=1
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~AgentActivityTests"
```

## Test Coverage

### ModelsTests.cs (68 tests)

#### AgentActivityTests (13 tests)
- ✅ Glyph mapping for all ActivityKind values
- ✅ DisplayText formatting for main and sub sessions
- ✅ Empty label handling

#### ChannelHealthTests (23 tests)
- ✅ Status display formatting (ON, OFF, ERR, LINKED, READY, etc.)
- ✅ Channel name capitalization
- ✅ Auth age display for linked channels
- ✅ Error message inclusion
- ✅ Case-insensitive status handling

#### SessionInfoTests (22 tests)
- ✅ DisplayText formatting with various combinations
- ✅ Main vs Sub session prefixes
- ✅ Channel and activity inclusion
- ✅ Status filtering (excludes "unknown" and "active")
- ✅ ShortKey extraction for different formats:
  - Colon-separated keys (agent:main:sub:uuid)
  - File paths with forward slashes
  - File paths with backslashes (Windows)
  - Long key truncation (>20 chars)

#### GatewayUsageInfoTests (10 tests)
- ✅ Token count formatting (K, M suffixes)
- ✅ Cost display (USD)
- ✅ Request count display
- ✅ Model name display
- ✅ Combined field formatting
- ✅ Empty state ("No usage data")

### OpenClawGatewayClientTests.cs (20 tests)

#### Notification Classification (11 tests)
- ✅ Health alerts (blood sugar, glucose, CGM, mg/dl)
- ✅ Urgent alerts (urgent, critical, emergency)
- ✅ Reminders
- ✅ Stock alerts
- ✅ Email notifications
- ✅ Calendar events
- ✅ Error notifications
- ✅ Build/CI notifications
- ✅ Default to "info" type
- ✅ Case-insensitive matching
- ✅ Correct title generation

#### Tool Classification (8 tests)
- ✅ All tool name mappings (exec, read, write, edit, etc.)
- ✅ Web search tools (web_search, web_fetch)
- ✅ Default to Tool kind for unknown tools
- ✅ Case-insensitive tool names

#### Utility Methods (6 tests)
- ✅ `ShortenPath()` - path truncation and formatting
- ✅ `TruncateLabel()` - label truncation with ellipsis
- ✅ Empty and edge case handling
- ✅ Constructor validation

## Test Strategy

### Unit Tests
All tests are **pure unit tests** that don't require:
- Network connections
- WebSocket servers
- File system access
- External dependencies

### Integration Tests (Opt-in)
Integration tests are disabled by default in CI and local runs. Mark them with:
- `[IntegrationFact]`
- `[IntegrationTheory]`

Enable locally by setting `OPENCLAW_RUN_INTEGRATION=1` before running `dotnet test`.

### Reflection Usage
Some tests use reflection to access private static utility methods:
- `ClassifyNotification()`
- `ClassifyTool()`
- `ShortenPath()`
- `TruncateLabel()`

**Rationale**: These are pure utility functions with no side effects. Testing them via reflection allows:
- Direct testing of core logic without integration complexity
- Verification of behavior without exposing unnecessary public API
- Focused unit tests that are fast and reliable

**Trade-off**: Tests are coupled to method signatures and will break if signatures change. This is acceptable for stable utility methods. If these methods become unstable, consider making them `internal` and using `InternalsVisibleTo` for test access.

## Platform Considerations

### Cross-Platform Testing
Tests run on both Windows and Linux:
- Most tests are platform-agnostic
- Path handling tests account for OS-specific `Path.GetFileName()` behavior
- Tests for backslash paths verify the code detects path separators

### Windows-Specific Code
Some functionality is Windows-only (Tray app, PowerToys extension), but the Shared library tests are cross-platform compatible.

## Future Test Additions

### Recommended Integration Tests
1. Mock WebSocket server for full protocol testing
2. Reconnection logic with simulated network failures
3. Concurrent session updates
4. Large message handling

### Recommended Edge Case Tests
1. Unicode and emoji in messages
2. Very long session keys (>1000 chars)
3. Malformed JSON responses
4. High-frequency activity updates

### Recommended Performance Tests
1. Large session lists (100+ sessions)
2. Memory usage over extended runtime
3. Reconnection under load

## Contributing

When adding new functionality to `OpenClaw.Shared`:
1. Add corresponding unit tests
2. Ensure tests are cross-platform compatible
3. Test edge cases (empty strings, null values, very long inputs)
4. Maintain >80% code coverage for new code

## Dependencies

- xUnit 2.9.3 - Test framework
- .NET 9.0 - Runtime
- OpenClaw.Shared library

## License

Same as parent project (MIT License)
