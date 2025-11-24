# Generic Render Function Documentation

## Overview

The `renderAnalysisResult` function provides a generic, schema-driven way to render analysis results dynamically. It uses JSON schema metadata (specifically `x-title` fields) to create human-readable labels for data fields.

## Function Signature

```javascript
export function renderAnalysisResult(data, schema, container)
```

### Parameters

- **data** (object): The analysis result data to render
- **schema** (object): The JSON schema with x-title metadata for field labels
- **container** (HTMLElement): The DOM element to render into

## Features

### 1. Schema-Driven Rendering
- Uses `x-title` from schema for field labels
- Falls back to property key name if `x-title` is not available
- Respects schema type information for proper rendering

### 2. Recursive Nested Object Support
- Handles deeply nested object structures
- Recursively processes nested properties using their schema definitions
- Maintains proper hierarchy in the rendered output

### 3. Array Handling
- Renders arrays of primitive values as lists
- Renders arrays of objects with full schema support
- Handles mixed array content gracefully

### 4. Null/Undefined Handling
- Displays "N/A" for null or undefined values
- Prevents rendering errors from missing data

### 5. Type Detection
- Automatically detects and handles:
  - Primitive types (string, number, boolean)
  - Objects (nested structures)
  - Arrays (both primitive and object arrays)

## Usage Examples

### Example 1: Simple Object

```javascript
import { renderAnalysisResult } from './render.js';

const data = {
    vertragstyp: "Mietvertrag Typ I",
    mieter: "Max Mustermann"
};

const schema = {
    type: "object",
    properties: {
        vertragstyp: {
            type: "string",
            "x-title": "Vertragstyp"
        },
        mieter: {
            type: "string",
            "x-title": "Mieter"
        }
    }
};

const container = document.getElementById('result');
renderAnalysisResult(data, schema, container);
```

### Example 2: Nested Object

```javascript
const data = {
    allgemeineVertragsdaten: {
        vertragstyp: "Mietvertrag Typ I",
        mieter: "Max Mustermann"
    }
};

const schema = {
    type: "object",
    properties: {
        allgemeineVertragsdaten: {
            type: "object",
            "x-title": "Allgemeine Vertragsdaten",
            properties: {
                vertragstyp: {
                    type: "string",
                    "x-title": "Vertragstyp"
                },
                mieter: {
                    type: "string",
                    "x-title": "Mieter"
                }
            }
        }
    }
};

renderAnalysisResult(data, schema, container);
```

### Example 3: Array of Objects

```javascript
const data = {
    rechtsgrundlage: [
        { rechtsgrundlage: "§ 1 MRG", wortlaut: "Mietrechtsgesetz Paragraph 1" },
        { rechtsgrundlage: "§ 2 MRG", wortlaut: "Mietrechtsgesetz Paragraph 2" }
    ]
};

const schema = {
    type: "object",
    properties: {
        rechtsgrundlage: {
            type: "array",
            "x-title": "Rechtsgrundlage",
            items: {
                type: "object",
                properties: {
                    rechtsgrundlage: {
                        type: "string",
                        "x-title": "Rechtsgrundlage"
                    },
                    wortlaut: {
                        type: "string",
                        "x-title": "Wortlaut"
                    }
                }
            }
        }
    }
};

renderAnalysisResult(data, schema, container);
```

## CSS Classes

The function adds the following CSS classes to rendered elements:

- `.field-item` - Applied to all field wrappers
- `.field-value` - Applied to primitive value containers
- `.field-array` - Applied to array field wrappers
- `.field-array-list` - Applied to the `<ul>` element containing array items
- `.field-object` - Applied to nested object field wrappers
- `.field-nested` - Applied to nested object containers

## Integration with Schema Endpoint

The function is designed to work seamlessly with the schema endpoint:

```javascript
import { fetchSchema } from './network.js';
import { renderAnalysisResult } from './render.js';

// Fetch schema from backend
const schema = await fetchSchema('Rental', 'Basic', 'General');

// Render analysis result
const container = document.getElementById('result');
renderAnalysisResult(analysisData, schema, container);
```

## Future Enhancements

The following features are planned for future versions:

1. **x-styling Support**: Process `x-styling` metadata for visual styling (colors, icons, emphasis)
2. **Accordion Integration**: Automatic accordion structure for complex nested data
3. **Custom Renderers**: Support for custom rendering functions for specific field types
4. **Validation Feedback**: Visual indicators for validation errors or warnings

## Testing

Test files are available:

- `test-render-generic.html` - Browser-based visual tests
- `test-render-logic.mjs` - Node.js logic tests

Run the logic tests:
```bash
node wwwroot/test-render-logic.mjs
```

Open the visual tests in a browser:
```
http://localhost:5000/test-render-generic.html
```

## Requirements Validation

This implementation satisfies the following task requirements:

✅ Create `renderAnalysisResult(data, schema, container)` function in render.js  
✅ Function dynamically builds UI from JSON structure using schema metadata  
✅ Use x-title from schema for field labels  
✅ Handle nested objects recursively  
✅ No x-styling information processing yet (future enhancement)  

## Related Files

- `wwwroot/render.js` - Main implementation
- `wwwroot/network.js` - Schema fetching utilities
- `wwwroot/test-render-generic.html` - Visual tests
- `wwwroot/test-render-logic.mjs` - Logic tests
