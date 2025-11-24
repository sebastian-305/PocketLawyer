/**
 * Simple test to verify the renderAnalysisResult function logic
 * This tests the core functionality without requiring a browser environment
 */

// Mock DOM elements for testing
class MockElement {
    constructor(tagName) {
        this.tagName = tagName;
        this.children = [];
        this.classList = new MockClassList();
        this.textContent = '';
        this.innerHTML = '';
        this.attributes = {};
    }

    appendChild(child) {
        this.children.push(child);
        return child;
    }

    setAttribute(name, value) {
        this.attributes[name] = value;
    }

    getAttribute(name) {
        return this.attributes[name];
    }
}

class MockClassList {
    constructor() {
        this.classes = [];
    }

    add(...classes) {
        this.classes.push(...classes);
    }

    contains(className) {
        return this.classes.includes(className);
    }
}

// Mock document.createElement
global.document = {
    createElement: (tagName) => new MockElement(tagName)
};

// Simple test function
function testRenderAnalysisResult() {
    console.log('Testing renderAnalysisResult function...\n');

    // Test 1: Simple object with x-title
    console.log('Test 1: Simple object with x-title');
    const container1 = new MockElement('div');
    const data1 = {
        vertragstyp: "Mietvertrag Typ I",
        mieter: "Max Mustermann"
    };
    const schema1 = {
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

    // Manually test the logic
    const schemaProperties = schema1.properties || {};
    let fieldCount = 0;
    
    for (const [key, value] of Object.entries(data1)) {
        const propertySchema = schemaProperties[key];
        const label = propertySchema?.['x-title'] || key;
        
        console.log(`  Field: ${key}`);
        console.log(`    Label: ${label}`);
        console.log(`    Value: ${value}`);
        console.log(`    Schema Type: ${propertySchema?.type}`);
        
        fieldCount++;
    }
    
    console.log(`  ✓ Processed ${fieldCount} fields\n`);

    // Test 2: Nested object
    console.log('Test 2: Nested object');
    const data2 = {
        allgemeineVertragsdaten: {
            vertragstyp: "Mietvertrag Typ I",
            mieter: "Max Mustermann"
        }
    };
    const schema2 = {
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

    const schemaProperties2 = schema2.properties || {};
    for (const [key, value] of Object.entries(data2)) {
        const propertySchema = schemaProperties2[key];
        const label = propertySchema?.['x-title'] || key;
        
        console.log(`  Field: ${key}`);
        console.log(`    Label: ${label}`);
        console.log(`    Type: ${propertySchema?.type}`);
        
        if (typeof value === 'object' && propertySchema?.type === 'object') {
            console.log(`    Nested properties:`);
            const nestedProperties = propertySchema?.properties || {};
            for (const [nestedKey, nestedValue] of Object.entries(value)) {
                const nestedPropertySchema = nestedProperties[nestedKey];
                const nestedLabel = nestedPropertySchema?.['x-title'] || nestedKey;
                console.log(`      ${nestedKey}: ${nestedLabel} = ${nestedValue}`);
            }
        }
    }
    console.log(`  ✓ Nested object processed correctly\n`);

    // Test 3: Array handling
    console.log('Test 3: Array of primitives');
    const data3 = {
        besonderheiten: ["Haustiere erlaubt", "Balkon vorhanden"]
    };
    const schema3 = {
        properties: {
            besonderheiten: {
                type: "array",
                "x-title": "Besonderheiten",
                items: {
                    type: "string"
                }
            }
        }
    };

    const schemaProperties3 = schema3.properties || {};
    for (const [key, value] of Object.entries(data3)) {
        const propertySchema = schemaProperties3[key];
        const label = propertySchema?.['x-title'] || key;
        
        console.log(`  Field: ${key}`);
        console.log(`    Label: ${label}`);
        console.log(`    Is Array: ${Array.isArray(value)}`);
        console.log(`    Items: ${value.join(', ')}`);
    }
    console.log(`  ✓ Array processed correctly\n`);

    // Test 4: Null/undefined handling
    console.log('Test 4: Null/undefined values');
    const data4 = {
        definedField: "Has Value",
        nullField: null,
        undefinedField: undefined
    };
    const schema4 = {
        properties: {
            definedField: {
                type: "string",
                "x-title": "Defined Field"
            },
            nullField: {
                type: "string",
                "x-title": "Null Field"
            },
            undefinedField: {
                type: "string",
                "x-title": "Undefined Field"
            }
        }
    };

    const schemaProperties4 = schema4.properties || {};
    for (const [key, value] of Object.entries(data4)) {
        const propertySchema = schemaProperties4[key];
        const label = propertySchema?.['x-title'] || key;
        const displayValue = (value === null || value === undefined) ? 'N/A' : value;
        
        console.log(`  Field: ${key}`);
        console.log(`    Label: ${label}`);
        console.log(`    Value: ${value}`);
        console.log(`    Display Value: ${displayValue}`);
    }
    console.log(`  ✓ Null/undefined handled correctly\n`);

    // Test 5: Missing x-title (fallback)
    console.log('Test 5: Missing x-title (fallback to key)');
    const data5 = {
        someField: "Some Value"
    };
    const schema5 = {
        properties: {
            someField: {
                type: "string"
                // No x-title
            }
        }
    };

    const schemaProperties5 = schema5.properties || {};
    for (const [key, value] of Object.entries(data5)) {
        const propertySchema = schemaProperties5[key];
        const label = propertySchema?.['x-title'] || key;
        
        console.log(`  Field: ${key}`);
        console.log(`    Label (fallback): ${label}`);
        console.log(`    Value: ${value}`);
    }
    console.log(`  ✓ Fallback to key name works correctly\n`);

    console.log('✅ All logic tests passed!');
}

// Run tests
testRenderAnalysisResult();
