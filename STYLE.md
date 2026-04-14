# Fibrous Style Guide

This repository favors compact, cohesive implementation units over strict IDE-default file splitting, but the structure should still look deliberate and consistent.

## File Organization

1. One public concept per file by default.
2. Tightly related internal helper types may share a file with the owning type.
3. Do not place multiple unrelated public APIs in the same file.
4. File names should match the primary public type or implementation concept.
5. Keep WPF-specific code isolated under the WPF project.
6. Keep extras isolated by folder and namespace, not mixed into core areas.

## Type Grouping

Multiple types in one file are acceptable when:
- at most one type is public
- the helper types are short
- the helper types are only meaningful with the owning type
- splitting them would make the implementation harder to follow

Split files when:
- more than one public type in the file is independently useful
- a file mixes unrelated concerns
- the file becomes difficult to scan quickly

## Member Ordering

Use this order in most files:
1. constants
2. static fields
3. readonly fields
4. mutable fields
5. constructors
6. public members
7. protected members
8. private members

## Formatting

- Keep constructors and hot paths compact.
- Use blank lines to separate logical blocks, not every statement group.
- Avoid excessive vertical whitespace.
- Keep `using` directives minimal and ordered consistently.
- Keep namespace style consistent within a file and gradually normalize across the repo.

## Comments

- Prefer comments that explain concurrency invariants, sequencing, disposal, or scheduling semantics.
- Avoid comments that merely restate the code.
- In hot-path code, add comments only when they explain a non-obvious correctness or performance tradeoff.

## Public API Files

Public API files should be stricter than internal implementation files:
- one public type per file
- cleaner XML documentation
- minimal internal helper detail

## Tests and Benchmarks

- Organize tests by behavior or subsystem.
- Organize benchmarks by performance question or subsystem.
- Prefer parameterized benchmarks over multiple nearly identical benchmark classes.

## Practical Rule

Optimize for cohesive implementation units, not one-type-per-file purity.
