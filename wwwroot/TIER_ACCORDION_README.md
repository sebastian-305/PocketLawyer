# Tier Accordion Implementation

## Overview

This document describes the implementation of the tier-level accordion structure for analysis results. The accordion provides a two-tier hierarchy:

1. **Tier Level**: "Basis Analyse" and "Premium Analyse" (main accordions)
2. **Section Level**: Individual data sections within each tier (nested accordions)

## Implementation

### Function: `createTierAccordion()`

Located in `wwwroot/render.js`

```javascript
createTierAccordion(id, title, content, expanded = true)
```

**Parameters:**
- `id` (string): Unique identifier for the accordion item
- `title` (string): Display title in the accordion header
- `content` (HTMLElement): Content to display in the accordion body
- `expanded` (boolean): Whether the accordion should be expanded by default (default: true)

**Returns:**
- HTMLElement: A Bootstrap accordion item element

### Usage Example

```javascript
import { createTierAccordion } from './render.js';

// Create content for Basic Analysis
const basicContent = document.createElement('div');
basicContent.innerHTML = '<p>Basic analysis data...</p>';

// Create content for Premium Analysis
const premiumContent = document.createElement('div');
premiumContent.innerHTML = '<p>Premium analysis data...</p>';

// Create tier accordions
const basicAccordion = createTierAccordion(
    'tier-basic',
    'Basis Analyse',
    basicContent,
    true  // expanded by default
);

const premiumAccordion = createTierAccordion(
    'tier-premium',
    'Premium Analyse',
    premiumContent,
    false  // collapsed by default
);

// Add to container
const container = document.getElementById('analysis-container');
container.classList.add('tier-accordion');
container.appendChild(basicAccordion);
container.appendChild(premiumAccordion);
```

## Structure

The generated HTML structure follows Bootstrap 5 accordion conventions:

```html
<div class="accordion-item">
    <h2 class="accordion-header">
        <button class="accordion-button [collapsed]"
                type="button"
                data-bs-toggle="collapse"
                data-bs-target="#tier-basic"
                aria-expanded="true|false"
                aria-controls="tier-basic">
            Basis Analyse
        </button>
    </h2>
    <div id="tier-basic" 
         class="accordion-collapse collapse [show]">
        <div class="accordion-body">
            <!-- Content goes here -->
        </div>
    </div>
</div>
```

## Styling

### Tier-Level Accordion Styles

The tier-level accordions have distinct styling to differentiate them from nested section accordions:

```css
.tier-accordion .accordion-item {
    background-color: var(--neutral-bg);
    border: 2px solid var(--paper-bg);
}

.tier-accordion .accordion-button {
    background-color: var(--neutral-bg);
    color: var(--blue-bg);
    font-size: 18px;
    font-weight: 800;
    text-transform: uppercase;
    padding: 1.25rem;
}
```

### Section-Level Accordion Styles

Nested accordions within tiers maintain the existing styling:

```css
.accordion-button {
    background-color: var(--paper-bg);
    color: black;
    font-size: 16px;
    font-weight: bold;
    text-transform: uppercase;
}
```

## Accessibility

The implementation includes proper ARIA attributes for accessibility:

- `aria-expanded`: Indicates whether the accordion is expanded or collapsed
- `aria-controls`: Links the button to the collapse element
- `type="button"`: Ensures proper button semantics
- Semantic HTML with `<h2>` headers for proper document structure

## Testing

### Manual Testing

Open `wwwroot/test-tier-accordion.html` in a browser to:
1. Verify accordion structure is created correctly
2. Test expand/collapse functionality
3. Verify Basic Analysis is expanded by default
4. Verify Premium Analysis is collapsed by default
5. Check visual styling and hierarchy

### Automated Tests

The test file includes assertions for:
- Accordion creation
- Proper class assignments
- ARIA attribute correctness
- Expand/collapse state
- Button attributes and targeting
- Content nesting

## Requirements Validation

This implementation satisfies task 15.4 requirements:

✅ Create two main accordion sections: "Basis Analyse" and "Premium Analyse"
✅ Each section should be collapsible/expandable
✅ Use Bootstrap accordion component for consistency
✅ Basic analysis should be expanded by default
✅ Premium analysis should be collapsed by default

## Integration Notes

### Future Integration with Generic Renderer

When integrating with the generic renderer (task 15.5+):

1. Wrap the entire analysis result in a tier accordion container:
   ```javascript
   const container = document.getElementById('evaluation-body');
   container.classList.add('tier-accordion');
   ```

2. Create separate content containers for Basic and Premium results:
   ```javascript
   const basicContainer = document.createElement('div');
   const premiumContainer = document.createElement('div');
   ```

3. Render analysis data into respective containers using `renderAnalysisResult()`

4. Wrap each container in a tier accordion:
   ```javascript
   const basicAccordion = createTierAccordion('tier-basic', 'Basis Analyse', basicContainer, true);
   const premiumAccordion = createTierAccordion('tier-premium', 'Premium Analyse', premiumContainer, false);
   ```

5. Append to main container:
   ```javascript
   container.appendChild(basicAccordion);
   container.appendChild(premiumAccordion);
   ```

## Browser Compatibility

The implementation uses:
- Bootstrap 5.3.2 accordion component
- Standard DOM APIs
- ES6 modules

Compatible with all modern browsers that support Bootstrap 5.
