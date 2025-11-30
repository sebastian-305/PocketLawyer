# Context for rental Agent - basic Tier - maintenance Step

This directory contains context files specific to the "maintenance" analysis step in the basic tier.

## Usage

Place `.txt` or `.md` files in this directory. These files will be automatically loaded and included in AI prompts when the maintenance step is executed.

## File Format

- Supported extensions: `.txt`, `.md`
- Files are concatenated in alphabetical order
- Each file's content is prefixed with its filename as a header

## Examples

- Legal texts relevant to maintenance analysis
- Reference documents for maintenance
- Specific regulations or guidelines for maintenance

## Context Loading Order

When this step executes, context is loaded in the following order:
1. Agent-level common context (if exists)
2. Tier-level common context (if exists)
3. Step-specific context (this directory)
