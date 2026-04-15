# Vendored Code

This folder contains third-party source that is intentionally vendored into Fibrous.

## Quartz CronExpression

File:

- `Quartz_CronExpression.cs`

Origin:

- Quartz.NET
- Apache License 2.0

Why it is vendored:

- Fibrous wants Quartz-style cron expression support without taking a runtime dependency on the full Quartz package.

Local notes:

- The file retains the original `Quartz` namespace so the copied parser can stay close to upstream behavior.
- Fibrous uses the parser through its own cron scheduling wrapper in `CronScheduler`.
- Small local updates may be applied to keep behavior current or improve validation.

When updating:

1. Prefer targeted syncs for specific fixes rather than ad hoc edits.
2. Preserve the upstream license header.
3. Keep Fibrous-specific behavior in wrapper code where possible.
