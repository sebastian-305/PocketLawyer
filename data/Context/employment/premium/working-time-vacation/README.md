# Context for employment Agent - Premium Tier - working-time-vacation Step

This directory contains context files specific to the "working-time-vacation" analysis step in the Premium tier.

## Usage

Place `.txt` or `.md` files in this directory. These files will be automatically loaded and included in AI prompts when the working-time-vacation step is executed.

## File Format

- Supported extensions: `.txt`, `.md`
- Files are concatenated in alphabetical order
- Each file's content is prefixed with its filename as a header

## Examples

- Legal texts relevant to working-time-vacation analysis
- Reference documents for working-time-vacation
- Specific regulations or guidelines for working-time-vacation

## Context Loading Order

When this step executes, context is loaded in the following order:
1. Agent-level common context (if exists)
2. Tier-level common context (if exists)
3. Step-specific context (this directory)
