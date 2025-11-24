# Tier Accordion Visual Demonstration

## Overview

This document provides a visual guide to the tier accordion implementation for the PocketLawyer analysis results.

## Visual Structure

```
┌─────────────────────────────────────────────────────────────┐
│  DEINE VERTRAGSANALYSE                                      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ ▼ BASIS ANALYSE                                       │ │ ← Expanded by default
│  ├───────────────────────────────────────────────────────┤ │
│  │                                                       │ │
│  │  ┌─────────────────────────────────────────────────┐ │ │
│  │  │ Allgemeine Vertragsdaten                        │ │ │
│  │  │ • Vertragstyp: Mietvertrag                      │ │ │
│  │  │ • Mieter: Max Mustermann                        │ │ │
│  │  │ • Vermieter: Immobilien GmbH                    │ │ │
│  │  └─────────────────────────────────────────────────┘ │ │
│  │                                                       │ │
│  │  ┌─────────────────────────────────────────────────┐ │ │
│  │  │ Mietkonditionen                                 │ │ │
│  │  │ • Kaltmiete: 800 EUR                            │ │ │
│  │  │ • Nebenkosten: 150 EUR                          │ │ │
│  │  │ • Warmmiete: 950 EUR                            │ │ │
│  │  └─────────────────────────────────────────────────┘ │ │
│  │                                                       │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ ▶ PREMIUM ANALYSE                                     │ │ ← Collapsed by default
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## When Premium is Expanded

```
┌─────────────────────────────────────────────────────────────┐
│  DEINE VERTRAGSANALYSE                                      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ ▼ BASIS ANALYSE                                       │ │
│  ├───────────────────────────────────────────────────────┤ │
│  │ [Basic analysis content...]                           │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ ▼ PREMIUM ANALYSE                                     │ │ ← Now expanded
│  ├───────────────────────────────────────────────────────┤ │
│  │                                                       │ │
│  │  ┌─────────────────────────────────────────────────┐ │ │
│  │  │ Mietzahlung und Nebenkosten                     │ │ │
│  │  │ • Zusammenfassung: Transparent                  │ │ │
│  │  │ • Bewertung: Zulässig                           │ │ │
│  │  │ • Risiko Score: Grün                            │ │ │
│  │  └─────────────────────────────────────────────────┘ │ │
│  │                                                       │ │
│  │  ┌─────────────────────────────────────────────────┐ │ │
│  │  │ Mietdauer und Kündigung                         │ │ │
│  │  │ • Zusammenfassung: Gesetzeskonform              │ │ │
│  │  │ • Bewertung: Zulässig                           │ │ │
│  │  │ • Risiko Score: Grün                            │ │ │
│  │  └─────────────────────────────────────────────────┘ │ │
│  │                                                       │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Color Scheme

### Tier-Level Accordions (Basic/Premium)
- **Background**: `var(--neutral-bg)` (#e9e6e0)
- **Text**: `var(--blue-bg)` (#2d3669)
- **Border**: 2px solid `var(--paper-bg)` (#dbd2c1)
- **Font**: 18px, weight 800, uppercase

### Section-Level Accordions (nested content)
- **Background**: `var(--paper-bg)` (#dbd2c1)
- **Text**: Black
- **Font**: 16px, weight bold, uppercase

## Interaction States

### Expanded State
```html
<button class="accordion-button" aria-expanded="true">
    BASIS ANALYSE
</button>
<div class="accordion-collapse collapse show">
    <!-- Content visible -->
</div>
```

### Collapsed State
```html
<button class="accordion-button collapsed" aria-expanded="false">
    PREMIUM ANALYSE
</button>
<div class="accordion-collapse collapse">
    <!-- Content hidden -->
</div>
```

## User Interactions

1. **Click on "BASIS ANALYSE" header**
   - Collapses the Basic Analysis section
   - Arrow changes from ▼ to ▶
   - Content slides up with smooth animation

2. **Click on "PREMIUM ANALYSE" header**
   - Expands the Premium Analysis section
   - Arrow changes from ▶ to ▼
   - Content slides down with smooth animation

3. **Both sections can be open simultaneously**
   - Independent collapse/expand behavior
   - No mutual exclusivity

## Accessibility Features

### Keyboard Navigation
- **Tab**: Move focus between accordion headers
- **Enter/Space**: Toggle accordion expand/collapse
- **Arrow keys**: Navigate between focusable elements

### Screen Reader Support
- `aria-expanded`: Announces whether section is expanded or collapsed
- `aria-controls`: Links button to controlled content
- Semantic `<h2>` headers for proper document outline
- `type="button"` ensures proper button semantics

### Focus Management
- Visible focus indicators on accordion buttons
- Focus remains on button after toggle
- Logical tab order through content

## Responsive Behavior

The tier accordions are fully responsive:

- **Desktop**: Full width with comfortable padding
- **Tablet**: Maintains structure, adjusts padding
- **Mobile**: Stacks vertically, touch-friendly targets

## Code Example

```javascript
import { createTierAccordion } from './render.js';

// Prepare content
const basicContent = document.createElement('div');
basicContent.innerHTML = '<p>Basic analysis data...</p>';

const premiumContent = document.createElement('div');
premiumContent.innerHTML = '<p>Premium analysis data...</p>';

// Create accordions
const basicAccordion = createTierAccordion(
    'tier-basic',           // Unique ID
    'Basis Analyse',        // Title
    basicContent,           // Content
    true                    // Expanded by default
);

const premiumAccordion = createTierAccordion(
    'tier-premium',         // Unique ID
    'Premium Analyse',      // Title
    premiumContent,         // Content
    false                   // Collapsed by default
);

// Add to page
const container = document.getElementById('evaluation-body');
container.classList.add('tier-accordion');
container.appendChild(basicAccordion);
container.appendChild(premiumAccordion);
```

## Testing the Implementation

1. Open `wwwroot/test-tier-accordion.html` in a browser
2. Observe the initial state:
   - ✅ Basic Analysis is expanded
   - ✅ Premium Analysis is collapsed
3. Click on "BASIS ANALYSE" header:
   - ✅ Section collapses smoothly
   - ✅ Arrow icon changes
4. Click on "PREMIUM ANALYSE" header:
   - ✅ Section expands smoothly
   - ✅ Arrow icon changes
5. Verify both can be open at the same time
6. Check test results panel shows all tests passing

## Integration with Existing Code

The tier accordion implementation is designed to work seamlessly with:

- Existing render functions (render-rent.js, render-employment.js)
- Generic renderer (task 15.5+)
- WebSocket message handling (task 15.7)
- Schema-driven rendering (task 15.8)

No changes to existing code are required until integration tasks begin.
