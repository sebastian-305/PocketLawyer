# Schema Fetch Utility

## Overview

The `fetchSchema` function in `network.js` provides a simple, cached interface for fetching JSON schemas from the backend API.

## Usage

```javascript
import { fetchSchema } from './network.js';

// Fetch a schema
const schema = await fetchSchema('Rental', 'Basic', 'General');

if (schema) {
    console.log('Schema type:', schema.type);
    console.log('Properties:', schema.properties);
    console.log('Required fields:', schema.required);
    
    // Access x-styling metadata
    const vertragstyp = schema.properties.vertragstyp;
    if (vertragstyp['x-styling']) {
        console.log('Display class:', vertragstyp['x-styling'].displayClass);
        console.log('Importance:', vertragstyp['x-styling'].importance);
    }
} else {
    console.log('Schema not found');
}
```

## Function Signature

```javascript
async function fetchSchema(contractType, tier, step)
```

### Parameters

- `contractType` (string): Type of contract (e.g., "Rental", "Employment")
- `tier` (string): Analysis tier ("Basic" or "Premium")
- `step` (string): Analysis step name (e.g., "General", "Costs", "Rent")

### Returns

- `Promise<object|null>`: The JSON schema object with x-title and x-styling metadata, or null if not found

## Features

### 1. Automatic Caching

Schemas are automatically cached in memory after the first fetch. Subsequent requests for the same schema return the cached version instantly.

```javascript
// First call - fetches from API
const schema1 = await fetchSchema('Rental', 'Basic', 'General'); // ~50ms

// Second call - returns from cache
const schema2 = await fetchSchema('Rental', 'Basic', 'General'); // <1ms
```

### 2. Graceful 404 Handling

When a schema is not found (404), the function returns `null` and caches this result to avoid repeated failed requests.

```javascript
const schema = await fetchSchema('Invalid', 'Invalid', 'Invalid');
console.log(schema); // null
```

### 3. Error Handling

Network errors and other failures are caught and logged (when DEBUG is enabled). The function returns `null` but does NOT cache errors, allowing retry on the next call.

```javascript
try {
    const schema = await fetchSchema('Rental', 'Basic', 'General');
    if (schema) {
        // Use schema
    } else {
        // Handle not found
    }
} catch (error) {
    // This won't happen - errors are handled internally
}
```

## Available Schemas

### Rental Contract - Basic Analysis

- `fetchSchema('Rental', 'Basic', 'General')` - General contract data
- `fetchSchema('Rental', 'Basic', 'Rent')` - Rent conditions
- `fetchSchema('Rental', 'Basic', 'Conditions')` - Contract conditions
- `fetchSchema('Rental', 'Basic', 'Summary')` - Basic analysis summary

### Rental Contract - Premium Analysis

- `fetchSchema('Rental', 'Premium', 'Costs')` - Costs and payments
- `fetchSchema('Rental', 'Premium', 'Terms')` - Terms and termination
- `fetchSchema('Rental', 'Premium', 'Use')` - Usage rights
- `fetchSchema('Rental', 'Premium', 'Maintenance')` - Maintenance obligations
- `fetchSchema('Rental', 'Premium', 'Duties')` - Rights and duties
- `fetchSchema('Rental', 'Premium', 'Privacy')` - Privacy and communication
- `fetchSchema('Rental', 'Premium', 'Summary')` - Premium analysis summary

## Schema Structure

All schemas follow this structure:

```json
{
  "type": "object",
  "additionalProperties": false,
  "properties": {
    "fieldName": {
      "type": "string",
      "description": "Field description",
      "x-styling": {
        "displayClass": "font-bold",
        "importance": "high"
      }
    }
  },
  "required": ["fieldName"]
}
```

## Testing

A test page is available at `/test-schema-fetch.html` to verify the functionality:

1. Start the application
2. Navigate to `http://localhost:5000/test-schema-fetch.html`
3. Click the test buttons to verify:
   - Valid schema fetching
   - Invalid schema handling (404)
   - Caching behavior
   - Multiple schema fetching

## Debug Mode

To enable debug logging, set `DEBUG = true` in `network.js`:

```javascript
const DEBUG = true; // Enable debug logging
```

This will log:
- Cache hits
- Schema fetches
- Errors
- Warnings for 404s

## Implementation Details

- **Cache Key Format**: `{contractType}_{tier}_{step}` (e.g., "Rental_Basic_General")
- **Cache Storage**: JavaScript `Map` object (in-memory, cleared on page reload)
- **404 Caching**: Yes (to avoid repeated failed requests)
- **Error Caching**: No (allows retry on next call)
- **API Endpoint**: `/api/schemas/{contractType}/{tier}/{step}`

## Requirements Validation

✅ Create `fetchSchema(contractType, tier, step)` function in network.js
✅ Function calls `/api/schemas/{contractType}/{tier}/{step}` endpoint
✅ Returns schema with x-title and x-styling metadata
✅ Cache schemas in memory to avoid repeated requests
✅ Handle 404 errors gracefully

## Next Steps

This utility will be used by the generic dynamic renderer (task 15.3) to fetch schemas and render analysis results dynamically based on the schema metadata.
