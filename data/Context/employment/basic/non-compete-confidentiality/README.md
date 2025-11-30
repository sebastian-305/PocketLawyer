# Context for employment Agent - basic Tier - non-compete-confidentiality Step

This directory contains context files specific to the "non-compete-confidentiality" analysis step in the basic tier.

## Usage

Place `.txt` or `.md` files in this directory. These files will be automatically loaded and included in AI prompts when the non-compete-confidentiality step is executed.

## File Format

- Supported extensions: `.txt`, `.md`
- Files are concatenated in alphabetical order
- Each file's content is prefixed with its filename as a header

## Examples

- Legal texts relevant to non-compete-confidentiality analysis
- Reference documents for non-compete-confidentiality
- Specific regulations or guidelines for non-compete-confidentiality

## Context Loading Order

When this step executes, context is loaded in the following order:
1. Agent-level common context (if exists)
2. Tier-level common context (if exists)
3. Step-specific context (this directory)
