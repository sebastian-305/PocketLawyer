# Kollektivvertrag (KV) Storage Organization

## Directory Structure

The KV files are organized by contract type to support the new agent architecture:

```
data/KVS/
  └── Employment/
      ├── kollektivvertrag_handel/
      ├── kollektivvertrag_hotel_und_gastgewerbe/
      ├── kollektivvertrag_pharmazie_apotheke/
      └── ... (54 total KV directories)
```

## Purpose

This organization allows:
- **Contract Type Separation**: Employment-related KVs are grouped together
- **Future Extensibility**: Easy to add other contract types (e.g., `Rental/`, `Partnership/`)
- **Clear Naming Convention**: Each KV has its own subdirectory with descriptive name
- **Agent-Specific Loading**: The EmploymentContractAgent loads KVs from `data/KVS/Employment/`

## Configuration

The KV storage path is configured in `appsettings.json`:

```json
"Paths": {
  "KVSPath": "data/KVS/Employment"
}
```

## How It Works

1. **KV Detection**: During contract initialization, the `KvAiDetectionService` analyzes the contract and identifies the applicable KV
2. **KV Loading**: The `KvFileService` loads all files from the detected KV directory
3. **Analysis Context**: The loaded KV text is used as additional context in Premium Analysis steps

## Adding New KVs

To add a new Kollektivvertrag:

1. Create a new directory under `data/KVS/Employment/` with a descriptive name (e.g., `kollektivvertrag_new_industry`)
2. Place all relevant KV files (PDF, TXT, etc.) in that directory
3. The system will automatically detect and use the new KV when analyzing contracts

## Naming Convention

KV directories follow this pattern:
- `kollektivvertrag_{industry_name}` - for standard collective agreements
- `mindestlohntarif_{category}` - for minimum wage agreements
- `lohntabellen_{category}` - for wage tables
- `gehaltstabellen_{category}` - for salary tables

All names use lowercase with underscores for spaces.

## Requirements Satisfied

This organization satisfies the following requirements from the Employment Contract Agent specification:

- **Requirement 6.1**: KVs are stored in an optimized directory for the new agent logic
- **Requirement 6.3**: KVs are loaded from the configured storage location
- **Requirement 6.4**: Clear naming convention is used for all KVs

## Migration Notes

**Date**: January 15, 2026
**Task**: Task 12 - Organize KV Storage Location
**Changes**:
- Created `data/KVS/Employment/` directory
- Moved all 54 existing KV directories from `data/KVS/` to `data/KVS/Employment/`
- Updated `appsettings.json` and `appsettings.Development.json` to point to new location
- All tests pass after migration
